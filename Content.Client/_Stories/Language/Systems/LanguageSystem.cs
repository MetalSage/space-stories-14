using Content.Shared._Stories.Language;
using Content.Shared._Stories.Language.Components;
using Content.Shared._Stories.Language.Prototypes;
using Content.Shared._Stories.Language.Systems;
using Robust.Client.Player;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Client._Stories.Language.Systems;

public sealed partial class LanguageSystem : SharedLanguageSystem
{
    [Dependency] private IPlayerManager _player = default!;

    public event Action? OnLanguagesChanged;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LanguageComponent, AfterAutoHandleStateEvent>(OnLanguageAfterState);
    }

    private void OnLanguageAfterState(Entity<LanguageComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (ent.Owner == _player.LocalEntity)
            OnLanguagesChanged?.Invoke();
    }

    public void RequestSetLanguage(ProtoId<LanguagePrototype> language)
    {
        if (CompOrNull<LanguageComponent>(_player.LocalEntity)?.CurrentLanguage == language)
            return;

        RaiseNetworkEvent(new LanguagesSetMessage(language));
    }
}
