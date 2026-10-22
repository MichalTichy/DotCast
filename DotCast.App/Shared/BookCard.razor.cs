using DotCast.SharedKernel.Models;
using Microsoft.AspNetCore.Components;

namespace DotCast.App.Shared;

public partial class BookCard : ComponentBase
{
    [Parameter, EditorRequired] public AudioBook Book { get; set; } = null!;
    [Parameter] public AudioBookUserState? UserState { get; set; }
    [Parameter] public EventCallback<AudioBook> Selected { get; set; }
    private DisplayedRating Rating => UserState?.DisplayedRating ?? DisplayedRating.From(Book, null);
    private Task Open() => Selected.InvokeAsync(Book);
}
