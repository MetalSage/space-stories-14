using System.Diagnostics.CodeAnalysis;
using Content.Shared.Preferences;
using Content.Shared.Preferences.Loadouts;
using Content.Shared.Preferences.Loadouts.Effects;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Shared._Stories.Sponsors.Loadouts.Effects;

public sealed partial class SponsorLoadoutEffect : LoadoutEffect
{
    [DataField("id", required: true)]
    public string PrototypeId = string.Empty;

    public override bool Validate(
        HumanoidCharacterProfile profile,
        RoleLoadout loadout,
        ICommonSession? session,
        IDependencyCollection collection,
        [NotNullWhen(false)] out FormattedMessage? reason)
    {
        reason = null;

        if (session == null)
            return true;

        var entManager = collection.Resolve<IEntityManager>();

        if (!entManager.TrySystem(out ISharedSponsorsManager? sponsorsManager))
        {
            reason = FormattedMessage.FromUnformatted(Loc.GetString("loadout-sponsor-api-error"));
            return false;
        }

        if (!sponsorsManager!.TryGetInfo(session.UserId, out var info))
        {
            reason = FormattedMessage.FromUnformatted(Loc.GetString("loadout-sponsor-only"));
            return false;
        }

        return CheckAllowed(info, out reason);
    }

    private bool CheckAllowed(SponsorInfo info, [NotNullWhen(false)] out FormattedMessage? reason)
    {
        reason = null;

        if (info.AllowedLoadouts != null && info.AllowedLoadouts.Contains(PrototypeId))
            return true;

        reason = FormattedMessage.FromUnformatted(Loc.GetString("loadout-sponsor-only-item"));
        return false;
    }
}
