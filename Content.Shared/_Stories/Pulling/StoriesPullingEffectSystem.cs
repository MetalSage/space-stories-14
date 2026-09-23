using System.Numerics;
using Content.Shared.Coordinates;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._Stories.Pulling;

public sealed partial class StoriesPullingEffectSystem : EntitySystem
{
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedMeleeWeaponSystem _melee = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    private static readonly EntProtoId PullEffect = "STEffectGrab";

    private readonly SoundSpecifier _pullSound = new SoundPathSpecifier("/Audio/Effects/thudswoosh.ogg")
    {
        Params = AudioParams.Default.WithVariation(0.05f),
    };

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PullableComponent, PullStartedMessage>(OnPullStarted);
    }

    private void OnPullStarted(Entity<PullableComponent> ent, ref PullStartedMessage args)
    {
        if (args.PulledUid != ent.Owner)
            return;

        PlayPullEffect(args.PullerUid, args.PulledUid);
    }

    public void PlayPullEffect(EntityUid puller, EntityUid pulled)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        var userXform = Transform(puller);
        var targetPos = _transform.GetWorldPosition(pulled);
        var localPos = Vector2.Transform(targetPos, _transform.GetInvWorldMatrix(userXform));
        localPos = userXform.LocalRotation.RotateVec(localPos);

        _melee.DoLunge(puller, puller, Angle.Zero, localPos, null);
        _audio.PlayPredicted(_pullSound, pulled, puller);

        PredictedSpawnAttachedTo(PullEffect, pulled.ToCoordinates());
    }
}
