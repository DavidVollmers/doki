﻿using System.Text.Json.Serialization;

namespace Doki.Output;

public abstract record OutputOptions
{
    [JsonPropertyName("outputPath")] public string? OutputPath { get; init; }
    
    public static OutputOptions Default { get; } = new DefaultOutputOptions();
}