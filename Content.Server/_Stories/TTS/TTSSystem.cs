using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Content.Server.Chat.Systems;
using Content.Server.Radio.EntitySystems;
using Content.Server._Stories.Language.Systems;
using Content.Server._Stories.Sponsors;
using Content.Shared._Stories.Language.Prototypes;
using Content.Shared._Stories.SCCVars;
using Content.Shared._Stories.TTS;
using Content.Shared.Chat;
using Content.Shared.GameTicking;
using Content.Shared.Humanoid;
using Content.Shared.Implants;
using Content.Shared.Inventory;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Robust.Shared.Configuration;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Stories.TTS;

public sealed partial class TTSSystem : EntitySystem
{
    private const int MaxMessageChars = 100 * 2;

    private static readonly ProtoId<TTSVoicePrototype> FatherGrigoriId = "father_grigori";

    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private InventorySystem _inventory = default!;
    [Dependency] private LanguageSystem _language = default!;
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SponsorsManager _sponsors = default!;
    [Dependency] private TtsAudioProcessingSystem _ttsAudio = default!;
    [Dependency] private TTSManager _ttsManager = default!;

    private bool _isEnabled;

    private readonly List<string> _sampleText =
        new()
        {
            "Съешь же ещё этих мягких французских булок, да выпей чаю.",
            "Клоун, прекрати разбрасывать банановые кожурки офицерам под ноги!",
            "Капитан, вы уверены что хотите назначить клоуна на должность главы персонала?",
            "Эс Бэ! Тут человек в сером костюме, с тулбоксом и в маске! Помогите!!",
            "Учёные, тут странная аномалия в баре! Она уже съела мима!",
            "Я надеюсь что инженеры внимательно следят за сингулярностью...",
            "Вы слышали эти странные крики в техах? Мне кажется туда ходить небезопасно.",
            "Вы не видели Гамлета? Мне кажется он забегал к вам на кухню.",
            "Здесь есть доктор? Человек умирает от отравленного пончика! Нужна помощь!",
            "Вам нужно согласие и печать квартирмейстера, если вы хотите сделать заказ на партию дробовиков.",
            "Возле эвакуационного шаттла разгерметизация! Инженеры, нам срочно нужна ваша помощь!",
            "Бармен, налей мне самого крепкого вина, которое есть в твоих запасах!",
        };

    public override void Initialize()
    {
        _cfg.OnValueChanged(SCCVars.TTSEnabled, v => _isEnabled = v, true);

        SubscribeLocalEvent<TransformSpeechEvent>(OnTransformSpeech);
        SubscribeLocalEvent<TTSComponent, EntitySpokeEvent>(OnEntitySpoke, new[] { typeof(HeadsetSystem) });
        SubscribeLocalEvent<TTSPlaybackModifierComponent, GetTTSPlaybackModifiersEvent>(OnGetTTSPlaybackModifiers);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestartCleanup);

        SubscribeNetworkEvent<RequestPreviewTTSEvent>(OnRequestPreviewTTS);

        InitializeSanitize();
    }

    public override void Shutdown()
    {
        base.Shutdown();
        ShutdownSanitize();
    }

    private void OnRoundRestartCleanup(RoundRestartCleanupEvent ev)
    {
        _ttsManager.ResetCache();
    }

    private async void OnRequestPreviewTTS(RequestPreviewTTSEvent ev, EntitySessionEventArgs args)
    {
        if (!_isEnabled ||
            !_proto.TryIndex<TTSVoicePrototype>(ev.VoiceId, out var protoVoice))
            return;

        var previewText = _random.Pick(_sampleText);
        var soundData = await GenerateTTS(previewText, protoVoice.Speaker);
        if (soundData is null)
            return;

        RaiseNetworkEvent(new PlayTTSEvent(soundData, previewText), Filter.SinglePlayer(args.SenderSession));
    }

    private bool ValidateVoiceForEntity(EntityUid uid, TTSVoicePrototype voice)
    {
        if (voice.Sex != Sex.Unsexed)
        {
            if (TryComp<HumanoidProfileComponent>(uid, out var humanoid))
            {
                if (humanoid.Sex != voice.Sex)
                    return false;
            }
        }


        if (voice.SponsorOnly)
        {
            NetUserId? userId = null;
            if (TryComp<ActorComponent>(uid, out var actor))
                userId = actor.PlayerSession.UserId;
            else if (TryComp<MindContainerComponent>(uid, out var mindContainer) && TryComp<MindComponent>(mindContainer.Mind, out var mind))
                userId = mind.UserId;

            if (userId == null)
                return false;

            if (_sponsors.TryGetInfo(userId.Value, out var sponsorInfo))
            {
                if (sponsorInfo.AllowedTTSVoices == null || !sponsorInfo.AllowedTTSVoices.Contains(voice.ID))
                    return false;
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    private bool GetVoicePrototype(string voiceId, [NotNullWhen(true)] out TTSVoicePrototype? voicePrototype)
    {
        if (!_proto.TryIndex(voiceId, out voicePrototype))
            return _proto.TryIndex(FatherGrigoriId, out voicePrototype);

        return true;
    }

    private void OnEntitySpoke(Entity<TTSComponent> ent, ref EntitySpokeEvent args)
    {
        if (args.Channel != null)
            return;

        var uid = ent.Owner;
        var component = ent.Comp;
        var voiceId = component.VoicePrototypeId;
        if (args.Message.Length > MaxMessageChars || voiceId == null)
            return;

        var voiceEv = new TransformSpeakerVoiceEvent(uid, voiceId);
        RaiseLocalEvent(uid, voiceEv);

        if (TryComp<InventoryComponent>(uid, out var inventory))
            _inventory.RelayEvent((uid, inventory), ref voiceEv);

        if (_container.TryGetContainer(uid, "implant", out var implantContainer))
        {
            var relayEv = new ImplantRelayEvent<TransformSpeakerVoiceEvent>(voiceEv, uid);
            foreach (var implant in implantContainer.ContainedEntities)
            {
                RaiseLocalEvent(implant, relayEv);
            }
        }

        voiceId = voiceEv.VoiceId;

        if (voiceId == null || !GetVoicePrototype(voiceId, out var protoVoice) || !ValidateVoiceForEntity(uid, protoVoice))
        {
            if (!GetVoicePrototype(FatherGrigoriId, out protoVoice))
                return;
        }

        var messageToUse = args.Message;
        if (messageToUse.Contains('\u200B'))
            return;

        var language = _language.GetCurrentLanguage(uid);

        if (args.ObfuscatedMessage != null)
        {
            var playbackModifiers = GetPlaybackModifiers(uid, ChatSystem.WhisperClearRange);
            HandleWhisper(uid, messageToUse, protoVoice.Speaker, language, playbackModifiers);
            return;
        }

        HandleSay(uid, messageToUse, protoVoice.Speaker, language, GetPlaybackModifiers(uid, ChatSystem.VoiceRange));
    }

    private void OnGetTTSPlaybackModifiers(Entity<TTSPlaybackModifierComponent> ent, ref GetTTSPlaybackModifiersEvent args)
    {
        args.AddVolumeMultiplier(ent.Comp.VolumeMultiplier);
        args.AddRangeMultiplier(ent.Comp.RangeMultiplier);
        args.AddAudioEffect(ent.Comp.AudioEffects);

        if (ent.Comp.MaxDistance != null)
            args.SetMaxDistance(ent.Comp.MaxDistance.Value);

        if (ent.Comp.ReferenceDistance != null)
            args.SetReferenceDistance(ent.Comp.ReferenceDistance.Value);

        if (ent.Comp.RolloffFactor != null)
            args.SetRolloffFactor(ent.Comp.RolloffFactor.Value);
    }

    private TTSPlaybackModifiers GetPlaybackModifiers(EntityUid uid, float baseRange)
    {
        var ev = new GetTTSPlaybackModifiersEvent(baseRange);
        RaiseLocalEvent(uid, ev);

        return new TTSPlaybackModifiers(
            ev.HasVolumeOverride ? ev.VolumeMultiplier : 1f,
            ev.HasDistanceOverride ? ev.EffectiveMaxDistance : (float?) null,
            ev.HasSpatialOverride ? ev.ReferenceDistance : null,
            ev.HasSpatialOverride ? ev.RolloffFactor : null,
            ev.HasAudioEffects ? ev.AudioEffects : TTSAudioEffect.None);
    }

    private async void HandleSay(
        EntityUid uid,
        string message,
        string speaker,
        ProtoId<LanguagePrototype> language,
        TTSPlaybackModifiers playbackModifiers)
    {
        var effectiveRange = playbackModifiers.MaxDistanceOverride ?? ChatSystem.VoiceRange;
        SplitListenersByComprehension(uid, language, effectiveRange, out var understood, out var confused);

        if (understood.Count > 0)
        {
            var soundData = await GenerateTTS(message, speaker);
            if (soundData is not null)
            {
                soundData = await _ttsAudio.ApplyPlaybackEffects(soundData, playbackModifiers.AudioEffects);
                var ev = new PlayTTSEvent(
                    soundData,
                    message,
                    GetNetEntity(uid),
                    volumeMultiplier: playbackModifiers.VolumeMultiplier,
                    maxDistanceOverride: playbackModifiers.MaxDistanceOverride,
                    referenceDistanceOverride: playbackModifiers.ReferenceDistanceOverride,
                    rolloffFactorOverride: playbackModifiers.RolloffFactorOverride);
                RaiseNetworkEvent(ev, Filter.Empty().AddPlayers(understood));
            }
        }

        if (confused.Count > 0)
        {
            var soundData = await GenerateTTS(_language.ObfuscateMessage(message, language), speaker);
            if (soundData is not null)
            {
                soundData = await _ttsAudio.ApplyPlaybackEffects(soundData, playbackModifiers.AudioEffects);
                var ev = new PlayTTSEvent(
                    soundData,
                    message,
                    GetNetEntity(uid),
                    volumeMultiplier: playbackModifiers.VolumeMultiplier,
                    maxDistanceOverride: playbackModifiers.MaxDistanceOverride,
                    referenceDistanceOverride: playbackModifiers.ReferenceDistanceOverride,
                    rolloffFactorOverride: playbackModifiers.RolloffFactorOverride);
                RaiseNetworkEvent(ev, Filter.Empty().AddPlayers(confused));
            }
        }
    }

    private async void HandleWhisper(
        EntityUid uid,
        string message,
        string speaker,
        ProtoId<LanguagePrototype> language,
        TTSPlaybackModifiers playbackModifiers)
    {
        var effectiveRange = playbackModifiers.MaxDistanceOverride ?? ChatSystem.WhisperClearRange;
        SplitListenersByComprehension(uid, language, effectiveRange, out var understood, out var confused);

        if (understood.Count > 0)
        {
            var soundData = await GenerateTTS(message, speaker, true);
            if (soundData is not null)
            {
                soundData = await _ttsAudio.ApplyPlaybackEffects(soundData, playbackModifiers.AudioEffects);
                var ev = new PlayTTSEvent(
                    soundData,
                    message,
                    GetNetEntity(uid),
                    isWhisper: true,
                    volumeMultiplier: playbackModifiers.VolumeMultiplier,
                    maxDistanceOverride: playbackModifiers.MaxDistanceOverride,
                    referenceDistanceOverride: playbackModifiers.ReferenceDistanceOverride,
                    rolloffFactorOverride: playbackModifiers.RolloffFactorOverride);
                RaiseNetworkEvent(ev, Filter.Empty().AddPlayers(understood));
            }
        }

        if (confused.Count > 0)
        {
            var soundData = await GenerateTTS(_language.ObfuscateMessage(message, language), speaker, true);
            if (soundData is not null)
            {
                soundData = await _ttsAudio.ApplyPlaybackEffects(soundData, playbackModifiers.AudioEffects);
                var ev = new PlayTTSEvent(
                    soundData,
                    message,
                    GetNetEntity(uid),
                    isWhisper: true,
                    volumeMultiplier: playbackModifiers.VolumeMultiplier,
                    maxDistanceOverride: playbackModifiers.MaxDistanceOverride,
                    referenceDistanceOverride: playbackModifiers.ReferenceDistanceOverride,
                    rolloffFactorOverride: playbackModifiers.RolloffFactorOverride);
                RaiseNetworkEvent(ev, Filter.Empty().AddPlayers(confused));
            }
        }
    }

    public async void PlayGlobalTTS(
        string text,
        string voiceId,
        Filter filter,
        TTSAudioEffect audioEffects = TTSAudioEffect.None,
        bool isAnnounce = false,
        bool isRadio = false)
    {
        if (text.Contains('\u200B')) return;

        if (!GetVoicePrototype(voiceId, out var protoVoice))
            return;

        var soundData = await GenerateTTS(text, protoVoice.Speaker);
        if (soundData == null) return;

        if (isRadio)
            audioEffects |= TTSAudioEffect.StandardRadio;

        if (isAnnounce)
            audioEffects |= TTSAudioEffect.Announce;

        soundData = await _ttsAudio.ApplyPlaybackEffects(soundData, audioEffects);

        var ev = new PlayTTSEvent(
            soundData,
            text,
            isRadio: isRadio,
            isAnnounce: isAnnounce);
        RaiseNetworkEvent(ev, filter);
    }

    private void SplitListenersByComprehension(
        EntityUid source,
        ProtoId<LanguagePrototype> language,
        float range,
        out List<ICommonSession> understood,
        out List<ICommonSession> confused)
    {
        understood = new List<ICommonSession>();
        confused = new List<ICommonSession>();

        var xformQuery = GetEntityQuery<TransformComponent>();
        var sourceCoords = xformQuery.GetComponent(source).Coordinates;

        foreach (var player in Filter.Pvs(source).Recipients)
        {
            if (player.AttachedEntity is not { } listener)
                continue;

            if (!xformQuery.GetComponent(listener).Coordinates.InRange(EntityManager, sourceCoords, range))
                continue;

            if (_language.GetComprehension(listener, language) >= 1f)
                understood.Add(player);
            else
                confused.Add(player);
        }
    }

    public async Task<byte[]?> GenerateTTS(string text, string speaker, bool isWhisper = false)
    {
        if (!_isEnabled)
            return null;

        var textSanitized = Sanitize(text);
        if (textSanitized == "")
            return null;
        if (char.IsLetter(textSanitized[^1]))
            textSanitized += ".";

        var ssmlTraits = SoundTraits.RateFast;
        if (isWhisper)
            ssmlTraits = SoundTraits.PitchVerylow;
        var textSsml = ToSsmlText(textSanitized, ssmlTraits);

        return await _ttsManager.ConvertTextToSpeech(speaker, textSsml);
    }
}

public readonly record struct TTSPlaybackModifiers(
    float VolumeMultiplier,
    float? MaxDistanceOverride,
    float? ReferenceDistanceOverride,
    float? RolloffFactorOverride,
    TTSAudioEffect AudioEffects);
