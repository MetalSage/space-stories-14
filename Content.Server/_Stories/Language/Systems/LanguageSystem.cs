using System.Linq;
using Content.Server.GameTicking.Events;
using Content.Shared._Stories.Language;
using Content.Shared._Stories.Language.Components;
using Content.Shared._Stories.Language.Prototypes;
using Content.Shared._Stories.Language.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server._Stories.Language.Systems;

public sealed partial class LanguageSystem : SharedLanguageSystem
{
    [Dependency] private IComponentFactory _compFactory = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LanguageComponent, MapInitEvent>(OnInitLanguageSpeaker);
        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStarting);
        SubscribeNetworkEvent<LanguagesSetMessage>(OnClientSetLanguage);
    }

    private void OnInitLanguageSpeaker(Entity<LanguageComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.Preset is { } presetId && presetId.TryGet(out var preset, _prototypeManager, _compFactory))
        {
            foreach (var language in preset.SpokenLanguages)
                AddLanguage(ent, language, addSpoken: true, addUnderstood: preset.UnderstoodLanguages.Contains(language), source: LanguageSource.Preset);

            foreach (var language in preset.UnderstoodLanguages)
            {
                if (!preset.SpokenLanguages.Contains(language))
                    AddLanguage(ent, language, addSpoken: false, addUnderstood: true, source: LanguageSource.Preset);
            }

            ent.Comp.CurrentLanguage ??= preset.CurrentLanguage;
            ent.Comp.DefaultLanguage ??= preset.DefaultLanguage;
        }

        if (ent.Comp.CurrentLanguage == null)
            ent.Comp.CurrentLanguage = ent.Comp.DefaultLanguage ?? ent.Comp.SpokenLanguages.FirstOrDefault();

        UpdateEntityLanguages(ent.AsNullable());
    }

    private void OnRoundStarting(RoundStartingEvent ev)
    {
        ReseedObfuscationForRound();
    }

    private void OnClientSetLanguage(LanguagesSetMessage message, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { Valid: true } uid)
            return;

        if (!TryComp<LanguageComponent>(uid, out var component))
            return;

        if (!CanSpeak(uid, message.CurrentLanguage))
            return;

        SetLanguage(uid, message.CurrentLanguage);
    }

    public void SetLanguage(Entity<LanguageComponent?> ent, ProtoId<LanguagePrototype> language)
    {
        if (!CanSpeak(ent, language) || !Resolve(ent, ref ent.Comp) || ent.Comp.CurrentLanguage == language)
            return;

        ent.Comp.CurrentLanguage = language;
        var update = new LanguagesUpdateEvent();
        RaiseLocalEvent(ent, ref update, true);
        Dirty(ent);
    }

    public void AddLanguage(
        EntityUid uid,
        ProtoId<LanguagePrototype> language,
        bool addSpoken = true,
        bool addUnderstood = true,
        string source = LanguageSource.Admin)
    {
        var component = EnsureComp<LanguageComponent>(uid);

        if (addSpoken)
            AddLanguageSource(component.SpokenLanguageSources, language, source);

        if (addUnderstood)
            AddLanguageSource(component.UnderstoodLanguageSources, language, source);

        UpdateEntityLanguages((uid, component));
    }

    public void RemoveLanguage(
        Entity<LanguageComponent?> ent,
        ProtoId<LanguagePrototype> language,
        bool removeSpoken = true,
        bool removeUnderstood = true,
        string? source = null)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        if (removeSpoken)
            RemoveLanguageSource(ent.Comp.SpokenLanguageSources, language, source);

        if (removeUnderstood)
            RemoveLanguageSource(ent.Comp.UnderstoodLanguageSources, language, source);

        UpdateEntityLanguages(ent.Owner);
    }

    public void AddBlockedLanguage(
        EntityUid uid,
        ProtoId<LanguagePrototype> language,
        bool blockSpeaking = true,
        bool blockUnderstanding = true)
    {
        var component = EnsureComp<LanguageComponent>(uid);

        if (blockSpeaking)
            component.BlockedSpeaking.Add(language);

        if (blockUnderstanding)
            component.BlockedUnderstanding.Add(language);

        UpdateEntityLanguages((uid, component));
    }

    public void RemoveBlockedLanguage(
        Entity<LanguageComponent?> ent,
        ProtoId<LanguagePrototype> language,
        bool unblockSpeaking = true,
        bool unblockUnderstanding = true)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        if (unblockSpeaking)
            ent.Comp.BlockedSpeaking.Remove(language);

        if (unblockUnderstanding)
            ent.Comp.BlockedUnderstanding.Remove(language);

        UpdateEntityLanguages(ent.Owner);
    }

    private static void AddLanguageSource(
        Dictionary<ProtoId<LanguagePrototype>, HashSet<string>> sources,
        ProtoId<LanguagePrototype> language,
        string source)
    {
        if (!sources.TryGetValue(language, out var set))
        {
            set = new HashSet<string>();
            sources[language] = set;
        }

        set.Add(source);
    }

    private static void RemoveLanguageSource(
        Dictionary<ProtoId<LanguagePrototype>, HashSet<string>> sources,
        ProtoId<LanguagePrototype> language,
        string? source)
    {
        if (!sources.TryGetValue(language, out var set))
            return;

        if (source == null)
        {
            sources.Remove(language);
            return;
        }

        set.Remove(source);
        if (set.Count == 0)
            sources.Remove(language);
    }

    public bool TryFixCurrentLanguage(Entity<LanguageComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return false;

        if (ent.Comp.CurrentLanguage == null ||
            !ent.Comp.SpokenLanguages.Contains(ent.Comp.CurrentLanguage.Value))
        {
            ent.Comp.CurrentLanguage = ent.Comp.DefaultLanguage ?? ent.Comp.SpokenLanguages.FirstOrDefault();
            var update = new LanguagesUpdateEvent();
            RaiseLocalEvent(ent, ref update);
            Dirty(ent);
            return true;
        }

        return false;
    }

    public void UpdateEntityLanguages(Entity<LanguageComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        var ev = new DetermineEntityLanguagesEvent();

        foreach (var spoken in ent.Comp.SpokenLanguageSources.Keys)
            ev.SpokenLanguages.Add(spoken);

        foreach (var understood in ent.Comp.UnderstoodLanguageSources.Keys)
            ev.UnderstoodLanguages.Add(understood);

        RaiseLocalEvent(ent, ref ev);

        ev.SpokenLanguages.ExceptWith(ent.Comp.BlockedSpeaking);
        ev.UnderstoodLanguages.ExceptWith(ent.Comp.BlockedUnderstanding);

        ent.Comp.SpokenLanguages.Clear();
        ent.Comp.UnderstoodLanguages.Clear();

        ent.Comp.SpokenLanguages.UnionWith(ev.SpokenLanguages);
        ent.Comp.UnderstoodLanguages.UnionWith(ev.UnderstoodLanguages);

        if (!TryFixCurrentLanguage(ent))
        {
            var update = new LanguagesUpdateEvent();
            RaiseLocalEvent(ent, ref update);
        }

        Dirty(ent);
    }

    public string ObfuscateMessageForSpeaker(EntityUid speaker, string message, ProtoId<LanguagePrototype> language)
    {
        return ObfuscateMessage(message, language, GetComprehension(speaker, language));
    }

    public string ObfuscateMessageForListener(EntityUid listener, string speakerMessage, ProtoId<LanguagePrototype> language)
    {
        return ObfuscateMessage(speakerMessage, language, GetComprehension(listener, language));
    }
}
