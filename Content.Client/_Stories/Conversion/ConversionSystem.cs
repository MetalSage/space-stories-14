using Content.Shared._Stories.Conversion;
using Content.Shared._Stories.Shadowling;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Content.Shared.Ghost;
using Robust.Client.Player;
using Robust.Shared.Prototypes;

namespace Content.Client._Stories.Conversion;

public sealed class ConversionSystem : SharedConversionSystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ConversionableComponent, GetStatusIconsEvent>(OnGetStatusIconsEvent);
    }

    private void OnGetStatusIconsEvent(EntityUid uid, ConversionableComponent component, ref GetStatusIconsEvent args)
    {
        var local = _player.LocalEntity;

        foreach (var (key, conversion) in component.ActiveConversions)
        {
            var proto = _prototype.Index(conversion.Prototype);
            if (proto.StatusIcon == null)
                continue;

            if (key == "STShadowlingThrall")
            {
                if (!HasComp<GhostComponent>(local) && !HasComp<ShadowlingComponent>(local) && !HasComp<ShadowlingThrallComponent>(local))
                    continue;
            }

            var iconProto = _prototype.Index<FactionIconPrototype>(proto.StatusIcon);
            args.StatusIcons.Add(iconProto);
        }
    }
}
