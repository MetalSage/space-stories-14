using Content.Shared._Stories.Shadowling;
using Content.Shared.Humanoid;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Content.Shared.Ghost;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Shared.Prototypes;

namespace Content.Client._Stories.Shadowling;

public sealed partial class ShadowlingSystem : EntitySystem
{
    [Dependency] private IPrototypeManager _prototype = default!;
    [Dependency] private SpriteSystem _spriteSystem = default!;
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingComponent, GetStatusIconsEvent>(OnGetStatusIconsEvent);
        SubscribeLocalEvent<ShadowWalkingComponent, ComponentStartup>(OnShadowWalkStartup);
        SubscribeLocalEvent<ShadowWalkingComponent, ComponentShutdown>(OnShadowWalkShutdown);

        SubscribeLocalEvent<ShadowlingThrallComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnAppearanceChange(EntityUid uid, ShadowlingThrallComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (!_appearance.TryGetData<bool>(uid, ShadowlingThrallVisuals.IsThrall, out var isThrall))
            return;

        if (!_spriteSystem.LayerMapTryGet(uid, HumanoidVisualLayers.Eyes, out var eyeLayer, false))
            return;

        if (isThrall)
        {
            args.Sprite.LayerSetShader(eyeLayer, "unshaded");
        }
        else
        {
            args.Sprite.LayerSetShader(eyeLayer, "shaded");
        }
    }

    private void OnGetStatusIconsEvent(EntityUid uid, ShadowlingComponent component, ref GetStatusIconsEvent args)
    {
        var local = _player.LocalSession?.AttachedEntity;
        if (local == null)
            return;

        if (!HasComp<GhostComponent>(local.Value) && !HasComp<ShadowlingComponent>(local.Value) && !HasComp<ShadowlingThrallComponent>(local.Value))
            return;

        args.StatusIcons.Add(_prototype.Index<FactionIconPrototype>(component.StatusIcon));
    }

    private void OnShadowWalkStartup(EntityUid uid, ShadowWalkingComponent component, ref ComponentStartup args)
    {
        if (TryComp<SpriteComponent>(uid, out var sprite))
        {
            sprite.Color = sprite.Color.WithAlpha(0.3f);
            component.OriginalDrawDepth = sprite.DrawDepth;
            sprite.DrawDepth = (int)Content.Shared.DrawDepth.DrawDepth.Ghosts;
        }
    }

    private void OnShadowWalkShutdown(EntityUid uid, ShadowWalkingComponent component, ref ComponentShutdown args)
    {
        if (TryComp<SpriteComponent>(uid, out var sprite))
        {
            sprite.Color = sprite.Color.WithAlpha(1f);
            if (component.OriginalDrawDepth != 0)
                sprite.DrawDepth = component.OriginalDrawDepth;
        }
    }
}
