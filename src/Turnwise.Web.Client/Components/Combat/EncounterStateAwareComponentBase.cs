using Microsoft.AspNetCore.Components;
using Turnwise.Web.Client.State;

namespace Turnwise.Web.Client.Components.Combat;

/// <summary>
/// Base for components that render from <see cref="EncounterSessionState"/>. Blazor does not
/// automatically re-render a child component just because its parent re-rendered when the child
/// has no changing parameters, so every component that reads shared session state needs its own
/// subscription to know when to redraw - subscribing only at the page level is not enough.
/// </summary>
public abstract class EncounterStateAwareComponentBase : ComponentBase, IAsyncDisposable
{
    [Inject]
    protected EncounterSessionState State { get; set; } = default!;

    protected override void OnInitialized()
    {
        State.Changed += StateHasChanged;
    }

    public async ValueTask DisposeAsync()
    {
        State.Changed -= StateHasChanged;
        await DisposeAsyncCore();
    }

    /// <summary>Override for extra async cleanup (e.g. disposing a JS module reference) - the base unsubscription above always runs regardless.</summary>
    protected virtual ValueTask DisposeAsyncCore() => ValueTask.CompletedTask;
}
