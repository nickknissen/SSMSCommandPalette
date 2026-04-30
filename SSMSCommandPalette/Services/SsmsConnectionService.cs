using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SSMSCommandPalette.Models;

namespace SSMSCommandPalette.Services;

public sealed class SsmsConnectionService
{
    private static readonly string[] DefaultExePaths =
    [
        @"C:\Program Files (x86)\Microsoft SQL Server Management Studio 18\Common7\IDE\Ssms.exe",
        @"C:\Program Files\Microsoft SQL Server Management Studio 18\Common7\IDE\Ssms.exe",
        @"C:\Program Files (x86)\Microsoft SQL Server Management Studio 19\Common7\IDE\Ssms.exe",
        @"C:\Program Files\Microsoft SQL Server Management Studio 19\Common7\IDE\Ssms.exe",
        @"C:\Program Files (x86)\Microsoft SQL Server Management Studio 20\Common7\IDE\Ssms.exe",
        @"C:\Program Files\Microsoft SQL Server Management Studio 20\Common7\IDE\Ssms.exe",
        @"C:\Program Files (x86)\Microsoft SQL Server Management Studio 21\Common7\IDE\Ssms.exe",
        @"C:\Program Files\Microsoft SQL Server Management Studio 21\Common7\IDE\Ssms.exe",
    ];

    private readonly string _settingsRoot;

#pragma warning disable CS0649, CS0169 // Unused under DEMO_MODE.
    private readonly object _cacheLock = new();
    private SsmsQueryResult? _cachedResult;
    private string _cachedSettingsPath = string.Empty;
    private DateTime _settingsLastWrite = DateTime.MinValue;
#pragma warning restore CS0649, CS0169

    public SsmsConnectionService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _settingsRoot = Path.Combine(appData, "Microsoft", "SQL Server Management Studio");

#if !DEMO_MODE
        // Warm up the cache so the first GetItems() doesn't block on disk I/O.
        Task.Run(() =>
        {
            try { _ = GetItems(); } catch { }
        });
#endif
    }

#pragma warning disable CA1822 // GetItems is static under DEMO_MODE — keep instance signature for the live path.
    public SsmsQueryResult GetItems()
#pragma warning restore CA1822
    {
#if DEMO_MODE
        return DemoSsmsData.Result();
#else
        var exePath = FindSsmsExe();
        if (string.IsNullOrEmpty(exePath))
        {
            return new SsmsQueryResult
            {
                ErrorKind = SsmsErrorKind.SsmsExeMissing,
                ErrorTitle = "SSMS not found",
                ErrorDescription = "Install SQL Server Management Studio (18, 19, 20, or 21) into the default Program Files location.",
            };
        }

        var settingsPath = FindLatestUserSettings();
        if (settingsPath is null)
        {
            return new SsmsQueryResult
            {
                ErrorKind = SsmsErrorKind.SettingsFileMissing,
                ErrorTitle = "SSMS UserSettings.xml not found",
                ErrorDescription = $"Looked under: {_settingsRoot}",
                SsmsExePath = exePath,
            };
        }

        var timestamp = GetLastWriteTimeUtcSafe(settingsPath);
        lock (_cacheLock)
        {
            if (_cachedResult is not null
                && string.Equals(_cachedSettingsPath, settingsPath, StringComparison.OrdinalIgnoreCase)
                && timestamp == _settingsLastWrite
                && string.Equals(_cachedResult.SsmsExePath, exePath, StringComparison.OrdinalIgnoreCase))
            {
                return _cachedResult;
            }
        }

        var result = LoadFromDisk(settingsPath, exePath);

        lock (_cacheLock)
        {
            _cachedResult = result;
            _cachedSettingsPath = settingsPath;
            _settingsLastWrite = timestamp;
            return _cachedResult;
        }
#endif
    }

    private static SsmsQueryResult LoadFromDisk(string settingsPath, string exePath)
    {
        List<SsmsConnection> connections;
        try
        {
            connections = ParseConnections(File.ReadAllText(settingsPath));
        }
        catch (Exception ex)
        {
            return new SsmsQueryResult
            {
                ErrorKind = SsmsErrorKind.ParseFailed,
                ErrorTitle = "Couldn't read SSMS connections",
                ErrorDescription = ex.Message,
                SsmsExePath = exePath,
            };
        }

        if (connections.Count == 0)
        {
            return new SsmsQueryResult
            {
                ErrorKind = SsmsErrorKind.NoConnectionsFound,
                ErrorTitle = "No SSMS connections found",
                ErrorDescription = "Connect to a server in SSMS to see it here.",
                SsmsExePath = exePath,
            };
        }

        return new SsmsQueryResult
        {
            Items = connections,
            SsmsExePath = exePath,
        };
    }

    // Mirrors raycast-ssms src/lib/user-settings.ts: split the file on
    // <ServerConnectionSettings> blocks and pull Instance / UserName /
    // AuthenticationMethod / Database out of each. Dedupe on (server, user, db).
    private static List<SsmsConnection> ParseConnections(string xml)
    {
        var blocks = xml.Split(["<ServerConnectionSettings>"], StringSplitOptions.None);
        var result = new List<SsmsConnection>();
        if (blocks.Length <= 1)
        {
            return result;
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 1; i < blocks.Length; i++)
        {
            var block = blocks[i];
            var server = ExtractTag(block, "Instance");
            if (string.IsNullOrWhiteSpace(server)) continue;

            var userName = ExtractTag(block, "UserName");
            var database = ExtractTag(block, "Database");
            var authRaw = ExtractTag(block, "AuthenticationMethod");
            var authMethod = int.TryParse(authRaw, out var n) ? n : 0;

            var key = $"{server}|{userName}|{database}";
            if (!seen.Add(key)) continue;

            result.Add(new SsmsConnection
            {
                Server = server,
                UserName = userName,
                Database = database,
                AuthMethod = authMethod,
            });
        }

        result.Sort((a, b) => string.Compare(a.Server, b.Server, StringComparison.OrdinalIgnoreCase));
        return result;
    }

    private static string ExtractTag(string block, string tag)
    {
        var match = Regex.Match(block, $"<{tag}>([^<]*)</{tag}>");
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    // SSMS settings live under %APPDATA%\Microsoft\SQL Server Management Studio\<version>\UserSettings.xml.
    // The version folder name varies per major (18.0, 19.0, 20.0, 21.0, ...).
    // We pick whichever existing UserSettings.xml was most recently written —
    // that way users running multiple SSMS versions get the one they actually use.
    private string? FindLatestUserSettings()
    {
        if (!Directory.Exists(_settingsRoot)) return null;

        try
        {
            return Directory.EnumerateDirectories(_settingsRoot)
                .Select(d => Path.Combine(d, "UserSettings.xml"))
                .Where(File.Exists)
                .OrderByDescending(GetLastWriteTimeUtcSafe)
                .FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    public static string? FindSsmsExe()
    {
        foreach (var path in DefaultExePaths)
        {
            if (File.Exists(path)) return path;
        }
        return null;
    }

    private static DateTime GetLastWriteTimeUtcSafe(string path)
    {
        try
        {
            return File.Exists(path) ? File.GetLastWriteTimeUtc(path) : DateTime.MinValue;
        }
        catch
        {
            return DateTime.MinValue;
        }
    }
}
