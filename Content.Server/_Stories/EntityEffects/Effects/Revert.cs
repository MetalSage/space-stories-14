using Content.Server.Polymorph.Systems;
using Content.Shared.EntityEffects;
using Content.Shared.EntityEffects.Effects;
using Robust.Shared.Prototypes;

namespace Content.Server.EntityEffects.Effects;

[Virtual]
public partial class Revert : SharedRevert
{
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) => ""; // TODO

    public override void Effect(EntityEffectBaseArgs args)
    {
        var entityManager = args.EntityManager;
        var uid = args.TargetEntity;
        var polySystem = entityManager.System<PolymorphSystem>();

        polySystem.Revert(uid);
    }
}
