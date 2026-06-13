using Content.Server.Weapons.Melee;
using Content.Shared._Stories.Force;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.Damage.Systems;
using Content.Shared.StatusEffectNew;

namespace Content.Server._Stories.ForceUser.ProtectiveBubble.Systems;

public sealed partial class ProtectiveBubbleSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private AlertsSystem _alerts = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private EntityManager _entityManager = default!;
    [Dependency] private IComponentFactory _factory = default!;
    [Dependency] private ForceSystem _force = default!;
    [Dependency] private MeleeWeaponSystem _meleeWeapon = default!;
    [Dependency] private StatusEffectsSystem _statusEffect = default!;
    [Dependency] private SharedTransformSystem _xform = default!;

    public override void Initialize()
    {
        base.Initialize();
        InitializeProtected();
        InitializeUser();
        InitializeBubble();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        UpdateProtected(frameTime);
        UpdateUser(frameTime);
        UpdateBubble(frameTime);
    }
}
