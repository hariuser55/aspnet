// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
namespace Microsoft.AspNetCore.Http.Metadata;

internal class RequestFormLimitsMetadata(
    bool bufferBody,
    int memoryBufferThreshold,
    long bufferBodyLengthLimit,
    int valueCountLimit,
    int keyLengthLimit,
    int valueLengthLimit,
    int multipartBoundaryLengthLimit,
    int multipartHeadersCountLimit,
    int multipartHeadersLengthLimit,
    long multipartBodyLengthLimit) : IRequestFormLimitsMetadata
{
    public bool BufferBody { get; set; } = bufferBody;
    public int MemoryBufferThreshold { get; set; } = memoryBufferThreshold;
    public long BufferBodyLengthLimit { get; set; } = bufferBodyLengthLimit;
    public int ValueCountLimit { get; set; } = valueCountLimit;
    public int KeyLengthLimit { get; set; } = keyLengthLimit;
    public int ValueLengthLimit { get; set; } = valueLengthLimit;
    public int MultipartBoundaryLengthLimit { get; set; } = multipartBoundaryLengthLimit;
    public int MultipartHeadersCountLimit { get; set; } = multipartHeadersCountLimit;
    public int MultipartHeadersLengthLimit { get; set; } = multipartHeadersLengthLimit;
    public long MultipartBodyLengthLimit { get; set; } = multipartBodyLengthLimit;
}
