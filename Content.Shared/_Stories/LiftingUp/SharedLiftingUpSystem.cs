using System.Numerics;
using Robust.Shared.Map;

namespace Content.Shared.Gravity;

public abstract class SharedLiftingUpSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LiftingUpComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<LiftingUpComponent, ComponentShutdown>(OnComponentShutdown);
        SubscribeLocalEvent<LiftingUpComponent, EntParentChangedMessage>(OnEntParentChanged);
    }
    public virtual void Animation(EntityUid uid, Vector2 offset, string animationKey, string animationDownKey, float animationTime, float animationDownTime, bool up = true) { }
    protected virtual void OnComponentShutdown(EntityUid uid, LiftingUpComponent component, ComponentShutdown args)
    {
        Animation(uid, component.Offset, component.AnimationKey, component.AnimationDownKey, component.AnimationTime, component.AnimationDownTime, false);
    }

    private void OnComponentStartup(EntityUid uid, LiftingUpComponent component, ComponentStartup args)
    {
        Animation(uid, component.Offset, component.AnimationKey, component.AnimationDownKey, component.AnimationTime, component.AnimationDownTime);
    }

    private void OnEntParentChanged(EntityUid uid, LiftingUpComponent component, ref EntParentChangedMessage args)
    {
        Animation(uid, component.Offset, component.AnimationKey, component.AnimationDownKey, component.AnimationTime, component.AnimationDownTime);
    }
}
