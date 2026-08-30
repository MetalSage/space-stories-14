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
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LanguageComponent, MapInitEvent>(OnInitLanguageSpeaker);
        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStarting);
        SubscribeNetworkEvent<LanguagesSetMessage>(OnClientSetLanguage);
    }

    private void OnInitLanguageSpeaker(Entity<LanguageComponent> ent, ref MapInitEvent args)
    {
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

        SetLanguage(uid, message.CurrentLanguage);
    }

    public void SetLanguage(Entity<LanguageComponent?> ent, ProtoId<LanguagePrototype> language)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        if (!CanSpeak(ent, language) || ent.Comp.CurrentLanguage == language)
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

    public void AddPartialUnderstanding(
        EntityUid uid,
        ProtoId<LanguagePrototype> language,
        float amount,
        string source = LanguageSource.Admin)
    {
        var component = EnsureComp<LanguageComponent>(uid);

        if (!component.PartialUnderstanding.TryGetValue(language, out var sources))
        {
            sources = new Dictionary<string, float>();
            component.PartialUnderstanding[language] = sources;
        }

        sources[source] = Math.Clamp(amount, 0f, 1f);
    }

    public void RemovePartialUnderstanding(
        Entity<LanguageComponent?> ent,
        ProtoId<LanguagePrototype> language,
        string? source = null)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        if (!ent.Comp.PartialUnderstanding.TryGetValue(language, out var sources))
            return;

        if (source == null)
            ent.Comp.PartialUnderstanding.Remove(language);
        else
        {
            sources.Remove(source);
            if (sources.Count == 0)
                ent.Comp.PartialUnderstanding.Remove(language);
        }
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

    private ProtoId<LanguagePrototype>? SelectPreferredLanguage(IReadOnlySet<ProtoId<LanguagePrototype>> languages)
    {
        ProtoId<LanguagePrototype>? best = null;
        var bestPriority = int.MinValue;

        foreach (var candidate in languages)
        {
            if (!_prototypeManager.TryIndex(candidate, out var proto))
                continue;

            if (proto.Priority < bestPriority)
                continue;

            if (proto.Priority == bestPriority &&
                (best == null || string.CompareOrdinal(candidate.Id, best.Value.Id) >= 0))
            {
                continue;
            }

            best = candidate;
            bestPriority = proto.Priority;
        }

        return best;
    }

    public void ClearBlockedLanguages(Entity<LanguageComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        if (ent.Comp.BlockedSpeaking.Count == 0 && ent.Comp.BlockedUnderstanding.Count == 0)
            return;

        ent.Comp.BlockedSpeaking.Clear();
        ent.Comp.BlockedUnderstanding.Clear();

        UpdateEntityLanguages(ent);
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

        if (ent.Comp.CurrentLanguage is { } current && ent.Comp.SpokenLanguages.Contains(current))
            return false;

        ProtoId<LanguagePrototype>? replacement = null;

        if (ent.Comp.DefaultLanguage is { } fallback && ent.Comp.SpokenLanguages.Contains(fallback))
            replacement = fallback;
        else
            replacement = SelectPreferredLanguage(ent.Comp.SpokenLanguages);

        if (ent.Comp.CurrentLanguage == replacement)
            return false;

        ent.Comp.CurrentLanguage = replacement;
        var update = new LanguagesUpdateEvent();
        RaiseLocalEvent(ent, ref update);
        Dirty(ent);
        return true;
    }

    public void SetRelayLanguage(EntityUid uid, ProtoId<LanguagePrototype> language)
    {
        var component = EnsureComp<LanguageComponent>(uid);

        if (component.SpokenLanguageSources.Count == 1 &&
            component.SpokenLanguageSources.ContainsKey(language) &&
            component.CurrentLanguage == language)
        {
            return;
        }

        component.SpokenLanguageSources.Clear();
        component.UnderstoodLanguageSources.Clear();
        AddLanguageSource(component.SpokenLanguageSources, language, LanguageSource.Relay);
        AddLanguageSource(component.UnderstoodLanguageSources, language, LanguageSource.Relay);
        component.DefaultLanguage = language;

        UpdateEntityLanguages((uid, component));
    }

    public void TransferMindBoundLanguages(EntityUid from, EntityUid to)
    {
        if (!TryComp<LanguageComponent>(from, out var source))
            return;

        var target = EnsureComp<LanguageComponent>(to);
        var moved = false;

        moved |= MoveMindBoundSources(source.SpokenLanguageSources, target.SpokenLanguageSources);
        moved |= MoveMindBoundSources(source.UnderstoodLanguageSources, target.UnderstoodLanguageSources);

        foreach (var (language, owners) in source.PartialUnderstanding)
        {
            foreach (var (owner, amount) in owners)
            {
                if (!LanguageSource.MindBound.Contains(owner))
                    continue;

                if (!target.PartialUnderstanding.TryGetValue(language, out var targetOwners))
                {
                    targetOwners = new Dictionary<string, float>();
                    target.PartialUnderstanding[language] = targetOwners;
                }

                targetOwners[owner] = amount;
                moved = true;
            }
        }

        foreach (var language in source.PartialUnderstanding.Keys.ToArray())
        {
            RemovePartialUnderstandingSources(source.PartialUnderstanding, language);
        }

        if (!moved)
            return;

        UpdateEntityLanguages((from, source));
        UpdateEntityLanguages((to, target));
    }

    private static bool MoveMindBoundSources(
        Dictionary<ProtoId<LanguagePrototype>, HashSet<string>> from,
        Dictionary<ProtoId<LanguagePrototype>, HashSet<string>> to)
    {
        var moved = false;

        foreach (var language in from.Keys.ToArray())
        {
            var owners = from[language];

            foreach (var owner in owners.ToArray())
            {
                if (!LanguageSource.MindBound.Contains(owner))
                    continue;

                AddLanguageSource(to, language, owner);
                owners.Remove(owner);
                moved = true;
            }

            if (owners.Count == 0)
                from.Remove(language);
        }

        return moved;
    }

    private static void RemovePartialUnderstandingSources(
        Dictionary<ProtoId<LanguagePrototype>, Dictionary<string, float>> partial,
        ProtoId<LanguagePrototype> language)
    {
        if (!partial.TryGetValue(language, out var owners))
            return;

        foreach (var owner in owners.Keys.ToArray())
        {
            if (LanguageSource.MindBound.Contains(owner))
                owners.Remove(owner);
        }

        if (owners.Count == 0)
            partial.Remove(language);
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
}
