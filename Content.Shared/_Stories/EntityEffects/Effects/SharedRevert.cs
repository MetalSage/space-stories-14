using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Shared.EntityEffects.Effects;

[Virtual]
public partial class SharedRevert : EntityEffect
{
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) => ""; // TODO

    public override void Effect(EntityEffectBaseArgs args)
    {

    }
}
