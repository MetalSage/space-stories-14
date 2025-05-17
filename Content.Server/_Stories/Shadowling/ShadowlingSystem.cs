using Content.Server.Actions;
using Content.Server.Body.Systems;
using Content.Server.Chat.Systems;
using Content.Server.DoAfter;
using Content.Server.Emp;
using Content.Server.Flash;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Light.EntitySystems;
using Content.Server.Lightning;
using Content.Server.Polymorph.Systems;
using Content.Server.Popups;
using Content.Server.RoundEnd;
using Content.Server._Stories.Conversion;
using Content.Server._Stories.Photosensitivity;
using Content.Server.Stunnable;
using Content.Shared.Damage;
using Content.Shared.Light.EntitySystems;
using Content.Shared.Mobs.Systems;
using Content.Shared._Stories.Shadowling;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Shared.Standing;
using Content.Server.Damage.Systems;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Humanoid;
using Content.Shared._Stories.Conversion;

namespace Content.Server._Stories.Shadowling;
public sealed partial class ShadowlingSystem : EntitySystem
{
    [Dependency] private readonly SmokeSystem _smoke = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly HandheldLightSystem _handheldLight = default!;
    [Dependency] private readonly UnpoweredFlashlightSystem _unpoweredFlashlight = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly RoundEndSystem _roundEnd = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly PhysicsSystem _physics = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly LightningSystem _lightning = default!;
    [Dependency] private readonly PoweredLightSystem _poweredLight = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly EmpSystem _emp = default!;
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly PhotosensitivitySystem _photosensitivity = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly ConversionSystem _conversion = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly TransformSystem _xform = default!;
    [Dependency] private readonly FlashSystem _flash = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    public override void Initialize()
    {
        base.Initialize();
        InitializeMindShield();
        InitializeActions();

        SubscribeLocalEvent<ShadowlingComponent, ShotAttemptedEvent>(OnShotAttempted);
        SubscribeLocalEvent<ShadowlingThrallComponent, ConvertedEvent>(OnThrallConverted);
        SubscribeLocalEvent<ShadowlingThrallComponent, RevertedEvent>(OnThrallReverted);
    }
    private void OnShotAttempted(EntityUid uid, ShadowlingComponent comp, ref ShotAttemptedEvent args)
    {
        _popup.PopupEntity(Loc.GetString("gun-disabled"), uid, uid);
        args.Cancel();
    }
    private void OnThrallConverted(EntityUid uid, ShadowlingThrallComponent comp, ConvertedEvent args)
    {
        if (args.Handled)
            return;

        if (TryComp<HumanoidAppearanceComponent>(uid, out var appearance))
        {
            comp.OldEyeColor = appearance.EyeColor;
            appearance.EyeColor = comp.EyeColor;
            Dirty(uid, appearance);
        }

        if (args.Data.Owner != null)
            RefreshActions(GetEntity(args.Data.Owner.Value));

        args.Handled = true;
    }
    private void OnThrallReverted(EntityUid uid, ShadowlingThrallComponent comp, RevertedEvent args)
    {
        if (args.Handled)
            return;

        if (TryComp<HumanoidAppearanceComponent>(uid, out var appearance))
        {
            appearance.EyeColor = comp.OldEyeColor;
            Dirty(uid, appearance);
        }

        if (args.Data.Owner != null)
            RefreshActions(GetEntity(args.Data.Owner.Value));

        args.Handled = true;
    }
}
