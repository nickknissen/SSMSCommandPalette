using System.Linq;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using SSMSCommandPalette.Commands;
using SSMSCommandPalette.Models;
using SSMSCommandPalette.Services;

namespace SSMSCommandPalette.Pages;

internal sealed partial class SSMSCommandPalettePage : ListPage
{
    private readonly SsmsConnectionService _service;

    public SSMSCommandPalettePage(SsmsConnectionService service)
    {
        _service = service;
        Icon = IconHelpers.FromRelativePath("Assets\\Square44x44Logo.scale-200.png");
        Title = "SSMS";
        Name = "Open";
    }

    public override IListItem[] GetItems()
    {
        var result = _service.GetItems();
        if (result.HasError)
        {
            return [
                new ListItem(new NoOpCommand())
                {
                    Title = result.ErrorTitle,
                    Subtitle = result.ErrorDescription,
                }
            ];
        }

        return result.Items
            .Select(c => CreateItem(c, result.SsmsExePath))
            .OrderBy(x => x.Title)
            .ToArray();
    }

    private static ListItem CreateItem(SsmsConnection connection, string ssmsExePath)
    {
        var subtitle = string.IsNullOrEmpty(connection.Database)
            ? string.Empty
            : $"🗂 {connection.Database}";

        return new ListItem(new OpenSsmsConnectionCommand(connection, ssmsExePath))
        {
            Title = connection.Server,
            Subtitle = subtitle,
            Tags = BuildTags(connection),
        };
    }

    private static Tag[] BuildTags(SsmsConnection connection)
    {
        var tags = new System.Collections.Generic.List<Tag>(2)
        {
            AuthTag(connection.AuthMethod),
        };

        if (connection.AuthMethod == 1 && !string.IsNullOrEmpty(connection.UserName))
        {
            tags.Add(new Tag(connection.UserName) { ToolTip = "Username" });
        }

        return tags.ToArray();
    }

    private static Tag AuthTag(int authMethod)
    {
        return authMethod switch
        {
            0 => new Tag("WIN AUTH")
            {
                Background = ColorHelpers.FromRgb(0, 128, 0),
                Foreground = ColorHelpers.FromRgb(255, 255, 255),
                ToolTip = "Windows Authentication",
            },
            1 => new Tag("SQL AUTH")
            {
                Background = ColorHelpers.FromArgb(255, 76, 161, 222),
                Foreground = ColorHelpers.FromRgb(255, 255, 255),
                ToolTip = "SQL Server Authentication",
            },
            _ => new Tag("AUTH")
            {
                Background = ColorHelpers.FromRgb(120, 120, 120),
                Foreground = ColorHelpers.FromRgb(255, 255, 255),
                ToolTip = "Unknown authentication method",
            },
        };
    }
}
