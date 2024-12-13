using Robust.Shared.Prototypes;

namespace Content.Shared._Stories.Partners;

[Prototype("specialRole")]
public sealed partial class SpecialRolePrototype : IPrototype
{
    [ViewVariables][IdDataField] public string ID { get; private set; } = default!;

    [DataField("state")]
    public PlayerState State = PlayerState.CrewMember;

    [DataField("gameRulesBlacklist")]
    public HashSet<string> GameRulesBlacklist = [];

    [DataField("gameRule")]
    public EntProtoId GameRule = default!;
}

public enum PlayerState
{
    Ghost,
    CrewMember,
    None
}

public enum StatusLabel
{
    Error,
    NotInGame,
    NotPartner,
    PartnerNotAllowedProto,
    IssuedLimit,
    WrongPlayerState,
    AlreadyAntag,
    CantBeAntag,
    EventsDisabled,
    NotInAvailableEvents,
    GameRulesBlacklist,
    Greenshift,
}
