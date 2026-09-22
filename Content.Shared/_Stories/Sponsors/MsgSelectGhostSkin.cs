using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared._Stories.Sponsors;

public sealed class MsgSelectGhostSkin : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Command;

    public string? SkinId;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        var hasSkin = buffer.ReadBoolean();
        if (hasSkin)
            SkinId = buffer.ReadString();
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(!string.IsNullOrEmpty(SkinId));
        if (!string.IsNullOrEmpty(SkinId))
            buffer.Write(SkinId);
    }
}
