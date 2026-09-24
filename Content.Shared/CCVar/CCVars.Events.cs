using Content.Shared.Administration;
using Content.Shared.CCVar.CVarAccess;
using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    ///     Controls if the game should run station events
    /// </summary>
    [CVarControl(AdminFlags.Server | AdminFlags.Mapping)]
    public static readonly CVarDef<bool>
        EventsEnabled = CVarDef.Create("events.enabled", true, CVar.ARCHIVE | CVar.SERVERONLY);

    // Stories-Antag-Start
    /// <summary>
    ///     Maximum number of active major antagonists from station events at the same time.
    /// </summary>
    [CVarControl(AdminFlags.Server | AdminFlags.Round)]
    public static readonly CVarDef<int>
        EventsMajorAntagsMax = CVarDef.Create("events.major_antags_max", 1, CVar.ARCHIVE | CVar.SERVERONLY);

    /// <summary>
    ///     Cooldown in minutes between major antagonist station events.
    /// </summary>
    [CVarControl(AdminFlags.Server | AdminFlags.Round)]
    public static readonly CVarDef<float>
        EventsMajorAntagCooldown = CVarDef.Create("events.major_antag_cooldown", 15.0f, CVar.ARCHIVE | CVar.SERVERONLY);
    // Stories-Antag-End
}
