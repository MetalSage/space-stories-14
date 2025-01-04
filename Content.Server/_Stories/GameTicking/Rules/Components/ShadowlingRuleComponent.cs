using Robust.Shared.Audio;

namespace Content.Server._Stories.GameTicking.Rules.Components;

[RegisterComponent, Access(typeof(ShadowlingRuleSystem))]
public sealed partial class ShadowlingRuleComponent : Component
{
    [DataField]
    public ShadowlingWinType WinType = ShadowlingWinType.Lost;

    [DataField]
    public LocId AscendanceAnnouncement = "shadowling-ascendance-announcement";

    [DataField]
    public Color AscendanceAnnouncementColor = Color.Red;

    [DataField]
    public SoundSpecifier? AscendanceGlobalSound = new SoundPathSpecifier("/Audio/_Stories/Misc/purple_code_remix.ogg");

    [DataField]
    public TimeSpan RoundEndTime = TimeSpan.FromMinutes(4);
}

public enum ShadowlingWinType : byte
{
    /// <summary>
    ///     Тенеморфы превознились.
    /// </summary>
    Won,
    /// <summary>
    ///     Тенеморфы выжили.
    /// </summary>
    Stalemate,
    /// <summary>
    ///     Все тенеморфы мертвы.
    /// </summary>
    Lost
}
