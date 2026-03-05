using Content.Shared._Stories.Shadowling;
using Content.Shared.Humanoid;
using Content.Shared.StatusIcon.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;

namespace Content.Client._Stories.Shadowling;

public sealed class ShadowlingSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    private readonly ProtoId<ShaderPrototype> _unshadedShaderProtoId = "unshaded";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingComponent, GetStatusIconsEvent>(OnGetStatusIconsEvent);

        SubscribeLocalEvent<ShadowlingThrallComponent, ComponentInit>(OnConverted);
        SubscribeLocalEvent<ShadowlingThrallComponent, ComponentShutdown>(OnReverted);
    }

    private void OnGetStatusIconsEvent(EntityUid uid, ShadowlingComponent component, ref GetStatusIconsEvent args)
    {
        args.StatusIcons.Add(_prototype.Index(component.StatusIcon));
    }

    private void OnConverted(EntityUid uid, ShadowlingThrallComponent component, ComponentInit args)
    {
        if (!HasComp<HumanoidProfileComponent>(uid))
            return;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        sprite.LayerSetShader(sprite.LayerMapReserveBlank(HumanoidVisualLayers.Eyes),
            _prototype.Index(_unshadedShaderProtoId).Instance());
    }

    private void OnReverted(EntityUid uid, ShadowlingThrallComponent component, ComponentShutdown args)
    {
        if (!HasComp<HumanoidProfileComponent>(uid))
            return;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        sprite.LayerSetShader(sprite.LayerMapReserveBlank(HumanoidVisualLayers.Eyes), (ShaderInstance?)null);
    }
}
