using Content.Shared.Actions;
using Content.Shared.Popups;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared._Stories.Force.Lightsaber;
using Robust.Shared.Prototypes;
using Content.Shared.Alert;
using Robust.Shared.Serialization.Manager;
using Content.Shared._Stories.Force;
using Content.Shared.Rounding;
using Content.Shared.DoAfter;
using Content.Shared.Mobs.Systems;

namespace Content.Shared._Stories.ForceUser;
public abstract partial class SharedForceUserSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ForceSystem _force = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IComponentFactory _compFact = default!;
    [Dependency] private readonly ISerializationManager _seriMan = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    private ISawmill _sawmill = default!;
    public override void Initialize()
    {
        base.Initialize();
        _sawmill = Logger.GetSawmill("ForseUser");
        SubscribeLocalEvent<ForceUserComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<ForceUserComponent, ComponentStartup>(OnStart);
        SubscribeLocalEvent<ForceUserComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<ForceUserComponent, ShotAttemptedEvent>(OnShotAttempted);
        SubscribeLocalEvent<ForceUserComponent, VolumeChangedEvent>(OnVolume);
        InitializeActions();
        InitializeLightsaber();
    }
    private void OnVolume(EntityUid uid, ForceUserComponent component, VolumeChangedEvent args)
    {
        var severity = ContentHelpers.RoundToLevels(MathF.Max(0f, args.NewVolume.Float()), args.MaxVolume.Float(), 20);
        _alerts.ShowAlert(uid, component.AlertType(), (short) severity);
    }
    private void OnStart(EntityUid uid, ForceUserComponent component, ComponentStartup args)
    {
        if (!_proto.TryIndex<ForcePresetPrototype>(component.Preset, out var proto))
        {
            _sawmill.Error($"{ToPrettyString(uid)} failed to load force prototype");
            return;
        }

        if (!_force.SetVolume(uid, proto.Volume, proto.PassiveVolume, proto.MaxVolume))
            _sawmill.Error($"{ToPrettyString(uid)} failed to set force volume");

        EntityManager.RemoveComponents(uid, proto.ToRemove);
        EntityManager.AddComponents(uid, proto.ToAdd);
    }
    private void OnInit(EntityUid uid, ForceUserComponent component, ComponentInit args)
    {
        _popup.PopupEntity(Loc.GetString("force-init-message", ("name", component.Name())), uid, uid);
        _actions.AddAction(uid, ref component.ShopActionEntity, component.ShopAction);
    }
    private void OnShutdown(EntityUid uid, ForceUserComponent component, ComponentShutdown args)
    {
        Del(component.ShopActionEntity);
    }
    private void OnShotAttempted(EntityUid uid, ForceUserComponent comp, ref ShotAttemptedEvent args)
    {
        _popup.PopupEntity(Loc.GetString("gun-disabled"), uid, uid);
        args.Cancel();
    }
}
