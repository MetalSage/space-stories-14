using Robust.Client.GameObjects;

using Content.Shared._Stories.Cards.Stack;
using Content.Shared.Examine;


namespace Content.Client._Stories.Cards.Stack;
public sealed class CardStackSystem : SharedCardStackSystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CardStackComponent, AppearanceChangeEvent>(OnAppearanceChanged);
        SubscribeLocalEvent<CardStackComponent, ExaminedEvent>(OnStackExamined);
    }
    private void OnAppearanceChanged(EntityUid uid, CardStackComponent comp, ref AppearanceChangeEvent args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        sprite.LayerSetVisible(0, false);
        if (_appearance.TryGetData<bool>(uid, CardStackVisuals.Shuffled, out var shuffled))
        {
            if (shuffled)
                _appearance.SetData(uid, CardStackVisuals.Shuffled, false);
        }
    }
    private void OnStackExamined(EntityUid uid, CardStackComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;
        args.PushMarkup(Loc.GetString("card-count-total", ("cardsTotal", component.CardContainer.ContainedEntities.Count)));
    }
}
