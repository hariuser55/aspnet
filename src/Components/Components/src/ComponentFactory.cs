// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.AspNetCore.Components.Reflection;
using Microsoft.AspNetCore.Components.Rendering;
using static Microsoft.AspNetCore.Internal.LinkerFlags;

namespace Microsoft.AspNetCore.Components;

internal sealed class ComponentFactory
{
    private const BindingFlags _injectablePropertyBindingFlags
        = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    private static readonly ConcurrentDictionary<Type, ComponentTypeInfoCacheEntry> _cachedComponentTypeInfo = new();

    private readonly IComponentActivator _componentActivator;
    private readonly RenderModeResolver _renderModeResolver;

    public ComponentFactory(IComponentActivator componentActivator, RenderModeResolver renderModeResolver)
    {
        _componentActivator = componentActivator ?? throw new ArgumentNullException(nameof(componentActivator));
        _renderModeResolver = renderModeResolver ?? throw new ArgumentNullException(nameof(renderModeResolver));
    }

    public static void ClearCache() => _cachedComponentTypeInfo.Clear();

    private static ComponentTypeInfoCacheEntry GetComponentTypeInfo([DynamicallyAccessedMembers(Component)] Type componentType)
    {
        // Unfortunately we can't use 'GetOrAdd' here because the DynamicallyAccessedMembers annotation doesn't flow through to the
        // callback, so it becomes an IL2111 warning. The following is equivalent and thread-safe because it's a ConcurrentDictionary
        // and it doesn't matter if we build a cache entry more than once.
        if (!_cachedComponentTypeInfo.TryGetValue(componentType, out var cacheEntry))
        {
            cacheEntry = new ComponentTypeInfoCacheEntry(GetRenderModeFromComponentType(componentType), CreatePropertyInjector(componentType));
            _cachedComponentTypeInfo.TryAdd(componentType, cacheEntry);
        }

        return cacheEntry;
    }

    public IComponent InstantiateComponent(IServiceProvider serviceProvider, [DynamicallyAccessedMembers(Component)] Type componentType, IComponentRenderMode? callerSpecifiedRenderMode)
    {
        var componentTypeInfo = GetComponentTypeInfo(componentType);
        var component = callerSpecifiedRenderMode is null && componentTypeInfo.RenderMode is null
            ? _componentActivator.CreateInstance(componentType)
            : _renderModeResolver.ResolveComponent(componentType, _componentActivator, componentTypeInfo.RenderMode, callerSpecifiedRenderMode);

        if (component is null)
        {
            // The default activator will never do this, but an externally-supplied one might
            throw new InvalidOperationException($"The component activator returned a null value for a component of type {componentType.FullName}.");
        }

        if (component.GetType() == componentType)
        {
            // Fast, common case: use the cached data we already looked up
            componentTypeInfo.PerformPropertyInjection(serviceProvider, component);
        }
        else
        {
            // Uncommon case where the activator/resolver returned a different type. Needs an extra cache lookup.
            PerformPropertyInjection(serviceProvider, component);
        }

        return component;
    }

    private static void PerformPropertyInjection(IServiceProvider serviceProvider, IComponent instance)
    {
        var componentTypeInfo = GetComponentTypeInfo(instance.GetType());
        componentTypeInfo.PerformPropertyInjection(serviceProvider, instance);
    }

    private static IComponentRenderMode? GetRenderModeFromComponentType(Type componentType)
        => componentType.GetCustomAttribute<RenderModeAttribute>()?.Mode;

    private static Action<IServiceProvider, IComponent> CreatePropertyInjector([DynamicallyAccessedMembers(Component)] Type type)
    {
        // Do all the reflection up front
        List<(string name, Type propertyType, PropertySetter setter)>? injectables = null;
        foreach (var property in MemberAssignment.GetPropertiesIncludingInherited(type, _injectablePropertyBindingFlags))
        {
            if (!property.IsDefined(typeof(InjectAttribute)))
            {
                continue;
            }

            injectables ??= new();
            injectables.Add((property.Name, property.PropertyType, new PropertySetter(type, property)));
        }

        if (injectables is null)
        {
            return static (_, _) => { };
        }

        return Initialize;

        // Return an action whose closure can write all the injected properties
        // without any further reflection calls (just typecasts)
        void Initialize(IServiceProvider serviceProvider, IComponent component)
        {
            foreach (var (propertyName, propertyType, setter) in injectables)
            {
                var serviceInstance = serviceProvider.GetService(propertyType);
                if (serviceInstance == null)
                {
                    throw new InvalidOperationException($"Cannot provide a value for property " +
                        $"'{propertyName}' on type '{type.FullName}'. There is no " +
                        $"registered service of type '{propertyType}'.");
                }

                setter.SetValue(component, serviceInstance);
            }
        }
    }

    // Tracks information about a specific component type that ComponentFactory uses
    private record class ComponentTypeInfoCacheEntry(
        IComponentRenderMode? RenderMode,
        Action<IServiceProvider, IComponent> PerformPropertyInjection);
}
