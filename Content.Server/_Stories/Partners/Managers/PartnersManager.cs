using Npgsql;
using Content.Shared._Corvax.CCCVars;
using Robust.Shared.Configuration;
using Robust.Shared.Network;

namespace Content.Server.Database;

// FIXME: Удалить это вообще. (Нужно перенести в API)

public interface IPartnersManager
{
    void Init();
    void SetAntagPicked(NetUserId userId);
}

public sealed class PartnersManager : IPartnersManager
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    private NpgsqlConnection _db = default!;
    private ISawmill _sawmill = default!;

    public void SetAntagPicked(NetUserId userId)
    {
        if (_db.FullState == System.Data.ConnectionState.Closed || _db.FullState == System.Data.ConnectionState.Broken)
            return;

        using NpgsqlCommand cmd = new NpgsqlCommand($"""UPDATE partners SET "last_day_taking_antag" = {DateTime.Now.DayOfYear} WHERE partners.net_id = '{userId.UserId.ToString()}'""", _db);
        using NpgsqlDataReader reader = cmd.ExecuteReader();
    }

    public void Init()
    {
        _sawmill = Logger.GetSawmill("partners");

        var host = _cfg.GetCVar(CCCVars.SponsorsDatabasePgHost);
        var port = _cfg.GetCVar(CCCVars.SponsorsDatabasePgPort);
        var db = _cfg.GetCVar(CCCVars.SponsorsDatabasePgDatabase);
        var user = _cfg.GetCVar(CCCVars.SponsorsDatabasePgUsername);
        var pass = _cfg.GetCVar(CCCVars.SponsorsDatabasePgPassword);

        var connectionString = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = port,
            Database = db,
            Username = user,
            Password = pass
        }.ConnectionString;
        try
        {
            _db = new NpgsqlConnection(connectionString);
            _db.Open();
            _sawmill.Debug($"Using Postgres \"{host}:{port}/{db}\"");
        }
        catch { }
    }
}
