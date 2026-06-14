using Content.Shared._Stories.Conversion;
using Content.Shared.Damage;
using Content.Shared.Polymorph;
using Content.Shared.StatusIcon;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Stories.Shadowling;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class ShadowlingComponent : Component
{
    [DataField, AutoNetworkedField]
    public Dictionary<EntProtoId, int> ActionRequirements = new()
    {
        { "STActionShadowlingEnthrall", 0 },
        { "STActionShadowlingHatch", 2 }
    };

    [DataField, AutoNetworkedField]
    public Dictionary<EntProtoId, EntityUid> GrantedActions = new();

    [DataField, AutoNetworkedField]
    public ProtoId<FactionIconPrototype> StatusIcon = "STShadowlingFaction";

    [DataField, AutoNetworkedField]
    public int? MaxThrallsBeforeHatch = 2;

    [DataField, AutoNetworkedField]
    public int AscendanceThrallRequirement = 15;

    [DataField]
    public ProtoId<PolymorphPrototype> HatchPolymorph = "STShadowling";

    [DataField]
    public ProtoId<PolymorphPrototype> AscendancePolymorph = "STAscended";

    [DataField]
    public EntProtoId SmokePrototype = "Smoke";

    [DataField]
    public string ShadowlingSmokeReagent = "STShadowlingSmokeReagent";

    [DataField]
    public ProtoId<ConversionPrototype> ShadowlingThrallConversion = "STShadowlingThrall";

    [DataField]
    public bool RequireHumanoid = true;

    [DataField]
    public bool RequireConnectedMind = true;

    [DataField]
    public float SonicScreechRange = 7f;

    [DataField]
    public float VeilRange = 5f;

    [DataField]
    public float FlashFreezeRange = 5f;

    [DataField]
    public float GlacialBlastRange = 5f;

    [DataField]
    public float DrainLifeRange = 3f;

    [DataField]
    public float SmokeRadius = 5f;

    [DataField]
    public TimeSpan EnthrallDuration = TimeSpan.FromSeconds(10f);

    [DataField]
    public TimeSpan HatchDuration = TimeSpan.FromSeconds(15f);

    [DataField]
    public TimeSpan AscendanceDuration = TimeSpan.FromSeconds(5f);

    [DataField]
    public TimeSpan ShadowWalkDuration = TimeSpan.FromSeconds(6f);

    [DataField]
    public TimeSpan GlareFlashDuration = TimeSpan.FromSeconds(10f);

    [DataField]
    public TimeSpan GlareStunDuration = TimeSpan.FromSeconds(5f);

    [DataField]
    public TimeSpan FlashFreezeStunDuration = TimeSpan.FromSeconds(2f);

    [DataField]
    public TimeSpan GlacialBlastStunDuration = TimeSpan.FromSeconds(5f);

    [DataField]
    public DamageSpecifier FlashFreezeDamage = new()
    {
        DamageDict = new() { { "Cold", 15 } }
    };

    [DataField]
    public DamageSpecifier GlacialBlastDamage = new()
    {
        DamageDict = new() { { "Cold", 80 } }
    };

    [DataField]
    public DamageSpecifier DrainLifeDamage = new()
    {
        DamageDict = new() { { "Asphyxiation", 25 } }
    };

    [DataField]
    public DamageSpecifier DrainLifeHeal = new()
    {
        DamageDict = new()
        {
            { "Blunt", -3.33f },
            { "Slash", -3.33f },
            { "Piercing", -3.34f },
            { "Heat", -2.5f },
            { "Shock", -2.5f },
            { "Cold", -2.5f },
            { "Caustic", -2.5f },
            { "Asphyxiation", -5f },
            { "Bloodloss", -5f },
            { "Poison", -5f },
            { "Radiation", -5f },
        }
    };

    [DataField]
    public DamageSpecifier AscendanceKillDamage = new()
    {
        DamageDict = new() { { "Bloodloss", 1000 } }
    };

    [DataField]
    public DamageSpecifier SonicScreechWindowDamage = new()
    {
        DamageDict = new() { { "Structural", 80 } }
    };

    [DataField]
    public SoundSpecifier? GlareSound = new SoundPathSpecifier("/Audio/Magic/forcewall.ogg");

    [DataField]
    public SoundSpecifier? FreezeSound = new SoundPathSpecifier("/Audio/_Stories/Effects/ghost2.ogg");

    [DataField]
    public SoundSpecifier? ScreechSound = new SoundPathSpecifier("/Audio/_Stories/Effects/screech.ogg");

    [DataField]
    public SoundSpecifier? SmokeSound = new SoundPathSpecifier("/Audio/_Stories/Effects/bamf.ogg");

    [DataField]
    public SoundSpecifier? HatchSound = new SoundPathSpecifier("/Audio/_Stories/Effects/splat.ogg");

    [DataField]
    public SoundSpecifier? AscendanceSound = new SoundPathSpecifier("/Audio/_Stories/Misc/veryfar_noise.ogg");

    [DataField]
    public SoundSpecifier? AnnihilateSound = new SoundPathSpecifier("/Audio/_Stories/Effects/splat.ogg");
}

[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowWalkingComponent : Component
{
    [DataField]
    public TimeSpan? EndTime;

    [DataField]
    public int OriginalCollisionLayer;

    [DataField]
    public int OriginalCollisionMask;

    [DataField]
    public float OriginalWalkSpeed;

    [DataField]
    public float OriginalSprintSpeed;

    [DataField]
    public int OriginalDrawDepth;

    [DataField]
    public int OriginalVisibility;

    [DataField]
    public int OriginalEyeVisibilityMask;

    [DataField]
    public bool OriginalDrawFov;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class AscendantBroadcastComponent : Component
{
    [DataField]
    public TimeSpan NextBroadcast = TimeSpan.Zero;
}
