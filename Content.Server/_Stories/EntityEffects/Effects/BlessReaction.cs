using Content.Server._Stories.Holy;
using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server.EntityEffects.Effects
{
    [UsedImplicitly]
    public sealed partial class BlessReaction : EntityEffect
    {
        [DataField]
        public TimeSpan Time = TimeSpan.FromSeconds(10);

        protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) => ""; // TODO

        public override void Effect(EntityEffectBaseArgs args)
        {
            var saintedSystem = args.EntityManager.System<HolySystem>();
            saintedSystem.TryBless(args.TargetEntity, Time, false);
        }
    }
}
