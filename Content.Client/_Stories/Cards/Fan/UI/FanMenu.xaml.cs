using Robust.Client.GameObjects;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Content.Client.UserInterface.Controls;
using Content.Shared.Foldable;
using System.Numerics;
using Content.Shared._Stories.Cards.Card;
using Content.Shared._Stories.Cards.Stack;

namespace Content.Client._Stories.Cards.Fan.UI;

public sealed partial class FanMenu : RadialMenu
{
    [Dependency] private readonly EntityManager _entManager = default!;
    private EntityUid _owner;
    private EntityUid _user;
    private FanMenuBoundUserInterface? _boundUI;
    public Action<NetEntity, NetEntity>? OnCardSelectedMessageAction;

    public FanMenu(EntityUid uid, FanMenuBoundUserInterface boundUI, EntityUid user)
    {
        IoCManager.InjectDependencies(this);
        RobustXamlLoader.Load(this);
        _owner = uid;
        _boundUI = boundUI;
        _user = user;

        var main = FindControl<RadialContainer>("Main");
        main.RemoveAllChildren();

        if (!_entManager.TryGetComponent<CardStackComponent>(_owner, out var stackComp))
            return;

        foreach (var card in stackComp.CardContainer.ContainedEntities)
        {
            if (!TryGetCardComponents(card, out var cardComp, out var cardSprite, out var foldable, out var cardMeta) ||
                cardComp == null || cardSprite == null || foldable == null || cardMeta == null)
                continue;

            var cardName = foldable.IsFolded ? cardComp.Name : cardMeta.EntityName;
            var cardLayer = cardSprite.LayerGetState(1);

            var button = new FanMenuButton()
            {
                StyleClasses = { "RadialMenuButton" },
                SetSize = new Vector2(64f, 64f),
                ToolTip = cardName,
            };

            var rsi = cardSprite.BaseRSI;
            if (rsi == null)
                continue;
            rsi.TryGetState(cardLayer, out var layer);
            if (layer == null)
                continue;
            var texture = layer.Frame0;

            if (cardLayer != null)
            {
                var tex = new TextureRect()
                {
                    VerticalAlignment = VAlignment.Center,
                    HorizontalAlignment = HAlignment.Center,
                    Texture = texture,
                    TextureScale = new Vector2(2f, 2f),
                };
                button.AddChild(tex);
            }
            button.SetCard(card);
            main.AddChild(button);

            button.OnPressed += _ =>
            {
                OnCardSelectedMessageAction?.Invoke(_entManager.GetNetEntity(card), _entManager.GetNetEntity(_user));
                Close();
            };
        }

        if (_boundUI == null)
            return;
        OnCardSelectedMessageAction += _boundUI.OnCardSelected;
    }

    private bool TryGetCardComponents(EntityUid card,
        out CardComponent? cardComp,
        out SpriteComponent? cardSprite,
        out FoldableComponent? foldable,
        out MetaDataComponent? cardMeta)
    {
        cardComp = null;
        cardSprite = null;
        foldable = null;
        cardMeta = null;

        return _entManager.TryGetComponent(card, out cardComp) &&
               _entManager.TryGetComponent(card, out cardSprite) &&
               _entManager.TryGetComponent(card, out foldable) &&
               _entManager.TryGetComponent(card, out cardMeta);
    }
}

public sealed class FanMenuButton : RadialMenuTextureButton
{
    private EntityUid _cardEntity;

    public void SetCard(EntityUid cardEntity)
    {
        _cardEntity = cardEntity;
    }
}
