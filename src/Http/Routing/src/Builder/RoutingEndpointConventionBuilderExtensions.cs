// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Extension methods for adding routing metadata to endpoint instances using <see cref="IEndpointConventionBuilder"/>.
/// </summary>
public static class RoutingEndpointConventionBuilderExtensions
{
    private const bool BufferBodyDefault = false;
    private const int MemoryBufferThresholdDefault = 1024 * 64;
    private const int BufferBodyLengthLimitDefault = 1024 * 1024 * 128;
    private const int ValueCountLimitDefault = 1024;
    private const int KeyLengthLimitDefault = 1024 * 2;
    private const int ValueLengthLimit = 1024 * 1024 * 4;
    private const int MultipartBoundaryLengthLimitDefault = 128;
    private const int MultipartHeadersCountLimitDefault = 16;
    private const int MultipartHeadersLengthLimitDefault = 1024 * 16;
    private const long MultipartBodyLengthLimitDefault = 1024 * 1024 * 128;
    private const int MaxCollectionSizeDefault = 1024;
    private const int MaxRecursionDepthDefault = 64;
    private const int MaxKeySizeDefault = 1024 * 2;

    /// <summary>
    /// Requires that endpoints match one of the specified hosts during routing.
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointConventionBuilder"/> to add the metadata to.</param>
    /// <param name="hosts">
    /// The hosts used during routing.
    /// Hosts should be Unicode rather than punycode, and may have a port.
    /// An empty collection means any host will be accepted.
    /// </param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static TBuilder RequireHost<TBuilder>(this TBuilder builder, params string[] hosts) where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(hosts);

        builder.Add(endpointBuilder =>
        {
            endpointBuilder.Metadata.Add(new HostAttribute(hosts));
        });
        return builder;
    }

    /// <summary>
    /// Sets the <see cref="EndpointBuilder.DisplayName"/> to the provided <paramref name="displayName"/> for all
    /// builders created by <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointConventionBuilder"/>.</param>
    /// <param name="displayName">The display name.</param>
    /// <returns>The <see cref="IEndpointConventionBuilder"/>.</returns>
    public static TBuilder WithDisplayName<TBuilder>(this TBuilder builder, string displayName) where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Add(b =>
        {
            b.DisplayName = displayName;
        });

        return builder;
    }

    /// <summary>
    /// Sets the <see cref="EndpointBuilder.DisplayName"/> using the provided <paramref name="func"/> for all
    /// builders created by <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointConventionBuilder"/>.</param>
    /// <param name="func">A delegate that produces the display name for each <see cref="EndpointBuilder"/>.</param>
    /// <returns>The <see cref="IEndpointConventionBuilder"/>.</returns>
    public static TBuilder WithDisplayName<TBuilder>(this TBuilder builder, Func<EndpointBuilder, string> func) where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(func);

        builder.Add(b =>
        {
            b.DisplayName = func(b);
        });

        return builder;
    }

    /// <summary>
    /// Adds the provided metadata <paramref name="items"/> to <see cref="EndpointBuilder.Metadata"/> for all builders
    /// produced by <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointConventionBuilder"/>.</param>
    /// <param name="items">A collection of metadata items.</param>
    /// <returns>The <see cref="IEndpointConventionBuilder"/>.</returns>
    public static TBuilder WithMetadata<TBuilder>(this TBuilder builder, params object[] items) where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(items);

        builder.Add(b =>
        {
            foreach (var item in items)
            {
                b.Metadata.Add(item);
            }
        });

        return builder;
    }

    /// <summary>
    /// Adds the <see cref="IEndpointNameMetadata"/> to the Metadata collection for all endpoints produced
    /// on the target <see cref="IEndpointConventionBuilder"/> given the <paramref name="endpointName" />.
    /// The <see cref="IEndpointNameMetadata" /> on the endpoint is used for link generation and
    /// is treated as the operation ID in the given endpoint's OpenAPI specification.
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointConventionBuilder"/>.</param>
    /// <param name="endpointName">The endpoint name.</param>
    /// <returns>The <see cref="IEndpointConventionBuilder"/>.</returns>
    public static TBuilder WithName<TBuilder>(this TBuilder builder, string endpointName) where TBuilder : IEndpointConventionBuilder
    {
        builder.WithMetadata(new EndpointNameMetadata(endpointName), new RouteNameMetadata(endpointName));
        return builder;
    }

    /// <summary>
    /// Sets the <see cref="EndpointGroupNameAttribute"/> for all endpoints produced
    /// on the target <see cref="IEndpointConventionBuilder"/> given the <paramref name="endpointGroupName" />.
    /// The <see cref="IEndpointGroupNameMetadata" /> on the endpoint is used to set the endpoint's
    /// GroupName in the OpenAPI specification.
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointConventionBuilder"/>.</param>
    /// <param name="endpointGroupName">The endpoint group name.</param>
    /// <returns>The <see cref="IEndpointConventionBuilder"/>.</returns>
    public static TBuilder WithGroupName<TBuilder>(this TBuilder builder, string endpointGroupName) where TBuilder : IEndpointConventionBuilder
    {
        builder.WithMetadata(new EndpointGroupNameAttribute(endpointGroupName));
        return builder;
    }

    /// <summary>
    /// Disables anti-forgery token validation for all endpoints produced on
    /// the target <see cref="IEndpointConventionBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointConventionBuilder"/>.</param>
    /// <returns>The <see cref="IEndpointConventionBuilder"/>.</returns>
    public static TBuilder DisableAntiforgery<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.WithMetadata(AntiforgeryMetadata.ValidationNotRequired);
        return builder;
    }

    /// <summary>
    /// Configures <see cref="FormMappingOptionsMetadata"/> for all endpoints produced
    /// on the target <see cref="IEndpointConventionBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointConventionBuilder"/>.</param>
    /// <param name="maxCollectionSize">The maximum number of elements allowed in a form collection.</param>
    /// <param name="maxRecursionDepth">The maximum depth allowed when recursively mapping form data.</param>
    /// <param name="maxKeySize">The maximum size of the buffer used to read form data keys.</param>
    /// <returns>The <see cref="IEndpointConventionBuilder"/>.</returns>
    public static TBuilder WithFormMappingOptions<TBuilder>(
        this TBuilder builder,
        int maxCollectionSize = MaxCollectionSizeDefault,
        int maxRecursionDepth = MaxRecursionDepthDefault,
        int maxKeySize = MaxKeySizeDefault) where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.WithMetadata(new FormMappingOptionsMetadata(maxCollectionSize, maxRecursionDepth, maxKeySize));
        return builder;
    }

    /// <summary>
    /// Configures <see cref="RequestFormLimitsMetadata"/> for all endpoints produced
    /// on the target <see cref="IEndpointConventionBuilder"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IEndpointConventionBuilder"/>.</param>
    /// <param name="bufferBody">Enables full request body buffering.</param>
    /// <param name="memoryBufferThreshold">Configures how many bytes of the body will be buffered in memory.</param>
    /// <param name="bufferBodyLengthLimit">Limit for the total number of bytes that will be buffered.</param>
    /// <param name="valueCountLimit">Limit for the number of form entries to allow.</param>
    /// <param name="keyLengthLimit">Limit on the length of individual keys.</param>
    /// <param name="valueLengthLimit">Limit on the length of individual form values.</param>
    /// <param name="multipartBoundaryLengthLimit">Limit for the length of the boundary identifier.</param>
    /// <param name="multipartHeadersCountLimit">Limit for the number of headers to allow in each multipart section.</param>
    /// <param name="multipartHeadersLengthLimit">Limit for the total length of the header keys and values in each multipart section.</param>
    /// <param name="multipartBodyLengthLimit">Limit for the length of each multipart body.</param>
    /// <returns>The <see cref="IEndpointConventionBuilder"/>.</returns>
    public static TBuilder WithRequestFormLimits<TBuilder>(
        this TBuilder builder,
        bool bufferBody = BufferBodyDefault,
        int memoryBufferThreshold = MemoryBufferThresholdDefault,
        long bufferBodyLengthLimit = BufferBodyLengthLimitDefault,
        int valueCountLimit = ValueCountLimitDefault,
        int keyLengthLimit = KeyLengthLimitDefault,
        int valueLengthLimit = ValueLengthLimit,
        int multipartBoundaryLengthLimit = MultipartBoundaryLengthLimitDefault,
        int multipartHeadersCountLimit = MultipartHeadersCountLimitDefault,
        int multipartHeadersLengthLimit = MultipartHeadersLengthLimitDefault,
        long multipartBodyLengthLimit = MultipartBodyLengthLimitDefault) where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.WithMetadata(new RequestFormLimitsMetadata(bufferBody, memoryBufferThreshold, bufferBodyLengthLimit, valueCountLimit, keyLengthLimit, valueLengthLimit, multipartBoundaryLengthLimit, multipartHeadersCountLimit, multipartHeadersLengthLimit, multipartBodyLengthLimit));
        return builder;
    }
}
