using Content.Shared.Mindshield.Components;

namespace Content.Shared._Stories.Mindshield;

public sealed partial class MindShieldSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MindShieldComponent, ComponentInit>(MindShieldImplanted);
    }

    private void MindShieldImplanted(EntityUid uid, MindShieldComponent comp, ComponentInit init)
    {
        var ev = new MindShieldImplantedEvent(uid, comp);
        _entityManager.EventBus.RaiseLocalEvent(uid, ref ev, true);
    }
}

[ByRefEvent]
public readonly record struct MindShieldImplantedEvent(EntityUid EntityUid, MindShieldComponent Component);
