using Content.Shared.Actions;
using Content.Shared.Humanoid;

namespace Content.Shared._Stories.Bioluminescence;
public sealed class BioluminescenceSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPointLightSystem _light = default!;

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

    private void TurnBioluminescence(EntityUid uid, BioluminescenceComponent component, TurnBioluminescenceEvent _)
    {
        SharedPointLightComponent? light = null;
        if (!_light.ResolveLight(uid, ref light))
            return;

        _light.SetEnabled(uid, !light.Enabled);

        if (!TryComp<HumanoidAppearanceComponent>(uid, out var humanoid))
            return;

        var luma = 0.2126 * humanoid.EyeColor.R + 0.7152 * humanoid.EyeColor.G + 0.0722 * humanoid.EyeColor.B;

        _light.SetColor(uid, luma < 75 ? Color.FromHex("#556b2f") : humanoid.EyeColor, light);
    }
}
