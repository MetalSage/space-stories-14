using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.Serialization;

namespace Content.Shared._Stories.Cards.Fan
{
    [Serializable, NetSerializable]
    public sealed class CardSelectedMessage : BoundUserInterfaceMessage
    {
        public readonly NetEntity CardEntity;

        public readonly NetEntity User;
        public CardSelectedMessage(NetEntity cardEntity, NetEntity user)
        {
            CardEntity = cardEntity;
            User = user;
        }
    }
}
