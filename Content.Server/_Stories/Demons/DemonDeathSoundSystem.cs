using Content.Shared._Stories.Demons;
using Content.Shared.Mobs;
using Robust.Shared.Audio.Systems;

namespace Content.Server._Stories.Demons;

public sealed partial class DemonDeathSoundSystem : EntitySystem
{
    [Dependency] private SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DemonDeathSoundComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnMobStateChanged(Entity<DemonDeathSoundComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        _audio.PlayPvs(ent.Comp.Sound, ent);
    }
}
