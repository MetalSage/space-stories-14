using System.Numerics;
using Content.Shared._Stories.Revenant;
using Content.Shared.Follower.Components;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Animations;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Client._Stories.Revenant;

public sealed class HaloVisualsSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _robustRandom = default!;
    [Dependency] private readonly AnimationPlayerSystem _animations = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private readonly string _halostopkey = "halo_stop";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HaloVisualsComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<HaloVisualsComponent, ComponentRemove>(OnComponentRemove);
    }

    private void OnComponentInit(EntityUid uid, HaloVisualsComponent component, ComponentInit args)
    {
        _robustRandom.SetSeed((int)_timing.CurTime.TotalMilliseconds);
        component.HaloDistance =
            _robustRandom.NextFloat(0.75f * component.HaloDistance, 1.25f * component.HaloDistance);

        component.HaloLength = _robustRandom.NextFloat(0.5f * component.HaloLength, 1.5f * component.HaloLength);

        if (TryComp<SpriteComponent>(uid, out var sprite))
        {
            sprite.EnableDirectionOverride = true;
            sprite.DirectionOverride = Direction.South;
        }

        var animationPlayer = EnsureComp<AnimationPlayerComponent>(uid);
        if (_animations.HasRunningAnimation(uid, animationPlayer, _halostopkey))
        {
            _animations.Stop((uid, animationPlayer), _halostopkey);
        }
    }

    private void OnComponentRemove(EntityUid uid, HaloVisualsComponent component, ComponentRemove args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        sprite.EnableDirectionOverride = false;

        var animationPlayer = EnsureComp<AnimationPlayerComponent>(uid);
        if (!_animations.HasRunningAnimation(uid, animationPlayer, _halostopkey))
        {
            _animations.Play((uid, animationPlayer), GetStopAnimation(component, sprite), _halostopkey);
        }
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        foreach (var (halo, sprite) in EntityManager.EntityQuery<HaloVisualsComponent, SpriteComponent>())
        {
            var progress = (float)(_timing.CurTime.TotalSeconds / halo.HaloLength) % 1;
            var angle = new Angle(Math.PI * 2 * progress);

            var baseVec = angle.RotateVec(new Vector2(halo.HaloDistance, 0));

            var haloScaleY = 0.3f;
            var haloVec = baseVec with { Y = baseVec.Y * haloScaleY };

            sprite.Offset = haloVec;
        }
    }

    private Animation GetStopAnimation(HaloVisualsComponent component, SpriteComponent sprite)
    {
        var length = component.HaloStopLength;

        return new Animation()
        {
            Length = TimeSpan.FromSeconds(length),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Offset),
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(sprite.Offset, 0f),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, length),
                    },
                    InterpolationMode = AnimationInterpolationMode.Linear
                },
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Rotation),
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(sprite.Rotation.Reduced(), 0f),
                        new AnimationTrackProperty.KeyFrame(Angle.Zero, length),
                    },
                    InterpolationMode = AnimationInterpolationMode.Linear
                }
            }
        };
    }
}
