using Robust.Shared.Prototypes;

namespace Content.Shared._Stories.Partners;

[Prototype("specialRole")]
public sealed partial class SpecialRolePrototype : IPrototype
{
    [DataField("gameRule")]
    public EntProtoId GameRule;

    [DataField("gameRulesBlacklist")]
    public HashSet<string> GameRulesBlacklist = [];

    [DataField("state")]
    public PlayerState State = PlayerState.CrewMember;

    [ViewVariables] [IdDataField] public string ID { get; private set; } = default!;
}

public enum PlayerState
{
    Ghost,
    CrewMember,
    None,
}

public enum StatusLabel
{
    Error,
    NotInGame,
    NotPartner,
    NoTokens,
    PartnerNotAllowedProto,
    WrongPlayerState,
    AlreadyAntag,
    CantBeAntag,
    EventsDisabled,
    NotInAvailableEvents,
    GameRulesBlacklist,
    Greenshift,
}
