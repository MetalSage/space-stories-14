using System;
using Content.Server._Stories.TTS;
using Content.Shared.Chat;
using Content.Shared.Database;
using Content.Shared.Station.Components;
using Robust.Shared.Audio;
using Robust.Shared.Enums;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server.Chat.Systems;

public sealed partial class ChatSystem
{
    /// <inheritdoc />
    public override void DispatchGlobalAnnouncement(
        string message,
        string? sender = null,
        bool playSound = true,
        SoundSpecifier? announcementSound = null,
        Color? colorOverride = null,
        string? ttsVoice = null,
        string? ttsMessage = null
        )
    {
        sender ??= Loc.GetString("chat-manager-sender-announcement");

        var wrappedMessage = Loc.GetString("chat-manager-sender-announcement-wrap-message", ("sender", sender), ("message", FormattedMessage.EscapeText(message)));
        _chatManager.ChatMessageToAll(ChatChannel.Radio, message, wrappedMessage, default, false, true, colorOverride);
        if (playSound)
        {
            if (sender == Loc.GetString("admin-announce-announcer-default")) announcementSound = new SoundPathSpecifier(CentComAnnouncementSound); // Corvax-Announcements: Support custom alert sound from admin panel
            _audio.PlayGlobal(announcementSound ?? new SoundPathSpecifier(DefaultAnnouncementSound), Filter.Broadcast(), true, AudioParams.Default.WithVolume(-2f));
        }
        _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Global station announcement from {sender}: {message}");

        // Stories-TTS-Start
        var ttsFilter = Filter.Empty().AddWhere(s => s.Status == SessionStatus.InGame);
        PlayTtsAnnouncement(ttsMessage ?? message, ttsFilter, ttsVoice);
        // Stories-TTS-End
    }

    /// <inheritdoc />
    public override void DispatchFilteredAnnouncement(
        Filter filter,
        string message,
        EntityUid? source = null,
        string? sender = null,
        bool playSound = true,
        SoundSpecifier? announcementSound = null,
        Color? colorOverride = null,
        string? ttsVoice = null,
        string? ttsMessage = null)
    {
        sender ??= Loc.GetString("chat-manager-sender-announcement");

        var wrappedMessage = Loc.GetString("chat-manager-sender-announcement-wrap-message", ("sender", sender), ("message", FormattedMessage.EscapeText(message)));
        _chatManager.ChatMessageToManyFiltered(filter, ChatChannel.Radio, message, wrappedMessage, source ?? default, false, true, colorOverride);
        if (playSound)
        {
            _audio.PlayGlobal(announcementSound ?? new SoundPathSpecifier(DefaultAnnouncementSound), filter, true, AudioParams.Default.WithVolume(-2f));
        }
        _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Station Announcement from {sender}: {message}");

        PlayTtsAnnouncement(ttsMessage ?? message, filter, ttsVoice); // Stories-TTS
    }

    /// <inheritdoc />
    public override void DispatchStationAnnouncement(
        EntityUid source,
        string message,
        string? sender = null,
        bool playDefaultSound = true,
        SoundSpecifier? announcementSound = null,
        Color? colorOverride = null,
        string? ttsVoice = null,
        string? ttsMessage = null)
    {
        sender ??= Loc.GetString("chat-manager-sender-announcement");

        var wrappedMessage = Loc.GetString("chat-manager-sender-announcement-wrap-message", ("sender", sender), ("message", FormattedMessage.EscapeText(message)));
        var station = _stationSystem.GetOwningStation(source);

        if (station == null)
        {
            // you can't make a station announcement without a station
            return;
        }

        if (!TryComp<StationDataComponent>(station, out var stationDataComp)) return;

        var filter = _stationSystem.GetInStation(stationDataComp);

        _chatManager.ChatMessageToManyFiltered(filter, ChatChannel.Radio, message, wrappedMessage, source, false, true, colorOverride);

        if (playDefaultSound)
        {
            _audio.PlayGlobal(announcementSound ?? new SoundPathSpecifier(DefaultAnnouncementSound), filter, true, AudioParams.Default.WithVolume(-2f));
        }

        _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Station Announcement on {station} from {sender}: {message}");

        PlayTtsAnnouncement(ttsMessage ?? message, filter, ttsVoice); // Stories-TTS
    }

    // Stories-TTS-Start
    private void PlayTtsAnnouncement(string message, Filter filter, string? ttsVoice = null)
    {
        if (string.IsNullOrEmpty(ttsVoice) || ttsVoice.ToLowerInvariant() is "none" or "off")
            return;

        var cleanMessage = StripSentByFooter(message);

        if (string.IsNullOrWhiteSpace(cleanMessage))
            return;

        Timer.Spawn(TimeSpan.FromSeconds(5), () =>
        {
            var tts = EntityManager.System<TTSSystem>();
            tts.PlayGlobalTTS(cleanMessage, ttsVoice, filter, isAnnounce: true);
        });
    }

    private string StripSentByFooter(string message)
    {
        var sentByPrefix = Loc.GetString("comms-console-announcement-sent-by");
        var lines = message.Split('\n');
        for (var i = lines.Length - 1; i >= 0; i--)
        {
            var trimmed = lines[i].Trim();
            if (string.IsNullOrEmpty(trimmed))
                continue;

            if (trimmed.StartsWith(sentByPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return string.Join('\n', lines[..i]).Trim();
            }

            break;
        }

        return message.Trim();
    }
    // Stories-TTS-End
}
