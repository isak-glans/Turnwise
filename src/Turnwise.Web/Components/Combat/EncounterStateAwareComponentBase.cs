using Microsoft.AspNetCore.Components;
using Turnwise.Web.State;

namespace Turnwise.Web.Components.Combat;

/// <summary>
/// Base for components that render from <see cref="EncounterSessionState"/>. Blazor does not
/// automatically re-render a child component just because its parent re-rendered when the child
/// has no changing parameters, so every component that reads shared session state needs its own
/// subscription to know when to redraw - subscribing only at the page level is not enough.
/// </summary>
public abstract class EncounterStateAwareComponentBase : ComponentBase, IDisposable
{
    [Inject]
    protected EncounterSessionState State { get; set; } = default!;

    protected override void OnInitialized()
    {
        State.Changed += StateHasChanged;
    }

    public void Dispose()
    {
        State.Changed -= StateHasChanged;
    }
}
