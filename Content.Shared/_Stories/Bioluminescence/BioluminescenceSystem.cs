using System.Linq;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Body;
using Content.Shared.Body.Components;

namespace Content.Shared._Stories.Bioluminescence;

public sealed class BioluminescenceSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPointLightSystem _light = default!;
    [Dependency] private readonly SharedVisualBodySystem _visualBody = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BioluminescenceComponent, ComponentStartup>(OnStartUp);
        SubscribeLocalEvent<BioluminescenceComponent, TurnBioluminescenceEvent>(TurnBioluminescence);
    }

    private void OnStartUp(EntityUid uid, BioluminescenceComponent component, ComponentStartup args)
    {
        if (!TryComp<ActionsComponent>(uid, out var action))
            return;
        
        SharedPointLightComponent? light = null;
        if (!_light.ResolveLight(uid, ref light))
            return;

        EntityUid? act = null;
        _actions.AddAction(uid, ref act, "TurnBioluminescenceAction", uid, action);
    }

    private void TurnBioluminescence(EntityUid uid, BioluminescenceComponent component, TurnBioluminescenceEvent args)
    {
        SharedPointLightComponent? light = null;
        if (!_light.ResolveLight(uid, ref light))
            return;

        _light.SetEnabled(uid, !light.Enabled);

        var eyeColor = Color.White;
        var foundEyes = false;

        if (TryComp<VisualBodyComponent>(uid, out var visualBody) &&
            _visualBody.TryGatherMarkingsData((uid, visualBody), null, out var profiles, out _, out _))
        {
            var profile = profiles.Values.FirstOrDefault();
            eyeColor = profile.EyeColor;
            foundEyes = true;
        }

        if (!foundEyes)
            return;

        var luma = 0.2126 * eyeColor.R + 0.7152 * eyeColor.G + 0.0722 * eyeColor.B;

        _light.SetColor(uid, luma < 75 ? Color.FromHex("#556b2f") : eyeColor, light);
    }
}
