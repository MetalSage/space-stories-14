using Content.Server._Stories.Economy.Components;
using Content.Server.Stack;
using Content.Server.Station.Systems;
using Content.Shared._Stories.Economy;
using Content.Shared.Interaction;
using Content.Shared.Stacks;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;

namespace Content.Server._Stories.Economy;

public sealed class AtmSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly BankSystem _bank = default!;
    [Dependency] private readonly StackSystem _stack = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AtmComponent, AtmLoginMessage>(OnLogin);
        SubscribeLocalEvent<AtmComponent, AtmWithdrawMessage>(OnWithdraw);
        SubscribeLocalEvent<AtmComponent, AtmLogoutMessage>(OnLogout);
        SubscribeLocalEvent<AtmComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<AtmComponent, ActivatableUIOpenAttemptEvent>(OnOpenAttempt);
    }

    private void OnOpenAttempt(EntityUid uid, AtmComponent component, ActivatableUIOpenAttemptEvent args)
    {
        UpdateUi(uid, component);
    }

    private void OnLogin(EntityUid uid, AtmComponent component, AtmLoginMessage args)
    {
        var station = _station.GetOwningStation(uid);
        if (station == null)
            return;

        if (_bank.TryGetAccount(station.Value, args.AccountNumber, out var account))
        {
            if (account!.Pin == args.Pin)
            {
                component.LoggedInAccountNumber = args.AccountNumber;
                UpdateUi(uid, component, Loc.GetString("atm-msg-login-success"));
            }
            else
            {
                UpdateUi(uid, component, Loc.GetString("atm-msg-invalid-pin"));
                _audio.PlayPvs(component.SoundError, uid);
            }
        }
        else
        {
            UpdateUi(uid, component, Loc.GetString("atm-msg-account-not-found"));
            _audio.PlayPvs(component.SoundError, uid);
        }
    }

    private void OnWithdraw(EntityUid uid, AtmComponent component, AtmWithdrawMessage args)
    {
        if (component.LoggedInAccountNumber == null || args.Amount <= 0)
            return;

        var station = _station.GetOwningStation(uid);
        if (station == null)
            return;

        if (_bank.TryChangeBalance(station.Value, component.LoggedInAccountNumber, -args.Amount))
        {
            SpawnCash(uid, args.Amount);
            _audio.PlayPvs(component.SoundCash, uid);
            UpdateUi(uid, component, Loc.GetString("atm-msg-withdraw-success", ("amount", args.Amount)));
        }
        else
        {
            UpdateUi(uid, component, Loc.GetString("atm-msg-insufficient-funds"));
            _audio.PlayPvs(component.SoundError, uid);
        }
    }

    private void OnLogout(EntityUid uid, AtmComponent component, AtmLogoutMessage args)
    {
        component.LoggedInAccountNumber = null;
        UpdateUi(uid, component, Loc.GetString("atm-msg-logged-out"));
    }

    private void OnInteractUsing(EntityUid uid, AtmComponent component, InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (component.LoggedInAccountNumber == null)
            return;

        if (TryComp<StackComponent>(args.Used, out var stack) &&
            Prototype(args.Used)?.ID == "SpaceCash")
        {
            var station = _station.GetOwningStation(uid);
            if (station == null)
                return;

            var amount = _stack.GetCount(args.Used);
            if (_bank.TryChangeBalance(station.Value, component.LoggedInAccountNumber, amount))
            {
                _audio.PlayPvs(component.SoundCash, uid);

                _stack.SetCount(args.Used, 0);
                if (!Terminating(args.Used))
                    Del(args.Used);

                UpdateUi(uid, component, Loc.GetString("atm-msg-deposit-success", ("amount", amount)));
                args.Handled = true;
            }
        }
    }

    private void SpawnCash(EntityUid atmUid, int amount)
    {
        var coords = Transform(atmUid).Coordinates;
        var cash = Spawn("SpaceCash", coords);
        _stack.SetCount(cash, amount);
    }

    private void UpdateUi(EntityUid uid, AtmComponent component, string message = "")
    {
        var balance = 0;
        var isLoggedIn = component.LoggedInAccountNumber != null;
        var accNum = component.LoggedInAccountNumber ?? "";
        var ownerName = string.Empty;

        if (isLoggedIn)
        {
            var station = _station.GetOwningStation(uid);
            if (station != null &&
                _bank.TryGetAccount(station.Value, component.LoggedInAccountNumber!, out var account))
            {
                balance = account!.Balance;
                ownerName = account.OwnerName;
            }
        }

        var state = new AtmBoundUserInterfaceState(isLoggedIn, accNum, balance, message, ownerName);
        _ui.SetUiState(uid, AtmUiKey.Key, state);
    }
}
