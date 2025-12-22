using Content.Shared.EntityEffects;

namespace Content.Shared.EntityEffects.Effects;

[Virtual]
public partial class SharedRevert : EntityEffect
{
    public override void RaiseEvent(EntityUid target, IEntityEffectRaiser raiser, float scale, EntityUid? solutionEntity)
    {
    }
}
