using Content.Server.Bible.Components;
using Content.Server.Body.Systems;
using Content.Server.Chat.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Chat.Prototypes;

namespace Content.Server._Stories.Chaplain;

public sealed partial class ChaplainSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly BodySystem _body = default!;

    [ValidatePrototypeId<EmotePrototype>]
    private const string FartingEmote = "Farting";
    private const float FartGibbingSearchRange = 0.5f;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BodyComponent, EmoteEvent>(OnEmote);
    }

    private void OnEmote(Entity<BodyComponent> entity, ref EmoteEvent args)
    {
        if (args.Emote.ID != FartingEmote)
            return;

        var ents = _lookup.GetEntitiesInRange<BibleComponent>(Transform(entity).Coordinates, FartGibbingSearchRange);
        if (ents.Count > 0)
            _body.GibBody(entity);
    }

}
