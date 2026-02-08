using System.Linq;
using Content.Server._Stories.Economy;
using Content.Server.Cargo.Systems;
using Content.Server.Power.Components;
using Content.Server.Station.Systems;
using Content.Server.Vocalization.Systems;
using Content.Shared._Stories.Economy.Components;
using Content.Shared.Access.Components;
using Content.Shared.Cargo;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Emp;
using Content.Shared.GameTicking;
using Content.Shared.PDA;
using Content.Shared.Power;
using Content.Shared.Throwing;
using Content.Shared.UserInterface;
using Content.Shared.VendingMachines;
using Content.Shared.Wall;
using Robust.Server.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Numerics;

namespace Content.Server.VendingMachines
{
    public sealed class VendingMachineSystem : SharedVendingMachineSystem
    {
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly PricingSystem _pricing = default!;
        [Dependency] private readonly ThrowingSystem _throwingSystem = default!;
        // Stories-Economy-Start
        [Dependency] private readonly ISharedPlayerManager _playerManager = default!;
        [Dependency] private readonly BankSystem _bank = default!;
        [Dependency] private readonly StationSystem _station = default!;
        [Dependency] private readonly Inventory.ServerInventorySystem _inventory = default!;
        // Stories-Economy-End

        private const float WallVendEjectDistanceFromWall = 1f;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<VendingMachineComponent, PowerChangedEvent>(OnPowerChanged);
            SubscribeLocalEvent<VendingMachineComponent, DamageChangedEvent>(OnDamageChanged);
            SubscribeLocalEvent<VendingMachineComponent, PriceCalculationEvent>(OnVendingPrice);
            SubscribeLocalEvent<VendingMachineComponent, TryVocalizeEvent>(OnTryVocalize);

            SubscribeLocalEvent<VendingMachineComponent, VendingMachineSelfDispenseEvent>(OnSelfDispense);

            SubscribeLocalEvent<VendingMachineRestockComponent, PriceCalculationEvent>(OnPriceCalculation);

            // Stories-Economy-Start
            SubscribeLocalEvent<VendingMachineComponent, BeforeActivatableUIOpenEvent>(
                (uid, comp, args) => UpdateVendingUI(uid, args.User, comp));
            SubscribeLocalEvent<VendingMachineComponent, BoundUIOpenedEvent>(OnBoundUIOpened);
            // Stories-Economy-End
        }

        private void OnVendingPrice(EntityUid uid, VendingMachineComponent component, ref PriceCalculationEvent args)
        {
            var price = 0.0;

            foreach (var entry in component.Inventory.Values)
            {
                if (!PrototypeManager.TryIndex<EntityPrototype>(entry.ID, out var proto))
                {
                    Log.Error($"Unable to find entity prototype {entry.ID} on {ToPrettyString(uid)} vending.");
                    continue;
                }

                price += entry.Amount * _pricing.GetEstimatedPrice(proto);
            }

            args.Price += price;
        }

        protected override void OnMapInit(EntityUid uid, VendingMachineComponent component, MapInitEvent args)
        {
            base.OnMapInit(uid, component, args);

            if (HasComp<ApcPowerReceiverComponent>(uid))
            {
                TryUpdateVisualState((uid, component));
            }

            RestockInventoryFromPrototype(uid, component, component.InitialStockQuality); // Stories-Economy
        }

        private void OnPowerChanged(EntityUid uid, VendingMachineComponent component, ref PowerChangedEvent args)
        {
            TryUpdateVisualState((uid, component));
        }

        private void OnDamageChanged(EntityUid uid, VendingMachineComponent component, DamageChangedEvent args)
        {
            if (!args.DamageIncreased && component.Broken)
            {
                component.Broken = false;
                Dirty(uid, component);
                TryUpdateVisualState((uid, component));
                return;
            }

            if (component.Broken || component.DispenseOnHitCoolingDown ||
                component.DispenseOnHitChance == null || args.DamageDelta == null)
                return;

            if (args.DamageIncreased && args.DamageDelta.GetTotal() >= component.DispenseOnHitThreshold &&
                _random.Prob(component.DispenseOnHitChance.Value))
            {
                if (component.DispenseOnHitCooldown != null)
                {
                    component.DispenseOnHitEnd = Timing.CurTime + component.DispenseOnHitCooldown.Value;
                }

                EjectRandom(uid, throwItem: true, forceEject: true, component);
            }
        }

        private void OnSelfDispense(EntityUid uid, VendingMachineComponent component, VendingMachineSelfDispenseEvent args)
        {
            if (args.Handled)
                return;

            args.Handled = true;
            EjectRandom(uid, throwItem: true, forceEject: false, component);
        }

        /// <summary>
        /// Sets the <see cref="VendingMachineComponent.CanShoot"/> property of the vending machine.
        /// </summary>
        public void SetShooting(EntityUid uid, bool canShoot, VendingMachineComponent? component = null)
        {
            if (!Resolve(uid, ref component))
                return;

            component.CanShoot = canShoot;
        }

        /// <summary>
        /// Sets the <see cref="VendingMachineComponent.Contraband"/> property of the vending machine.
        /// </summary>
        public void SetContraband(EntityUid uid, bool contraband, VendingMachineComponent? component = null)
        {
            if (!Resolve(uid, ref component))
                return;

            component.Contraband = contraband;
            Dirty(uid, component);
        }

        /// <summary>
        /// Ejects a random item from the available stock. Will do nothing if the vending machine is empty.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="throwItem">Whether to throw the item in a random direction after dispensing it.</param>
        /// <param name="forceEject">Whether to skip the regular ejection checks and immediately dispense the item without animation.</param>
        /// <param name="vendComponent"></param>
        public void EjectRandom(EntityUid uid, bool throwItem, bool forceEject = false, VendingMachineComponent? vendComponent = null)
        {
            if (!Resolve(uid, ref vendComponent))
                return;

            var availableItems = GetAvailableInventory(uid, vendComponent);
            if (availableItems.Count <= 0)
                return;

            var item = _random.Pick(availableItems);

            if (forceEject)
            {
                vendComponent.NextItemToEject = item.ID;
                vendComponent.ThrowNextItem = throwItem;
                var entry = GetEntry(uid, item.ID, item.Type, vendComponent);
                if (entry != null)
                    entry.Amount--;
                EjectItem(uid, vendComponent, forceEject);
            }
            else
            {
                TryEjectVendorItem(uid, item.Type, item.ID, throwItem, user: null, vendComponent: vendComponent);
            }
        }

        protected override void EjectItem(EntityUid uid, VendingMachineComponent? vendComponent = null, bool forceEject = false)
        {
            if (!Resolve(uid, ref vendComponent))
                return;

            // No need to update the visual state because we never changed it during a forced eject
            if (!forceEject)
                TryUpdateVisualState((uid, vendComponent));

            if (string.IsNullOrEmpty(vendComponent.NextItemToEject))
            {
                vendComponent.ThrowNextItem = false;
                return;
            }

            // Default spawn coordinates
            var xform = Transform(uid);
            var spawnCoordinates = xform.Coordinates;

            //Make sure the wallvends spawn outside of the wall.
            if (TryComp<WallMountComponent>(uid, out var wallMountComponent))
            {
                var offset = (wallMountComponent.Direction + xform.LocalRotation - Math.PI / 2).ToVec() * WallVendEjectDistanceFromWall;
                spawnCoordinates = spawnCoordinates.Offset(offset);
            }

            var ent = Spawn(vendComponent.NextItemToEject, spawnCoordinates);

            if (vendComponent.ThrowNextItem)
            {
                var range = vendComponent.NonLimitedEjectRange;
                var direction = new Vector2(_random.NextFloat(-range, range), _random.NextFloat(-range, range));
                _throwingSystem.TryThrow(ent, direction, vendComponent.NonLimitedEjectForce);
            }

            vendComponent.NextItemToEject = null;
            vendComponent.ThrowNextItem = false;
        }

        // Stories-Economy-Start
        private void OnPriceCalculation(EntityUid uid, VendingMachineRestockComponent component, ref PriceCalculationEvent args)
        {
            var priceSets = new List<double>();
            foreach (var vendingInventory in component.CanRestock)
            {
                double total = 0;
                if (PrototypeManager.TryIndex(vendingInventory, out VendingMachineInventoryPrototype? inventoryPrototype))
                {
                    foreach (var (item, amount) in inventoryPrototype.StartingInventory)
                    {
                        if (PrototypeManager.TryIndex(item, out EntityPrototype? entity))
                            total += _pricing.GetEstimatedPrice(entity) * amount;
                    }
                }
                priceSets.Add(total);
            }
            if (priceSets.Any())
                args.Price += priceSets.Max();
        }

        private void OnTryVocalize(Entity<VendingMachineComponent> ent, ref TryVocalizeEvent args)
        {
            args.Cancelled |= ent.Comp.Broken;
        }

        private void UpdateVendingUI(EntityUid uid, EntityUid user, VendingMachineComponent? component = null)
        {
            if (!Resolve(uid, ref component))
                return;

            var authorized = IsAuthorized(uid, user, component);

            var inventory = GetAllInventory(uid, component);
            var state = new VendingMachineUIState(inventory, authorized);
            UISystem.SetUiState(uid, VendingMachineUiKey.Key, state);

            if (authorized)
            {
                SendBalanceToUser(uid, user);
            }
        }

        protected override void UpdateUI(Entity<VendingMachineComponent?> entity)
        {
            if (!Resolve(entity, ref entity.Comp))
                return;

            var inventory = GetAllInventory(entity.Owner, entity.Comp);
            var state = new VendingMachineUIState(inventory, true);
            UISystem.SetUiState(entity.Owner, VendingMachineUiKey.Key, state);

            foreach (var actor in UISystem.GetActors(entity.Owner, VendingMachineUiKey.Key))
            {
                SendBalanceToUser(entity.Owner, actor);
            }
        }

        private bool TryGetAccountNumber(EntityUid user, out string accountNumber)
        {
            accountNumber = string.Empty;
            if (_inventory.TryGetSlotEntity(user, "id", out var idEntity))
            {
                if (TryComp<PdaComponent>(idEntity, out var pda) && pda.ContainedId != null)
                {
                    if (TryComp<IdBankAccountComponent>(pda.ContainedId.Value, out var bankComp))
                    {
                        accountNumber = bankComp.AccountNumber;
                        return true;
                    }
                }
                else if (TryComp<IdBankAccountComponent>(idEntity, out var bankComp))
                {
                    accountNumber = bankComp.AccountNumber;
                    return true;
                }
            }
            return false;
        }

        private void AddInventoryFromPrototype(EntityUid uid, Dictionary<string, uint>? entries,
            InventoryType type,
            VendingMachineComponent? component = null, float restockQuality = 1.0f)
        {
            if (!Resolve(uid, ref component) || entries == null)
                return;

            Dictionary<string, VendingMachineInventoryEntry> inventory;
            switch (type)
            {
                case InventoryType.Regular: inventory = component.Inventory; break;
                case InventoryType.Emagged: inventory = component.EmaggedInventory; break;
                case InventoryType.Contraband: inventory = component.ContrabandInventory; break;
                default: return;
            }

            foreach (var (id, amount) in entries)
            {
                if (!PrototypeManager.HasIndex<EntityPrototype>(id))
                    continue;

                var restock = amount;
                if (restockQuality < 1.0f && !_random.Prob(restockQuality))
                {
                    restock = (uint)_random.Next(0, (int)amount);
                }

                if (inventory.TryGetValue(id, out var entry))
                {
                    entry.Amount = Math.Min(entry.Amount + restock, amount * 3);
                }
                else
                {
                    inventory.Add(id, new VendingMachineInventoryEntry(type, id, restock));
                }
            }
        }

        public void RestockInventoryFromPrototype(EntityUid uid,
            VendingMachineComponent? component = null, float restockQuality = 1f)
        {
            if (!Resolve(uid, ref component))
                return;

            if (!PrototypeManager.TryIndex(component.PackPrototypeId, out VendingMachineInventoryPrototype? packPrototype))
                return;

            AddInventoryFromPrototype(uid, packPrototype.StartingInventory, InventoryType.Regular, component, restockQuality);
            AddInventoryFromPrototype(uid, packPrototype.EmaggedInventory, InventoryType.Emagged, component, restockQuality);
            AddInventoryFromPrototype(uid, packPrototype.ContrabandInventory, InventoryType.Contraband, component, restockQuality);

            if (packPrototype.ItemPrices.Count > 0)
            {
                ApplyPricesToInventory(component.Inventory, packPrototype.ItemPrices);
                ApplyPricesToInventory(component.EmaggedInventory, packPrototype.ItemPrices);
                ApplyPricesToInventory(component.ContrabandInventory, packPrototype.ItemPrices);
            }

            Dirty(uid, component);
        }

        private void ApplyPricesToInventory(Dictionary<string, VendingMachineInventoryEntry> inventory, Dictionary<string, uint> prices)
        {
            foreach (var (itemId, entry) in inventory)
            {
                if (prices.TryGetValue(itemId, out var price))
                {
                    entry.Price = price;
                }
            }
        }

        private void OnBoundUIOpened(EntityUid uid, VendingMachineComponent component, BoundUIOpenedEvent args)
        {
            if (args.UiKey is not VendingMachineUiKey key || key != VendingMachineUiKey.Key)
                return;
            
            SendBalanceToUser(uid, args.Actor);
        }

        private void SendBalanceToUser(EntityUid machine, EntityUid user)
        {
            var station = _station.GetOwningStation(machine);
            if (station != null && TryGetAccountNumber(user, out var accountNumber))
            {
                if (_bank.TryGetAccount(station.Value, accountNumber, out var account))
                {
                    UISystem.ServerSendUiMessage(machine, VendingMachineUiKey.Key, new VendingMachineBalanceMessage(account!.Balance), user);
                }
            }
        }

        public override void AuthorizedVend(EntityUid uid, EntityUid sender, InventoryType type, string itemId, VendingMachineComponent component)
        {
            if (!IsAuthorized(uid, sender, component))
            {
                return;
            }

            var entry = GetEntry(uid, itemId, type, component);

            if (entry == null || entry.Amount <= 0)
            {
                Deny((uid, component), sender);
                return;
            }

            if (entry.Price > 0)
            {
                if (!TryProcessPayment(sender, uid, (int)entry.Price))
                {
                    Popup.PopupEntity(Loc.GetString("vending-machine-insufficient-funds"), uid, sender);
                    Deny((uid, component), sender);
                    return;
                }
            }

            TryEjectVendorItem(uid, type, itemId, component.CanShoot, sender, component);
        }

        private bool TryProcessPayment(EntityUid user, EntityUid machine, int amount)
        {
            var station = _station.GetOwningStation(machine);
            if (station == null)
                return false;

            if (!TryGetAccountNumber(user, out var accountNumber))
                return false;

            if (_bank.TryChangeBalance(station.Value, accountNumber, -amount))
            {
                if (_bank.TryGetAccount(station.Value, accountNumber, out var account))
                {
                    UISystem.ServerSendUiMessage(machine, VendingMachineUiKey.Key, new VendingMachineBalanceMessage(account!.Balance), user);
                }
                return true;
            }

            return false;
        }
        // Stories-Economy-End
    }
}
