using Robust.Client.GameObjects;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Content.Client.UserInterface.Controls;
using Content.Shared.Foldable;
using System.Numerics;
using System.Linq;

using Content.Shared._Stories.Cards.Stack;
using Content.Shared._Stories.Cards.Fan;

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

        foreach (var child in main.Children.ToList())
        {
            main.RemoveChild(child);
        }


        if (!_entManager.TryGetComponent<CardFanComponent>(_owner, out var fanComp))
            return;
        if (!_entManager.TryGetComponent<CardStackComponent>(_owner, out var stackComp))
            return;

        foreach (var card in stackComp.Cards)
        {
            if (!_entManager.TryGetComponent<SpriteComponent>(card, out var cardSprite))
                continue;
            if (!_entManager.TryGetComponent<FoldableComponent>(card, out var foldable))
                continue;

            var button = new FanMenuButton()
            {
                StyleClasses = { "RadialMenuButton" },
                SetSize = new Vector2(64f, 64f),
            };


            var cardLayer = cardSprite.LayerGetState(1);

            var rsi = cardSprite.BaseRSI;
            if (rsi == null)
                return;
            rsi.TryGetState(cardLayer, out var layer);
            if (layer == null)
                return;
            var t = layer.Frame0;

            if (cardLayer != null)
            {
                var tex = new TextureRect()
                {
                    VerticalAlignment = VAlignment.Center,
                    HorizontalAlignment = HAlignment.Center,
                    Texture = t,
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
}
public sealed class FanMenuButton : RadialMenuTextureButton
{
    private EntityUid _cardEntity;

    public void SetCard(EntityUid cardEntity)
    {
        _cardEntity = cardEntity;
    }
}
