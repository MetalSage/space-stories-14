using Content.Shared._Stories.Language.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Stories.Language;

[ByRefEvent]
public record struct DetermineLanguageEvent(EntityUid Speaker, ProtoId<LanguagePrototype> Language);

[Serializable, NetSerializable]
public sealed class LanguagesSetMessage(ProtoId<LanguagePrototype> currentLanguage) : EntityEventArgs
{
    public ProtoId<LanguagePrototype> CurrentLanguage = currentLanguage;
}

[ByRefEvent]
public record struct CanUnderstandLanguageEvent(
    EntityUid Listener,
    ProtoId<LanguagePrototype> Language,
    bool CanUnderstand = false);

[ByRefEvent]
public record struct DetermineEntityLanguagesEvent(
    HashSet<ProtoId<LanguagePrototype>> SpokenLanguages,
    HashSet<ProtoId<LanguagePrototype>> UnderstoodLanguages)
{
    public DetermineEntityLanguagesEvent() : this([], [])
    {
    }
}

[ByRefEvent]
public readonly record struct LanguagesUpdateEvent;
