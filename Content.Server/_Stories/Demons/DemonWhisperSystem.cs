using Content.Server.Administration;
using Content.Server.Administration.Logs;
using Content.Server.Popups;
using Content.Shared._Stories.Demons;
using Content.Shared.Database;
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
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private IAdminLogManager _adminLogger = default!;

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

        if (!IsValidTarget(ent, target))
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

    private bool IsValidTarget(Entity<DemonWhisperComponent> ent, EntityUid target)
    {
        if (Deleted(target) || !_mobState.IsAlive(target) || !_mind.TryGetMind(target, out _, out _))
            return false;

        return _transform.InRange(Transform(ent).Coordinates, Transform(target).Coordinates, ent.Comp.Range);
    }

    private void SendWhisper(Entity<DemonWhisperComponent> ent, EntityUid target, string message)
    {
        if (Deleted(ent) || MetaData(ent).EntityPaused)
            return;

        if (!IsValidTarget(ent, target))
        {
            _popup.PopupEntity(Loc.GetString("demon-whisper-invalid-target"), ent, ent);
            return;
        }

        var trimmed = message.Trim();

        if (string.IsNullOrWhiteSpace(trimmed))
            return;

        if (trimmed.Length > ent.Comp.MaxMessageLength)
            trimmed = trimmed[..ent.Comp.MaxMessageLength];

        _adminLogger.Add(LogType.Chat,
            LogImpact.Medium,
            $"Demon whisper from {ToPrettyString(ent):user} to {ToPrettyString(target):target}: {trimmed}");

        _popup.PopupEntity(Loc.GetString("demon-whisper-sent", ("target", target)), ent, ent);
        _popup.PopupEntity(Loc.GetString("demon-whisper-received", ("message", trimmed)), target, target);
    }
}
