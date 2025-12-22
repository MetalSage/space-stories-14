using Content.Server.Polymorph.Systems;
using Content.Shared.EntityEffects;
using Content.Shared.EntityEffects.Effects;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Prototypes;

namespace Content.Server.EntityEffects.Effects;

[Virtual]
public partial class Revert : SharedRevert
{
    public override void RaiseEvent(EntityUid target, IEntityEffectRaiser raiser, float scale, EntityUid? user)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();
        var polySystem = entityManager.System<PolymorphSystem>();

        polySystem.Revert(target);
    }
}
