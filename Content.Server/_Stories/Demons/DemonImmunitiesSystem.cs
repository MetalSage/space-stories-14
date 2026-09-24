using Content.Shared._Stories.Demons;
using Content.Shared.Mobs;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Standing;
using Robust.Shared.Timing;

namespace Content.Server._Stories.Demons;

public sealed partial class DemonImmunitiesSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DemonImmunitiesComponent, PullAttemptEvent>(OnPullAttempt);
        SubscribeLocalEvent<DemonImmunitiesComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<DemonImmunitiesComponent, DownAttemptEvent>(OnDownAttempt);
    }

    private void OnDownAttempt(Entity<DemonImmunitiesComponent> ent, ref DownAttemptEvent args)
    {
        if (ent.Comp.ImmuneToKnockdown)
            args.Cancel();
    }

    private void OnPullAttempt(Entity<DemonImmunitiesComponent> ent, ref PullAttemptEvent args)
    {
        if (args.PulledUid == ent.Owner)
            args.Cancelled = true;
    }

    private void OnMobStateChanged(Entity<DemonImmunitiesComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        if (ent.Comp.VanishDelay is not { } delay)
            return;

        var uid = ent.Owner;
        Timer.Spawn(delay, () =>
        {
            if (!Deleted(uid))
                QueueDel(uid);
        });
    }
}
