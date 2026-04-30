using System;
using System.Collections.Generic;

namespace SSMSCommandPalette.Models;

public enum SsmsErrorKind
{
    None,
    SsmsExeMissing,
    SettingsFileMissing,
    NoConnectionsFound,
    ParseFailed,
    Unknown,
}

public sealed class SsmsQueryResult
{
    public IReadOnlyList<SsmsConnection> Items { get; init; } = Array.Empty<SsmsConnection>();

    public string SsmsExePath { get; init; } = string.Empty;

    public SsmsErrorKind ErrorKind { get; init; }
    public string ErrorTitle { get; init; } = string.Empty;
    public string ErrorDescription { get; init; } = string.Empty;

    public bool HasError => ErrorKind != SsmsErrorKind.None;
}
