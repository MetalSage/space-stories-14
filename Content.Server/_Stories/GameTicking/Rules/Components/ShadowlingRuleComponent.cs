using Robust.Shared.Audio;

namespace Content.Server._Stories.GameTicking.Rules.Components;

[RegisterComponent]
[Access(typeof(ShadowlingRuleSystem))]
public sealed partial class ShadowlingRuleComponent : Component
{
    [DataField]
    public LocId AscendanceAnnouncement = "stories-shadowling-ascendance-announcement";

    [DataField]
    public Color AscendanceAnnouncementColor = Color.Red;

    [DataField]
    public SoundSpecifier? AscendanceGlobalSound = new SoundPathSpecifier("/Audio/_Stories/Misc/purple_code_remix.ogg");

    [DataField]
    public TimeSpan RoundEndTime = TimeSpan.FromMinutes(4);

    [DataField]
    public ShadowlingWinType WinType = ShadowlingWinType.Lost;

    [DataField]
    public bool HalfwayWarningSent = false;
}

public enum ShadowlingWinType : byte
{
    Won,
    Stalemate,
    Lost,
}
