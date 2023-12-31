// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Components.Endpoints;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// A builder that can be used to configure Server-Side Blazor.
/// </summary>
public interface IServerSideBlazorBuilder : IRazorComponentsBuilder
{
    /// <summary>
    /// Gets the <see cref="IServiceCollection"/>.
    /// </summary>
    public new IServiceCollection Services { get; }

    IServiceCollection IRazorComponentsBuilder.Services => Services;
}
