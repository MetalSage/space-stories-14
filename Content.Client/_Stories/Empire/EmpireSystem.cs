using Content.Shared._Stories.Empire.Components;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;

namespace Content.Client._Stories.Empire;

public sealed class EmpireSystem : SharedStatusIconSystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EmpireComponent, GetStatusIconsEvent>(OnGetStatusIconsEvent);
    }

    private void OnGetStatusIconsEvent(EntityUid uid, EmpireComponent component, ref GetStatusIconsEvent args)
    {
        args.StatusIcons.Add(_prototype.Index(component.StatusIcon));
    }
}
