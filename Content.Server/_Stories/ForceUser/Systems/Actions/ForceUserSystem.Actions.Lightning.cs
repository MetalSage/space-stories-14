using Content.Shared._Stories.ForceUser.Actions.Events;
using Robust.Shared.Map;
using System.Numerics;

namespace Content.Server._Stories.ForceUser;
public sealed partial class ForceUserSystem
{
    public void InitializeLightning()
    {
        SubscribeLocalEvent<LightningStrikeEvent>(OnLightning);
    }
    private void OnLightning(LightningStrikeEvent args)
    {
        if (args.Handled)
            return;

        var xform = Transform(args.Performer);

        var coord = _xform.GetMapCoordinates(xform);

        HashSet<MapCoordinates> coords = new()
        {
            new MapCoordinates(coord.Position + new Vector2(0, 1), coord.MapId),
            new MapCoordinates(coord.Position + new Vector2(1, -1), coord.MapId),
            new MapCoordinates(coord.Position + new Vector2(-1, -1), coord.MapId)
        };

        foreach (var coordinates in coords)
        {
            var user = Spawn(null, coordinates);
            _lightning.ShootLightning(user, args.Target);
        }

        args.Handled = true;
    }
}
