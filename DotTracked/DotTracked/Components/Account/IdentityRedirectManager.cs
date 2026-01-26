using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;

namespace DotTracked.Components.Account;

internal sealed class IdentityRedirectManager(NavigationManager navigationManager)
{
    private string CurrentPath => navigationManager.ToAbsoluteUri(navigationManager.Uri).GetLeftPart(UriPartial.Path);

    [DoesNotReturn]
    public void RedirectTo(string? uri)
    {
        uri ??= "";

        if (!Uri.IsWellFormedUriString(uri, UriKind.Relative))
        {
            uri = navigationManager.ToBaseRelativePath(uri);
        }

        navigationManager.NavigateTo(uri);
        throw new InvalidOperationException(
            $"{nameof(IdentityRedirectManager)} can only be used during static rendering.");
    }

    [DoesNotReturn]
    public void RedirectToCurrentPage()
    {
        RedirectTo(CurrentPath);
    }
}