using Content.Shared._Stories.Cards.Deck;
using Content.Shared.Cabinet;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Robust.Client.GameObjects;

namespace Content.Client._Stories.Cards.Deck;

public sealed class CardDeckSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CardDeckBoxComponent, AppearanceChangeEvent>(OnAppearanceBoxChanged);
        SubscribeLocalEvent<CardDeckBoxComponent, OpenableOpenedEvent>(OnOpenedBox);
    }

    private void OnOpenedBox(EntityUid uid, CardDeckBoxComponent comp, OpenableOpenedEvent args)
    {
        if (!_appearance.TryGetData<bool>(uid, ItemCabinetVisuals.ContainsItem, out var value))
            return;

        if (value)
            _appearance.SetData(uid, CardDeckVisuals.InBox, true);
        else
            _appearance.SetData(uid, CardDeckVisuals.InBox, false);
    }

    private void OnAppearanceBoxChanged(EntityUid uid, CardDeckBoxComponent comp, ref AppearanceChangeEvent args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite)
            || !_appearance.TryGetData<bool>(uid, ItemCabinetVisuals.ContainsItem, out var containsItem)
            || !_appearance.TryGetData<bool>(uid, OpenableVisuals.Opened, out var opened))
            return;

        if (containsItem && opened)
            sprite.LayerSetVisible(1, true);
        else
            sprite.LayerSetVisible(1, false);
    }
}
