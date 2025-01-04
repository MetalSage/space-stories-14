using Content.Shared._Stories.ForceUser;
using Content.Shared.Popups;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Throwing;
using Content.Server._Stories.TetherGun;
using Content.Server.Flash;
using Content.Server.Emp;
using Content.Server.Cuffs;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Lightning;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Containers;
using Content.Shared.Damage;
using Content.Shared.Stunnable;
using Content.Shared.Standing;
using Content.Shared.StatusEffect;
using Content.Server.Body.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Movement.Systems;
using Content.Server.Popups;
using Content.Shared.Inventory;
using Content.Server._Stories.Empire;
using Content.Server.Store.Systems;
using Content.Shared._Stories.Force;
using Content.Server.Polymorph.Systems;
using Content.Shared.Tag;
using Robust.Shared.Map;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Administration.Systems;
using Content.Shared._Stories.PullTo;
using Robust.Shared.Random;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Interaction;
using Content.Server.Beam;
using Content.Server._Stories.ForceUser.ProtectiveBubble.Systems;
using Robust.Shared.Prototypes;
using Content.Server._Stories.Conversion;

namespace Content.Server._Stories.ForceUser;
public sealed partial class ForceUserSystem : SharedForceUserSystem
{
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly BeamSystem _beam = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly RejuvenateSystem _rejuvenate = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly PolymorphSystem _polymorphSystem = default!;
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly StandingStateSystem _standingState = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffect = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly RespiratorSystem _respiratorSystem = default!;
    [Dependency] private readonly EmpireSystem _empireSystem = default!;
    [Dependency] private readonly FlashSystem _flashSystem = default!;
    [Dependency] private readonly CuffableSystem _cuffable = default!;
    [Dependency] private readonly LightningSystem _lightning = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = default!;
    [Dependency] private readonly EmpSystem _emp = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly ProtectiveBubbleSystem _bubble = default!;
    [Dependency] private readonly TetherGunSystem _tetherGunSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly ForceSystem _force = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SmokeSystem _smoke = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly PullToSystem _pullTo = default!;
    [Dependency] private readonly ItemToggleSystem _toggleSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly ConversionSystem _conversion = default!;
    public override void Initialize()
    {
        base.Initialize();
        InitializeTetherHand();
        InitializeRecall();
        InitializeRecallEquipment();
        InitializeSimpleActions();
        InitializeStrangle();
        InitializePolymorph();
        InitializeProtectiveBubble();
        InitializeLightsaber();
        InitializeLightning();
        InitializeSteal();
    }
    public override void Update(float frameTime)
    {
        base.Update(frameTime);
    }
}
