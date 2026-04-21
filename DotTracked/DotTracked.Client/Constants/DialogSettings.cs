using MudBlazor;

namespace DotTracked.Client.Constants;

public static class DialogSettings
{
    public static DialogOptions Medium => new()
    {
        MaxWidth = MaxWidth.Medium,
        FullWidth = true,
        CloseButton = true,
        BackdropClick = false,
        CloseOnEscapeKey = true
    };

    public static DialogOptions Small => new()
    {
        MaxWidth = MaxWidth.Small,
        FullWidth = true,
        CloseButton = true,
        BackdropClick = false,
        CloseOnEscapeKey = true
    };

    public static DialogOptions ExtraSmall => new()
    {
        MaxWidth = MaxWidth.ExtraSmall,
        FullWidth = true,
        CloseButton = true,
        BackdropClick = false,
        CloseOnEscapeKey = true
    };
}