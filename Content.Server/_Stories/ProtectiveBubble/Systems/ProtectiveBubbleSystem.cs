using Content.Shared.StatusEffect;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared._Stories.Force;
using Content.Server.Weapons.Melee;
using Content.Shared.Alert;
namespace Content.Server._Stories.ForceUser.ProtectiveBubble.Systems;

public sealed partial class ProtectiveBubbleSystem : EntitySystem
{
    [Dependency] private readonly StatusEffectsSystem _statusEffect = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MeleeWeaponSystem _meleeWeapon = default!;
    [Dependency] private readonly IComponentFactory _factory = default!;
    [Dependency] private readonly ForceSystem _force = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
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
    }
}
