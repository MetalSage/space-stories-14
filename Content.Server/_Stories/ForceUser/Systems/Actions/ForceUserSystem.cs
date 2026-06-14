using Content.Server._Stories.Conversion;
using Content.Server._Stories.ForceUser.ProtectiveBubble.Systems;
using Content.Server._Stories.TetherGun;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Cuffs;
using Content.Server.Emp;
using Content.Server.Flash;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Lightning;
using Content.Server.Polymorph.Systems;
using Content.Server.Store.Systems;
using Content.Shared._Stories.Force;
using Content.Shared._Stories.ForceUser;
using Content.Shared._Stories.PullTo;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Standing;
using Content.Shared.StatusEffectNew;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Content.Shared.Throwing;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Stories.ForceUser;

public sealed partial class ForceUserSystem : SharedForceUserSystem
{
    [Dependency] private ProtectiveBubbleSystem _bubble = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private ConversionSystem _conversion = default!;
    [Dependency] private CuffableSystem _cuffable = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private EmpSystem _emp = default!;
    [Dependency] private FlammableSystem _flammable = default!;
    [Dependency] private FlashSystem _flashSystem = default!;
    [Dependency] private ForceSystem _force = default!;
    [Dependency] private SharedHandsSystem _hands = default!;
    [Dependency] private InventorySystem _inventory = default!;
    [Dependency] private LightningSystem _lightning = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private MovementSpeedModifierSystem _movementSpeedModifier = default!;
    [Dependency] private PolymorphSystem _polymorphSystem = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private PullToSystem _pullTo = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SmokeSystem _smoke = default!;
    [Dependency] private StandingStateSystem _standingState = default!;
    [Dependency] private StatusEffectsSystem _statusEffect = default!;
    [Dependency] private StoreSystem _store = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private TagSystem _tagSystem = default!;
    [Dependency] private StoriesTetherGunSystem _tetherGunSystem = default!;
    [Dependency] private ThrowingSystem _throwing = default!;
    [Dependency] private ItemToggleSystem _toggleSystem = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

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
