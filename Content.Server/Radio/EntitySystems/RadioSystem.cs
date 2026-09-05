using System.Linq;
using Content.Server._Stories.Language.Systems;
using Content.Server._Stories.TTS;
using Content.Shared._Stories.Language.Components;
using Content.Server.Administration.Logs;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.Ghost;
using Content.Server.Power.Components;
using Content.Shared._Stories.SCCVars;
using Content.Shared._Stories.TTS;
using Content.Shared.Chat;
using Content.Shared.Database;
using Content.Shared.Radio;
using Content.Shared.Radio.Components;
using Content.Shared.Radio.EntitySystems;
using Content.Shared.Speech;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Replays;
using Robust.Shared.Utility;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;

namespace Content.Server.Radio.EntitySystems;

/// <inheritdoc/>
public sealed partial class RadioSystem : SharedRadioSystem
{
    [Dependency] private INetManager _netMan = default!;
    [Dependency] private IReplayRecordingManager _replay = default!;
    [Dependency] private IAdminLogManager _adminLogger = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private ChatSystem _chat = default!;
    [Dependency] private IChatManager _chatManager = default!;
    [Dependency] private GhostSystem _ghost = default!;
    [Dependency] private EntityQuery<TelecomExemptComponent> _exemptQuery = default!;
    [Dependency] private LanguageSystem _language = default!; // Stories-Language

    // Stories-TTS Start
    [Dependency] private TTSSystem _tts = default!;
    [Dependency] private TtsAudioProcessingSystem _ttsProcessing = default!;
    [Dependency] private IConfigurationManager _cfg = default!;
    // Stories-TTS End

    // set used to prevent radio feedback loops.
    private readonly HashSet<string> _messages = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<IntrinsicRadioReceiverComponent, RadioReceiveEvent>(OnIntrinsicReceive);
        SubscribeLocalEvent<IntrinsicRadioTransmitterComponent, EntitySpokeEvent>(OnIntrinsicSpeak);
    }

    private void OnIntrinsicSpeak(EntityUid uid, IntrinsicRadioTransmitterComponent component, EntitySpokeEvent args)
    {
        if (args.Channel != null && component.Channels.Contains(args.Channel.ID))
        {
            SendRadioMessage(uid, args.Message, args.Channel, uid);
            args.Channel = null; // prevent duplicate messages from other listeners.
        }
    }

    private void OnIntrinsicReceive(EntityUid uid, IntrinsicRadioReceiverComponent component, ref RadioReceiveEvent args)
    {
        if (!TryComp(uid, out ActorComponent? actor))
            return;

        var playerSession = actor.PlayerSession;
        if (playerSession.Status != SessionStatus.InGame)
            return;

        var msg = args.ChatMsg;
        if (_ghost.CanGhostWarp(playerSession, out _))
        {
            msg = new MsgChatMessage
            {
                Message = new ChatMessage(args.ChatMsg.Message)
                {
                    WrappedMessage = _chatManager.PrependFollowButtonIfAppropriate(
                        args.ChatMsg.Message.WrappedMessage,
                        args.MessageSource,
                        playerSession.Channel),
                },
            };
        }

        _netMan.ServerSendMessage(msg, playerSession.Channel);
    }

    // Stories-TTS Start
    private async void ProcessAndSendRadioTts(EntityUid messageSource, string message, RadioChannelPrototype channel, IEnumerable<ICommonSession> recipients)
    {
        if (!_cfg.GetCVar(SCCVars.TTSEnabled))
            return;

        var voiceId = GetVoiceId(messageSource);
        var soundData = await _tts.GenerateTTS(message, voiceId);

        if (soundData == null)
            return;

        byte[] processedSoundData = await _ttsProcessing.ApplyRadioEffect(soundData);

        var ttsEvent = new PlayTTSEvent(processedSoundData, sourceUid: null, isWhisper: false, originalSourceUid: GetNetEntity(messageSource));

        var filter = Filter.Empty().AddPlayers(recipients.ToList());
        RaiseNetworkEvent(ttsEvent, filter);
    }

    private string GetVoiceId(EntityUid sourceUid)
    {
        if (TryComp<TTSComponent>(sourceUid, out var tts) && !string.IsNullOrEmpty(tts.VoicePrototypeId) &&
            ProtoMan.TryIndex<TTSVoicePrototype>(tts.VoicePrototypeId, out var protoVoice))
        {
            return protoVoice.Speaker;
        }
        return "father_grigori";
    }
    // Stories-TTS End

    /// <inheritdoc/>
    public override void SendRadioMessage(EntityUid messageSource, string message, RadioChannelPrototype channel, EntityUid radioSource, bool escapeMarkup = true)
    {
        // TODO if radios ever garble / modify messages, feedback-prevention needs to be handled better than this.
        if (!_messages.Add(message))
            return;

        var evt = new TransformSpeakerNameEvent(messageSource, MetaData(messageSource).EntityName);
        RaiseLocalEvent(messageSource, evt);

        var name = evt.VoiceName;
        name = FormattedMessage.EscapeText(name);

        SpeechVerbPrototype speech;
        if (evt.SpeechVerb != null && ProtoMan.Resolve(evt.SpeechVerb, out var evntProto))
            speech = evntProto;
        else
            speech = _chat.GetSpeechVerb(messageSource, message);

        // Stories-Language-Start
        var language = _language.GetCurrentLanguage(messageSource);

        if (ProtoMan.TryIndex(language, out var languageProto) && !languageProto.CanUseRadio)
        {
            _messages.Remove(message);
            return;
        }
        // Stories-Language-End

        var content = escapeMarkup
            ? FormattedMessage.EscapeText(message)
            : message;

        var wrappedMessage = Loc.GetString(speech.Bold ? "chat-radio-message-wrap-bold" : "chat-radio-message-wrap",
            ("color", channel.Color),
            ("fontType", speech.FontId),
            ("fontSize", speech.FontSize),
            ("verb", Loc.GetString(_random.Pick(speech.SpeechVerbStrings))),
            ("channel", $"\\[{channel.LocalizedName}\\]"),
            ("name", name),
            ("message", _language.ColorizeMessage(content, language)));

        // most radios are relayed to chat, so lets parse the chat message beforehand
        var chat = new ChatMessage(
            ChatChannel.Radio,
            message,
            wrappedMessage,
            NetEntity.Invalid,
            null);
        var chatMsg = new MsgChatMessage { Message = chat };
        var ev = new RadioReceiveEvent(message, messageSource, channel, radioSource, chatMsg);

        var sendAttemptEv = new RadioSendAttemptEvent(channel, radioSource);
        RaiseLocalEvent(ref sendAttemptEv);
        RaiseLocalEvent(radioSource, ref sendAttemptEv);
        var canSend = !sendAttemptEv.Cancelled;

        var sourceMapId = Transform(radioSource).MapID;
        var hasActiveServer = HasActiveServer(sourceMapId, channel.ID);
        var sourceServerExempt = _exemptQuery.HasComp(radioSource);

        var ttsUnderstood = new List<EntityUid>(); // Stories-TTS
        var ttsConfused = new List<EntityUid>(); // Stories-TTS

        var radioQuery = EntityQueryEnumerator<ActiveRadioComponent, TransformComponent>();
        while (canSend && radioQuery.MoveNext(out var receiver, out var radio, out var transform))
        {
            if (!radio.ReceiveAllChannels)
            {
                if (!radio.Channels.Contains(channel.ID) || (TryComp<IntercomComponent>(receiver, out var intercom) &&
                                                             !intercom.SupportedChannels.Contains(channel.ID)))
                    continue;
            }

            if (!channel.LongRange && transform.MapID != sourceMapId && !radio.GlobalReceive)
                continue;

            // don't need telecom server for long range channels or handheld radios and intercoms
            var needServer = !channel.LongRange && !sourceServerExempt;
            if (needServer && !hasActiveServer)
                continue;

            // check if message can be sent to specific receiver
            var attemptEv = new RadioReceiveAttemptEvent(channel, radioSource, receiver);
            RaiseLocalEvent(ref attemptEv);
            RaiseLocalEvent(receiver, ref attemptEv);
            if (attemptEv.Cancelled)
                continue;

            // send the message
            // Stories-Language-Start
            float comprehension;

            if (IsRelaySpeaker(receiver))
            {
                _language.SetRelayLanguage(receiver, language);
                comprehension = 1f;
            }
            else
            {
                var listener = ResolveLanguageListener(receiver);
                comprehension = listener is null ? 1f : _language.GetComprehension(listener.Value, language);
            }

            if (comprehension >= 1f)
            {
                RaiseLocalEvent(receiver, ref ev);
                ttsUnderstood.Add(receiver); // Stories-TTS
            }
            else
            {
                var listenerMessage = _language.ObfuscateMessage(message, language, comprehension);
                var listenerContent = escapeMarkup
                    ? FormattedMessage.EscapeText(listenerMessage)
                    : listenerMessage;
                var listenerWrapped = Loc.GetString(speech.Bold ? "chat-radio-message-wrap-bold" : "chat-radio-message-wrap",
                    ("color", channel.Color),
                    ("fontType", speech.FontId),
                    ("fontSize", speech.FontSize),
                    ("verb", Loc.GetString(_random.Pick(speech.SpeechVerbStrings))),
                    ("channel", $"\\[{channel.LocalizedName}\\]"),
                    ("name", name),
                    ("message", _language.ColorizeMessage(listenerContent, language)));
                var listenerChat = new ChatMessage(ChatChannel.Radio, listenerMessage, listenerWrapped, NetEntity.Invalid, null);
                var listenerChatMsg = new MsgChatMessage { Message = listenerChat };
                var evListener = new RadioReceiveEvent(listenerMessage, messageSource, channel, radioSource, listenerChatMsg);
                RaiseLocalEvent(receiver, ref evListener);
                ttsConfused.Add(receiver); // Stories-TTS
            }
            // Stories-Language-End
        }

        // Stories-TTS Start
        if (canSend)
        {
            var actorQuery = GetEntityQuery<ActorComponent>();

            var understoodSessions = ResolveTtsSessions(ttsUnderstood, actorQuery);
            if (understoodSessions.Count > 0)
                ProcessAndSendRadioTts(messageSource, message, channel, understoodSessions);

            var confusedSessions = ResolveTtsSessions(ttsConfused, actorQuery);
            if (confusedSessions.Count > 0)
                ProcessAndSendRadioTts(messageSource, _language.ObfuscateMessage(message, language), channel, confusedSessions);
        }
        // Stories-TTS End

        if (name != Name(messageSource))
            _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Radio message from {ToPrettyString(messageSource):user} as {name} on {channel.LocalizedName}: {message}");
        else
            _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Radio message from {ToPrettyString(messageSource):user} on {channel.LocalizedName}: {message}");

        _replay.RecordServerMessage(chat);
        _messages.Remove(message);
    }

    // Stories-Language-Start
    private bool IsRelaySpeaker(EntityUid receiver)
    {
        return HasComp<RadioSpeakerComponent>(receiver);
    }


    private EntityUid? ResolveLanguageListener(EntityUid receiver)
    {
        if (HasComp<LanguageComponent>(receiver))
            return receiver;

        var wearer = Transform(receiver).ParentUid;
        if (wearer.IsValid() && TryComp<WearingHeadsetComponent>(wearer, out var wearing) && wearing.Headset == receiver)
            return wearer;

        return null;
    }

    private HashSet<ICommonSession> ResolveTtsSessions(IReadOnlyList<EntityUid> recipients, EntityQuery<ActorComponent> actorQuery)
    {
        var sessions = new HashSet<ICommonSession>();
        foreach (var uid in recipients)
        {
            var parent = Transform(uid).ParentUid;
            var target = actorQuery.HasComponent(uid) ? uid : (actorQuery.HasComponent(parent) ? parent : (EntityUid?)null);

            if (target.HasValue && actorQuery.TryGetComponent(target.Value, out var actor))
            {
                if (actor.PlayerSession.Status == SessionStatus.InGame)
                    sessions.Add(actor.PlayerSession);
            }
        }

        return sessions;
    }
    // Stories-Language-End

    /// <inheritdoc cref="TelecomServerComponent"/>
    private bool HasActiveServer(MapId mapId, string channelId)
    {
        var servers = EntityQuery<TelecomServerComponent, EncryptionKeyHolderComponent, ApcPowerReceiverComponent, TransformComponent>();
        foreach (var (_, keys, power, transform) in servers)
        {
            if (transform.MapID == mapId &&
                power.Powered &&
                keys.Channels.Contains(channelId))
            {
                return true;
            }
        }
        return false;
    }
}
