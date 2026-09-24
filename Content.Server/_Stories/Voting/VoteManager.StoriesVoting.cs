// ReSharper disable CheckNamespace

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Content.Server.Maps;
using Content.Shared.Maps;
using Robust.Shared.Random;

namespace Content.Server.Voting.Managers;

public sealed partial class VoteManager
{
    private readonly Dictionary<string, int> _mapCarryoverVotes = new();
    private readonly Dictionary<string, int> _presetCarryoverVotes = new();

    private GameMapPrototype FinishMapVoteWithCarryover(List<GameMapPrototype> maps, List<int> votes)
    {
        var adjustedVotes = maps
            .Zip(votes, (map, newVotes) => (
                map,
                newVotes,
                totalVotes: newVotes + _mapCarryoverVotes.GetValueOrDefault(map.ID)
            ))
            .OrderByDescending(v => v.totalVotes)
            .ToList();

        var maxVotes = adjustedVotes.Count > 0 ? adjustedVotes.Max(v => v.totalVotes) : 0;
        var winningMaps = adjustedVotes
            .Where(v => v.totalVotes == maxVotes)
            .Select(v => v.map)
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLine(Loc.GetString("stories-vote-map-header"));
        foreach (var result in adjustedVotes)
        {
            sb.AppendLine(Loc.GetString(result.newVotes > 0
                ? "stories-vote-map-votes-new"
                : "stories-vote-map-votes",
                ("map", result.map.MapName),
                ("votes", result.totalVotes),
                ("newVotes", result.newVotes)));
        }
        sb.AppendLine();

        GameMapPrototype picked;
        if (winningMaps.Count > 1)
        {
            sb.AppendLine(Loc.GetString("stories-vote-map-tiebreaker"));
            foreach (var map in winningMaps)
            {
                sb.AppendLine($"    {map.MapName}");
            }
            picked = _random.Pick(winningMaps);
        }
        else
        {
            picked = winningMaps.First();
        }
        sb.AppendLine(Loc.GetString("stories-vote-map-win", ("winner", picked.MapName)));

        _chatManager.DispatchServerAnnouncement(sb.ToString());

        foreach (var (map, voteCount) in maps.Zip(votes))
        {
            _mapCarryoverVotes[map.ID] = _mapCarryoverVotes.GetValueOrDefault(map.ID) + voteCount;
        }

        _mapCarryoverVotes[picked.ID] = 0;

        return picked;
    }

    private string FinishPresetVoteWithCarryover(
        Dictionary<string, string> presetDict,
        List<string> presetList,
        List<int> votes)
    {
        var adjustedVotes = presetList
            .Zip(votes, (presetId, newVotes) => (
                presetId,
                presetName: Loc.GetString(presetDict[presetId]),
                newVotes,
                totalVotes: newVotes + _presetCarryoverVotes.GetValueOrDefault(presetId)
            ))
            .OrderByDescending(v => v.totalVotes)
            .ToList();

        var maxVotes = adjustedVotes.Count > 0 ? adjustedVotes.Max(v => v.totalVotes) : 0;
        var winningPresets = adjustedVotes
            .Where(v => v.totalVotes == maxVotes)
            .Select(v => (v.presetId, v.presetName))
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLine(Loc.GetString("stories-vote-preset-header"));
        foreach (var result in adjustedVotes)
        {
            sb.AppendLine(Loc.GetString(result.newVotes > 0
                ? "stories-vote-preset-votes-new"
                : "stories-vote-preset-votes",
                ("preset", result.presetName),
                ("votes", result.totalVotes),
                ("newVotes", result.newVotes)));
        }
        sb.AppendLine();

        string pickedId;
        string pickedName;
        if (winningPresets.Count > 1)
        {
            sb.AppendLine(Loc.GetString("stories-vote-preset-tiebreaker"));
            foreach (var (_, name) in winningPresets)
            {
                sb.AppendLine($"    {name}");
            }
            var picked = _random.Pick(winningPresets);
            pickedId = picked.presetId;
            pickedName = picked.presetName;
        }
        else
        {
            pickedId = winningPresets[0].presetId;
            pickedName = winningPresets[0].presetName;
        }
        sb.AppendLine(Loc.GetString("stories-vote-preset-win", ("winner", pickedName)));

        _chatManager.DispatchServerAnnouncement(sb.ToString());

        foreach (var (presetId, voteCount) in presetList.Zip(votes))
        {
            _presetCarryoverVotes[presetId] = _presetCarryoverVotes.GetValueOrDefault(presetId) + voteCount;
        }

        _presetCarryoverVotes[pickedId] = 0;

        return pickedId;
    }
}
