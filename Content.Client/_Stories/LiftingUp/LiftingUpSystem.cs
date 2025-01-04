using System.Numerics;
using Content.Shared.Gravity;
using Robust.Client.GameObjects;
using Robust.Client.Animations;
using Robust.Shared.Animations;

namespace Content.Client.Gravity;

/// <inheritdoc/>
public sealed class LiftingUpSystem : SharedLiftingUpSystem
{
    public const string AnimationKey = "_gravity";
    public Vector2 Offset = new(0, 0.2f);
    public const float AnimationTime = 2f;
    [Dependency] private readonly AnimationPlayerSystem _animationSystem = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LiftingUpComponent, AnimationCompletedEvent>(OnAnimationCompleted);
    }
    protected override void OnComponentShutdown(EntityUid uid, LiftingUpComponent component, ComponentShutdown args)
    {
        base.OnComponentShutdown(uid, component, args);
        _animationSystem.Stop(uid, AnimationKey);
    }
    public override void Animation(EntityUid uid, Vector2 offset, string animationKey, string animationDownKey, float animationTime, float animationDownTime, bool up = true)
    {
        var animation = new Animation
        {
            Length = TimeSpan.FromSeconds(animationTime),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Offset),
                    InterpolationMode = AnimationInterpolationMode.Cubic,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0f),
                        new AnimationTrackProperty.KeyFrame(offset, animationTime),
                    }
                }
            }
        };

        var animationDown = new Animation
        {
            Length = TimeSpan.FromSeconds(animationDownTime),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Offset),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(offset, 0f),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, animationDownTime),
                    }
                }
            }
        };

        if (_animationSystem.HasRunningAnimation(uid, animationKey))
            return;

        if (up)
            _animationSystem.Play(uid, animation, animationKey);
        else
            _animationSystem.Play(uid, animationDown, animationDownKey);
    }
    private void FloatAnimation(EntityUid uid, LiftingUpComponent? component = null, bool stop = false)
    {
        if (!Resolve(uid, ref component))
            return;

        if (stop)
        {
            _animationSystem.Stop(uid, AnimationKey);
            return;
        }

        var animation = new Animation
        {
            Length = TimeSpan.FromSeconds(AnimationTime * 2),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Offset),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(component.Offset, 0f),
                        new AnimationTrackProperty.KeyFrame(Offset, AnimationTime),
                        new AnimationTrackProperty.KeyFrame(component.Offset, AnimationTime),
                    }
                }
            }
        };

        if (!_animationSystem.HasRunningAnimation(uid, AnimationKey))
            _animationSystem.Play(uid, animation, AnimationKey);
    }
    private void OnAnimationCompleted(EntityUid uid, LiftingUpComponent component, AnimationCompletedEvent args)
    {
        if (args.Key == component.AnimationKey)
            FloatAnimation(uid, component, false);

        if (args.Key == component.AnimationDownKey)
            FloatAnimation(uid, component, true);

        if (args.Key == AnimationKey)
            FloatAnimation(uid, component, false);
    }
}
