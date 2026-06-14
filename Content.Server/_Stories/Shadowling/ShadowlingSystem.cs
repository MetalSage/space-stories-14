using System.Linq;
using Content.Server._Stories.Conversion;
using Content.Shared._Stories.Conversion;
using Content.Shared._Stories.Mindshield;
using Content.Shared._Stories.Shadowling;
using Content.Shared._Stories.Vision.Components;
using Content.Shared._Stories.Vision.Systems;
using Content.Shared.Actions;
using Content.Shared.Body;
using Content.Shared.Mindshield.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;

namespace Content.Server._Stories.Shadowling;

public sealed partial class ShadowlingSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private ConversionSystem _conversion = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private SharedVisionSystem _vision = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    [Dependency] private SharedVisualBodySystem _visualBody = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<ShadowlingComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<ShadowlingComponent, MindShieldImplantedEvent>(OnMindShieldImplanted);

        SubscribeLocalEvent<ShadowlingThrallComponent, ConvertedEvent>(OnThrallConverted);
        SubscribeLocalEvent<ShadowlingThrallComponent, RevertedEvent>(OnThrallReverted);
        SubscribeLocalEvent<ShadowlingThrallComponent, MobStateChangedEvent>(OnThrallMobStateChanged);
    }

    private void OnInit(EntityUid uid, ShadowlingComponent component, ComponentInit args)
    {
        RefreshActions(uid, component);

        if (!HasComp<VisionProviderComponent>(uid))
        {
            var vision = EnsureComp<VisionProviderComponent>(uid);
            vision.IsActive = true;
            vision.ThermalVision = true;
            vision.AmbientColor = Color.FromHex("#FFFFFF10");
            vision.Priority = 10;
            vision.ToggleAction = "STShadowlingActionToggleVision";

            if (vision.ToggleAction != null)
                _actions.AddAction(uid, ref vision.ToggleActionEntity, vision.ToggleAction.Value, uid);

            Dirty(uid, vision);
            _vision.UpdateVision(uid);
        }
    }

    private void OnMobStateChanged(EntityUid uid, ShadowlingComponent component, MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        var thralls = _conversion.GetEntitiesConvertedBy(uid, component.ShadowlingThrallConversion).ToList();
        foreach (var thrall in thralls)
        {
            _conversion.TryRevert(thrall, component.ShadowlingThrallConversion);
        }
    }

    private void OnMindShieldImplanted(EntityUid uid, ShadowlingComponent component, MindShieldImplantedEvent args)
    {
        RemCompDeferred<MindShieldComponent>(uid);
    }

    private void OnThrallConverted(EntityUid uid, ShadowlingThrallComponent component, ConvertedEvent args)
    {
        if (args.Handled)
            return;

        _appearance.SetData(uid, ShadowlingThrallVisuals.IsThrall, true);

        if (_visualBody.TryGatherMarkingsData(uid, null, out var profiles, out _, out _) && profiles.Count > 0)
        {
            component.OldEyeColor = profiles.Values.First().EyeColor;

            var newProfiles = profiles.ToDictionary(
                pair => pair.Key,
                pair => pair.Value with { EyeColor = component.EyeColor });

            _visualBody.ApplyProfiles(uid, newProfiles);
            Dirty(uid, component);
        }

        RefreshAllShadowlings();
        args.Handled = true;
    }

    private void OnThrallReverted(EntityUid uid, ShadowlingThrallComponent component, RevertedEvent args)
    {
        if (args.Handled)
            return;

        _appearance.SetData(uid, ShadowlingThrallVisuals.IsThrall, false);

        if (_visualBody.TryGatherMarkingsData(uid, null, out var profiles, out _, out _) && profiles.Count > 0)
        {
            var newProfiles = profiles.ToDictionary(
                pair => pair.Key,
                pair => pair.Value with { EyeColor = component.OldEyeColor });

            _visualBody.ApplyProfiles(uid, newProfiles);
        }

        RefreshAllShadowlings();
        args.Handled = true;
    }

    private void OnThrallMobStateChanged(EntityUid uid, ShadowlingThrallComponent component, MobStateChangedEvent args)
    {
        RefreshAllShadowlings();
    }

    private void RefreshAllShadowlings()
    {
        var query = EntityQueryEnumerator<ShadowlingComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            RefreshActions(uid, comp);
        }
    }

    public int RefreshActions(EntityUid uid, ShadowlingComponent component)
    {
        var aliveThrallsAmount = 0;
        var thrallsQuery = EntityQueryEnumerator<ShadowlingThrallComponent>();
        while (thrallsQuery.MoveNext(out var thrallUid, out _))
        {
            if (_mobState.IsAlive(thrallUid))
                aliveThrallsAmount++;
        }

        foreach (var (action, requiredAmount) in component.ActionRequirements)
        {
            if (aliveThrallsAmount >= requiredAmount && !component.GrantedActions.ContainsKey(action))
            {
                var actionId = _actions.AddAction(uid, action);
                if (actionId != null)
                    component.GrantedActions[action] = actionId.Value;
            }
            else if (aliveThrallsAmount < requiredAmount && component.GrantedActions.TryGetValue(action, out var actionId))
            {
                _actions.RemoveAction(uid, actionId);
                component.GrantedActions.Remove(action);
            }
        }

        Dirty(uid, component);
        return aliveThrallsAmount;
    }
}
