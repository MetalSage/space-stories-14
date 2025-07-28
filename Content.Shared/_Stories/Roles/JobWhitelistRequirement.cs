using System.Diagnostics.CodeAnalysis;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._Stories.Roles;

[Serializable, NetSerializable]
public sealed partial class JobWhitelistRequirement : JobRequirement
{
    [DataField(required: true)]
    public string Job { get; private set; } = default!;

    public override bool Check(IEntityManager entManager,
        IPrototypeManager protoManager,
        HumanoidCharacterProfile? profile,
        IReadOnlyDictionary<string, TimeSpan> playTimes,
        [NotNullWhen(false)] out FormattedMessage? reason)
    {
        reason = null;
        return true;
    }
}
