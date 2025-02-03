using Robust.Client.GameObjects;

using Content.Shared._Stories.Cards.Stack;


namespace Content.Client._Stories.Cards.Stack;
public sealed class CardStackSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CardStackComponent, AppearanceChangeEvent>(OnAppearanceChanged);
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
}
