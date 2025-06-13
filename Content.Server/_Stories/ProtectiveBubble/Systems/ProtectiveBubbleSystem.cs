using Content.Shared.StatusEffect;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared._Stories.Force;
using Content.Server.Weapons.Melee;
using Content.Shared.Alert;
using Content.Server.Destructible;
using Robust.Shared.Prototypes;
using Content.Shared.Popups;
using Content.Shared.Timing;
using Content.Shared.Item.ItemToggle;
using Content.Shared._Stories.ProtectiveBubble;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Microsoft.EntityFrameworkCore.Update;
using Content.Server.Item;

namespace Content.Server._Stories.ProtectiveBubble;

public sealed partial class ProtectiveBubbleSystem : EntitySystem
{
    [Dependency] private readonly ItemSystem _item = default!;
    [Dependency] private readonly PhysicsSystem _physics = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly UseDelaySystem _useDelay = default!;
    [Dependency] private readonly ItemToggleSystem _itemToggle = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly DestructibleSystem _destructible = default!;
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
        InitializeAlert();
        InitializeCollide();
        InitializeUser();
        InitializeProtection();
        InitializeGenerator();
    }
    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        UpdateProtection(frameTime);
    }
}
