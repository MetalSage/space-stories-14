using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.Standing
{
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
    [Access(typeof(StandingStateSystem))]
    public sealed partial class StandingStateComponent : Component
    {
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField]
        public SoundSpecifier? DownSound { get; private set; } = new SoundCollectionSpecifier("BodyFall");

        [DataField, AutoNetworkedField]
        public bool Standing { get; set; } = true;

        /// <summary>
        /// Friction modifier applied to an entity in the downed state.
        /// </summary>
        [DataField, AutoNetworkedField]
        public float DownFrictionMod = 0.4f;

        /// <summary>
        ///     List of fixtures that had their collision mask changed when the entity was downed.
        ///     Required for re-adding the collision mask.
        /// </summary>
        [DataField, AutoNetworkedField]
        public List<string> ChangedFixtures = new();

        // Stories-Crawling-Start

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField]
        public bool CanCrawl { get; set; } = false;

        /// <summary>
        /// Is entity able to stand up in it's own
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField]
        public bool CanStandUp { get; set; } = true;

        /// <summary>
        /// Delay used in do after to lie down
        /// </summary>
        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public TimeSpan DownDelay = TimeSpan.FromMilliseconds(500);

        // <summary>
        /// Delay used in do after to get up
        /// </summary>
        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public TimeSpan StandDelay = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Speed modificator which applies while entity crawling
        /// </summary>
        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public float CrawlingSpeedModifier = 0.3f;

        // Stories-Crawling-End
    }
}
