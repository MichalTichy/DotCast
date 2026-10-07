using DotCast.SharedKernel.Models;
using Microsoft.AspNetCore.Components;

namespace DotCast.App.Shared;

public partial class BookCard : ComponentBase
{
    [Parameter, EditorRequired] public AudioBook Book { get; set; } = null!;
    [Parameter] public EventCallback<AudioBook> Selected { get; set; }
    private Task Open() => Selected.InvokeAsync(Book);
}
