using SSMSCommandPalette.Models;

namespace SSMSCommandPalette.Services;

/// <summary>
/// Hard-coded fake connections used when the project is built with the
/// DEMO_MODE define. Used for Microsoft Store screenshots — never ships
/// to end users.
/// </summary>
internal static class DemoSsmsData
{
    private const string FakeSsmsExePath =
        @"C:\Program Files\Microsoft SQL Server Management Studio 20\Common7\IDE\Ssms.exe";

    public static SsmsQueryResult Result() => new()
    {
        SsmsExePath = FakeSsmsExePath,
        Items = new[]
        {
            Conn("LOCALHOST\\SQLEXPRESS",                           authMethod: 0),
            Conn("dev-sql-01.example.com",     "AppDb_Dev",         authMethod: 0),
            Conn("acme-staging.database.windows.net", "AcmeStaging", authMethod: 1, user: "acme_reader"),
            Conn("acme-prod.database.windows.net",    "AcmeOrders",  authMethod: 1, user: "acme_reader"),
            Conn("analytics-01.example.com",   "Warehouse",         authMethod: 0),
            Conn("analytics-01.example.com",   "Reports",           authMethod: 0),
            Conn("legacy-sql.example.com",     "BillingArchive",    authMethod: 1, user: "billing_ro"),
            Conn("sandbox-sql.example.com",                          authMethod: 1, user: "sa"),
        },
    };

    private static SsmsConnection Conn(
        string server,
        string database = "",
        int authMethod = 0,
        string user = "")
        => new()
        {
            Server = server,
            Database = database,
            AuthMethod = authMethod,
            UserName = user,
        };
}
