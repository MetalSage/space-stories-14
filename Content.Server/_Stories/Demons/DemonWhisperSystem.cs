using Content.Server.Administration;
using Content.Server.Popups;
using Content.Shared._Stories.Demons;
using Content.Shared.Mind;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Player;

namespace Content.Server._Stories.Demons;

public sealed partial class DemonWhisperSystem : EntitySystem
{
    [Dependency] private PopupSystem _popup = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private SharedMindSystem _mind = default!;
    [Dependency] private QuickDialogSystem _quickDialog = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DemonWhisperComponent, DemonWhisperActionEvent>(OnWhisper);
    }

    private void OnWhisper(Entity<DemonWhisperComponent> ent, ref DemonWhisperActionEvent args)
    {
        if (args.Handled)
            return;

        var target = args.Target;

        if (!_mobState.IsAlive(target) || !_mind.TryGetMind(target, out _, out _))
        {
            _popup.PopupEntity(Loc.GetString("demon-whisper-invalid-target"), ent, ent);
            return;
        }

        if (!TryComp<ActorComponent>(ent, out var actor))
            return;

        args.Handled = true;

        _quickDialog.OpenDialog<string>(actor.PlayerSession,
            Loc.GetString("demon-whisper-dialog-title"),
            Loc.GetString("demon-whisper-dialog-prompt"),
            message => SendWhisper(ent, target, message));
    }

    private void SendWhisper(EntityUid demon, EntityUid target, string message)
    {
        if (Deleted(demon) || Deleted(target) || string.IsNullOrWhiteSpace(message))
            return;

        _popup.PopupEntity(Loc.GetString("demon-whisper-sent", ("target", target)), demon, demon);
        _popup.PopupEntity(Loc.GetString("demon-whisper-received", ("message", message)), target, target);
    }
}
