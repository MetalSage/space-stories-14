﻿using System.IO;
using System.Text.Json.Serialization;
using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._Stories.Partners;

[Serializable, NetSerializable]
public sealed class SponsorInfo
{
    public static readonly TimeSpan TimeAdvantage = TimeSpan.FromMinutes(3);

    [JsonPropertyName("tier")]
    public int? Tier { get; set; }

    [JsonPropertyName("oocColor")]
    public string? OOCColor { get; set; }

    [JsonPropertyName("priorityJoin")]
    public bool HavePriorityJoin { get; set; } = false;

    [JsonPropertyName("allowedMarkings")] // TODO: Rename API field in separate PR as breaking change!
    public string[] AllowedMarkings { get; set; } = Array.Empty<string>();

    [JsonPropertyName("roleTimeBypass")]
    public bool RoleTimeBypass { get; set; } = false;

    [JsonPropertyName("ghost_skin")]
    public string GhostSkin { get; set; } = "MobObserver";

    [JsonPropertyName("allowed_antags")]
    public string[] AllowedAntags { get; set; } = Array.Empty<string>();

    [JsonPropertyName("tokens")]
    public int Tokens { get; set; } = 0;
}


/// <summary>
/// Server sends sponsoring info to client on connect only if user is sponsor
/// </summary>
public sealed class MsgSponsorInfo : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Command;

    public SponsorInfo? Info;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        var isSponsor = buffer.ReadBoolean();
        buffer.ReadPadBits();
        if (!isSponsor) return;
        var length = buffer.ReadVariableInt32();
        using var stream = new MemoryStream(length);
        buffer.ReadAlignedMemory(stream, length);
        serializer.DeserializeDirect(stream, out Info);
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(Info != null);
        buffer.WritePadBits();
        if (Info == null) return;
        var stream = new MemoryStream();
        serializer.SerializeDirect(stream, Info);
        buffer.WriteVariableInt32((int) stream.Length);
        buffer.Write(stream.AsSpan());
    }
}
