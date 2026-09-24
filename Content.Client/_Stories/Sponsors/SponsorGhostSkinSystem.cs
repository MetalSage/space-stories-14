using Content.Shared._Stories.Sponsors;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client._Stories.Sponsors;

public sealed partial class SponsorGhostSkinSystem : EntitySystem
{
    [Dependency] private IPrototypeManager _prototype = default!;
    [Dependency] private SpriteSystem _spriteSystem = default!;

    private static readonly ResPath DefaultGhostRsi = new("Mobs/Ghosts/ghost_human.rsi");
    private static readonly ResPath SponsorGhostRsi = new("_Stories/Mobs/Ghosts/sponsor.rsi");
    private const string DefaultGhostState = "animated";
    private static readonly Color DefaultGhostColor = Color.FromHex("#fff8");

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SponsorGhostSkinComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<SponsorGhostSkinComponent, AfterAutoHandleStateEvent>(OnState);
        SubscribeLocalEvent<SponsorGhostSkinComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<SponsorGhostSkinComponent> ent, ref ComponentStartup args)
    {
        UpdateSkin(ent);
    }

    private void OnState(Entity<SponsorGhostSkinComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        UpdateSkin(ent);
    }

    private void OnShutdown(Entity<SponsorGhostSkinComponent> ent, ref ComponentShutdown args)
    {
        ResetSkin(ent.Owner);
    }

    private void UpdateSkin(Entity<SponsorGhostSkinComponent> ent)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        if (string.IsNullOrEmpty(ent.Comp.Skin))
        {
            ResetSkin(ent.Owner, sprite);
            return;
        }

        var specifier = ResolveSkinSprite(ent.Comp.Skin);
        if (specifier != null)
        {
            _spriteSystem.LayerSetSprite((ent.Owner, sprite), 0, specifier);
            _spriteSystem.LayerSetColor((ent.Owner, sprite), 0, Color.White);
        }
    }

    private void ResetSkin(EntityUid uid, SpriteComponent? sprite = null)
    {
        if (!Resolve(uid, ref sprite, false))
            return;

        _spriteSystem.LayerSetSprite((uid, sprite), 0, new SpriteSpecifier.Rsi(DefaultGhostRsi, DefaultGhostState));
        _spriteSystem.LayerSetColor((uid, sprite), 0, DefaultGhostColor);
    }

    private SpriteSpecifier? ResolveSkinSprite(string skin)
    {
        if (_prototype.TryIndex<EntityPrototype>(skin, out var proto) &&
            proto.TryGetComponent<SponsorGhostSkinInfoComponent>(out var info, EntityManager.ComponentFactory))
        {
            return info.Sprite;
        }

        return null;
    }
}
