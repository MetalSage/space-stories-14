using Content.Shared._Stories.Holy;
using Content.Shared.EntityEffects;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;

namespace Content.Shared.EntityEffects.Effects
{
    public sealed partial class BlessReaction : EntityEffect
    {
        [DataField]
        public TimeSpan Time = TimeSpan.FromSeconds(10);

        public override void RaiseEvent(EntityUid target, IEntityEffectRaiser raiser, float scale, EntityUid? solutionEntity)
        {
            var entManager = IoCManager.Resolve<IEntityManager>();
            var holySystem = entManager.System<SharedHolySystem>();
            holySystem.TryBless(target, Time, false);
        }
    }
}
