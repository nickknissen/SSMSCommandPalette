using System;
using System.Diagnostics;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SSMSCommandPalette.Models;

namespace SSMSCommandPalette.Commands;

internal sealed partial class OpenSsmsConnectionCommand : InvokableCommand
{
    private readonly SsmsConnection _connection;
    private readonly string _ssmsExePath;

    public OpenSsmsConnectionCommand(SsmsConnection connection, string ssmsExePath)
    {
        _connection = connection;
        _ssmsExePath = ssmsExePath;
        Icon = IconHelpers.FromRelativePath("Assets\\Square44x44Logo.scale-200.png");
    }

    public override string Name => $"Open {_connection.Server}";

    public override CommandResult Invoke()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = _ssmsExePath,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            psi.ArgumentList.Add("-S");
            psi.ArgumentList.Add(_connection.Server);
            if (!string.IsNullOrEmpty(_connection.Database))
            {
                psi.ArgumentList.Add("-d");
                psi.ArgumentList.Add(_connection.Database);
            }
            // SQL Server Auth — SSMS will prompt for the password.
            if (_connection.AuthMethod == 1 && !string.IsNullOrEmpty(_connection.UserName))
            {
                psi.ArgumentList.Add("-U");
                psi.ArgumentList.Add(_connection.UserName);
            }

            Process.Start(psi);
            return CommandResult.Dismiss();
        }
        catch (Exception ex)
        {
            return CommandResult.ShowToast(new ToastArgs
            {
                Message = $"Couldn't open SSMS: {ex.Message}",
                Result = CommandResult.KeepOpen(),
            });
        }
    }
}
