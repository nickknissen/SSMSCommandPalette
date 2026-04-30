namespace SSMSCommandPalette.Models;

public sealed class SsmsConnection
{
    public string Server { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;

    // 0 = Windows Authentication, 1 = SQL Server Authentication
    // (matches AuthenticationMethod values written by SSMS to UserSettings.xml)
    public int AuthMethod { get; set; }

    public string AuthLabel => AuthMethod switch
    {
        0 => "Windows Auth",
        1 => "SQL Server Auth",
        _ => "Unknown Auth",
    };
}
