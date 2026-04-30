// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SSMSCommandPalette.Pages;
using SSMSCommandPalette.Services;

namespace SSMSCommandPalette;

public partial class SSMSCommandPaletteCommandsProvider : CommandProvider
{
    private readonly ICommandItem[] _commands;

    public SSMSCommandPaletteCommandsProvider()
    {
        DisplayName = "SSMS";
        Icon = IconHelpers.FromRelativePath("Assets\\Square44x44Logo.scale-200.png");

        var service = new SsmsConnectionService();

        _commands = [
            new CommandItem(new SSMSCommandPalettePage(service)) { Title = DisplayName },
        ];
    }

    public override ICommandItem[] TopLevelCommands()
    {
        return _commands;
    }
}
