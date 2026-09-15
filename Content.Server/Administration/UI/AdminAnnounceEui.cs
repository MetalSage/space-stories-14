using Content.Server.Administration.Managers;
using Content.Server.Chat;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.EUI;
using Content.Shared.Administration;
using Content.Shared.Eui;
using Content.Shared._Stories.SCCVars;
using Robust.Shared.Configuration;

namespace Content.Server.Administration.UI
{
    public sealed partial class AdminAnnounceEui : BaseEui
    {
        [Dependency] private IAdminManager _adminManager = default!;
        [Dependency] private IChatManager _chatManager = default!;
        // Stories-TTS-Start
        [Dependency] private IConfigurationManager _cfg = default!;
        // Stories-TTS-End
        private readonly ChatSystem _chatSystem;

        public AdminAnnounceEui()
        {
            IoCManager.InjectDependencies(this);
            _chatSystem = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<ChatSystem>();
        }

        public override void Opened()
        {
            StateDirty();
        }

        public override EuiStateBase GetNewState()
        {
            return new AdminAnnounceEuiState();
        }

        public override void HandleMessage(EuiMessageBase msg)
        {
            base.HandleMessage(msg);

            switch (msg)
            {
                case AdminAnnounceEuiMsg.DoAnnounce doAnnounce:
                    if (!_adminManager.HasAdminFlag(Player, AdminFlags.Admin))
                    {
                        Close();
                        break;
                    }

                    switch (doAnnounce.AnnounceType)
                    {
                        case AdminAnnounceType.Server:
                            _chatManager.DispatchServerAnnouncement(doAnnounce.Announcement);
                            break;
                        // TODO: Per-station announcement support
                        case AdminAnnounceType.Station:
                            // Stories-TTS-Start
                            var voice = doAnnounce.Voice ?? _cfg.GetCVar(SCCVars.TTSAnnounceVoice);
                            _chatSystem.DispatchGlobalAnnouncement(doAnnounce.Announcement, doAnnounce.Announcer, colorOverride: Color.Gold, ttsVoice: voice);
                            // Stories-TTS-End
                            break;
                    }

                    StateDirty();

                    if (doAnnounce.CloseAfter)
                        Close();

                    break;
            }
        }
    }
}
