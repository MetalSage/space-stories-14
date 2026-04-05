using Content.Shared.Inventory.Events;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Events;

namespace Content.Shared._Stories.Shadowling;

public sealed class SharedShadowlingSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadowlingComponent, ShotAttemptedEvent>(OnShotAttempted);
    }

    private void OnShotAttempted(Entity<ShadowlingComponent> entity, ref ShotAttemptedEvent args)
    {
        args.Cancel();
    }
}
