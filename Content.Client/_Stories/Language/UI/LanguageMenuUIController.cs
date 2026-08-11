using Content.Client._Stories.Language.Systems;
using Content.Client.Gameplay;
using Content.Client.UserInterface.Controls;
using Content.Client.UserInterface.Systems.MenuBar.Widgets;
using Content.Shared._Stories.Language;
using Content.Shared._Stories.Language.Components;
using Content.Shared._Stories.Language.Prototypes;
using Content.Shared.Input;
using JetBrains.Annotations;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input.Binding;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using static Robust.Client.UserInterface.Controls.BaseButton;

namespace Content.Client._Stories.Language.UI;

[UsedImplicitly]
public sealed partial class LanguageMenuUIController : UIController, IOnStateEntered<GameplayState>, IOnStateExited<GameplayState>
{
    [Dependency] private IEntityManager _ent = default!;
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private IPrototypeManager _prototypeManager = default!;

    [UISystemDependency] private readonly LanguageSystem _language = default!;

    private LanguageMenuWindow? _window;
    private MenuButton? LanguageButton => UIManager.GetActiveUIWidgetOrNull<GameTopMenuBar>()?.LanguageButton;

    public void OnStateEntered(GameplayState state)
    {
        DebugTools.Assert(_window == null);

        _window = UIManager.CreateWindow<LanguageMenuWindow>();
        LayoutContainer.SetAnchorPreset(_window, LayoutContainer.LayoutPreset.CenterTop);

        _window.OnClose += DeactivateButton;
        _window.OnOpen += ActivateButton;

        _language.OnLanguagesChanged += RefreshList;

        CommandBinds.Builder
            .Bind(ContentKeyFunctions.OpenLanguageMenu,
                InputCmdHandler.FromDelegate(_ => ToggleWindow()))
            .Register<LanguageMenuUIController>();
    }

    public void OnStateExited(GameplayState state)
    {
        _language.OnLanguagesChanged -= RefreshList;

        if (_window != null)
        {
            _window.Close();
            _window = null;
        }

        CommandBinds.Unregister<LanguageMenuUIController>();
    }

    public void LoadButton()
    {
        if (LanguageButton == null)
            return;

        LanguageButton.OnPressed += LanguageButtonPressed;
    }

    public void UnloadButton()
    {
        if (LanguageButton == null)
            return;

        LanguageButton.OnPressed -= LanguageButtonPressed;
    }

    private void DeactivateButton()
    {
        if (LanguageButton == null)
            return;

        LanguageButton.Pressed = false;
    }

    private void ActivateButton()
    {
        if (LanguageButton == null)
            return;

        LanguageButton.Pressed = true;
    }

    private void LanguageButtonPressed(ButtonEventArgs args)
    {
        ToggleWindow();
    }

    private void ToggleWindow()
    {
        if (_window == null)
            return;

        LanguageButton?.SetClickPressed(!_window.IsOpen);

        if (_window.IsOpen)
        {
            _window.Close();
        }
        else
        {
            RefreshList();
            _window.Open();
        }
    }

    private void RefreshList()
    {
        if (_window == null || !_window.IsOpen)
            return;

        _window.LanguageList.RemoveAllChildren();

        if (_player.LocalEntity is not { } player ||
            !_ent.TryGetComponent<LanguageComponent>(player, out var speaker))
        {
            return;
        }

        foreach (var languageId in speaker.SpokenLanguages)
        {
            if (!_prototypeManager.TryIndex(languageId, out var language))
                continue;

            var button = new Button
            {
                Text = language.LocalizedName,
                ToggleMode = true,
                Pressed = languageId == speaker.CurrentLanguage,
            };

            button.OnPressed += _ => SelectLanguage(languageId);

            _window.LanguageList.AddChild(button);
        }
    }

    private void SelectLanguage(ProtoId<LanguagePrototype> language)
    {
        _language.RequestSetLanguage(language);
    }
}
