using Empire_Rewritten.Events;
using Empire_Rewritten.Factions;
using Empire_Rewritten.Resources;
using Empire_Rewritten.Settlements;
using Empire_Rewritten.Territories;
using Empire_Rewritten.Utils;
using JetBrains.Annotations;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Empire_Rewritten
{
    /// <summary>
    /// </summary>
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
    public class EmpireWorldComp : WorldComponent
    {
        /// <summary>
        ///     Current world's static instance of <see cref="EmpireWorldComp" />
        /// </summary>
        public static EmpireWorldComp Current { get; private set; }

        public const int TICK_DAILY = 60000;
        public const int TICK_HOURLY = 2500;

        /* TICK ACTIONS */
        private List<WorldCompAction> _actions = new List<WorldCompAction>();
        public List<WorldCompAction> Actions
        {
            set
            {
                if (_actions != null)
                {
                    Logger.Warn("Skipping Actions assignment: already exists");
                    return;
                }

                _actions = value;
            }
            get => _actions;
        }

        /* GLOBAL CONTROLLERS */
        private PlayerController _playerController;
        public PlayerController PlayerController
        {
            set
            {
                if (_playerController != null)
                {
                    Logger.Warn("Skipping PlayerController assignment: already exists");
                    return;
                }

                _playerController = value;
            }
            get => _playerController;
        }

        private EventManager _eventManager;
        public EventManager EventManager
        {
            set
            {
                if (_eventManager != null)
                {
                    Logger.Warn("Skipping EventManager assignment: already exists");
                    return;
                }

                _eventManager = value;
            }
            get => _eventManager;
        }

        private TerritoryManager _territoryManager;
        public TerritoryManager TerritoryManager
        {
            set
            {
                if (_territoryManager != null)
                {
                    Logger.Warn("Skipping TerritoryManager assignment: already exists");
                    return;
                }

                _territoryManager = value;
            }
            get => _territoryManager;
        }

        /* GLOBAL DATA DICTS */
        private Dictionary<Settlement, SettlementDetails> _settlementDetails;
        public Dictionary<Settlement, SettlementDetails> SettlementDetails
        {
            set
            {
                if (_settlementDetails != null)
                {
                    Logger.Warn("Skipping SettlementDetails assignment: already exists");
                    return;
                }

                _settlementDetails = value;
            }
            get => _settlementDetails;
        }

        private Dictionary<Faction, FactionDetails> _factionDetails;
        public Dictionary<Faction, FactionDetails> FactionDetails
        {
            set
            {
                if (_factionDetails != null)
                {
                    Logger.Warn("Skipping FactionDetails assignment: already exists");
                    return;
                }

                _factionDetails = value;
            }

            get => _factionDetails;
        }

        /* PLAYER FACTION DETAILS */
        private Dictionary<ResourceDef, TaxList> _taxDetails;
        public Dictionary<ResourceDef, TaxList> TaxDetails
        {
            set
            {
                if (_taxDetails != null)
                {
                    Logger.Warn("Skipping TaxDetails assignment: already exists");
                    return;
                }

                _taxDetails = value;
            }
            get => _taxDetails;
        }

        private float _taxRate = 0.1f;
        public float TaxRate
        {
            set => _taxRate = value;
            get => _taxRate;
        }

        public EmpireWorldComp(World world) : base(world)
        {
            Current = this;
        }

        public List<Faction> GetTrackedFactions()
        {
            return _factionDetails.Keys.ToList();
        }

        public List<Settlement> GetTrackedSettlements()
        {
            return _settlementDetails.Keys.ToList();
        }

        /// <summary>
        ///     Registers an <see cref="WorldCompAction" /> to be called as determined by its
        ///     <see cref="WorldCompAction.ShouldExecute" /> method
        /// </summary>
        /// <param name="action">The <see cref="WorldCompAction" /> to be added</param>
        public void AddUpdateCall([NotNull] WorldCompAction action)
        {
            _actions.Add(action);
        }

        /// <summary>
        ///     Calls each registered <see cref="WorldCompAction" />.
        /// </summary>
        public override void WorldComponentTick()
        {
            // _actions is modified during the loop, so we need to copy it first.
            List<WorldCompAction> actions = _actions.ListFullCopy();
            actions.ForEach(action => action.TryExecute(this));

            // Remove all actions that should be discarded, but only if it was executed.
            _actions.RemoveAll(action => actions.Contains(action) && action.ShouldDiscard());
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();

            _InitPlayers();
            _InitActions();

            // The following code will create new objects for everything, because everything is empty until
            //   ResolvingCrossRefs is called. The order is LoadingVars, FinalizeInit, and then ResolvingCrossRefs.
            //   Scribe.mode stays at LoadingVars until after FinalizeInit.
            if (Scribe.mode == LoadSaveMode.LoadingVars) return;

            // This is the start of a new game, so init everything else.
            _InitFactions();
            _RemoveSettlements();
            _InitMissingSettlements();
            _InitTerritories();
            _InitEvents();
        }

        private void _InitPlayers()
        {
            if (PlayerController is null) PlayerController = new PlayerController();
            foreach (Faction faction in Find.World.factionManager.AllFactions)
            {
                if (PlayerController.Players.ContainsKey(faction)) continue;

                if (faction.Hidden) continue;
                if (!faction.IsPlayer) PlayerController.CreateAIPlayer(faction);
                else PlayerController.CreateUserPlayer();

            }
        }

        private void _InitFactions()
        {
            if (FactionDetails is null) FactionDetails = new Dictionary<Faction, FactionDetails>();
            foreach (Faction faction in Find.World.factionManager.AllFactions)
            {
                if (faction.IsPlayer || faction.Hidden) continue;

                if (!FactionDetails.ContainsKey(faction))
                {
                    FactionDetails.Add(faction, new FactionDetails(faction));

                    faction.GetStockpile().Add(EmpireDefOf.Empire_Res_Food, 200 * (int)faction.def.techLevel);
                    faction.GetStockpile().Add(EmpireDefOf.Empire_Res_Wood, 100 * (int)faction.def.techLevel - 1);
                    faction.GetStockpile().Add(EmpireDefOf.Empire_Res_Stone, 100 * (int)faction.def.techLevel - 2);
                }
            }
        }

        private void _RemoveSettlements()
        {
            List<Faction> factions = GetTrackedFactions();
            // Sort by # of settlements, descending, so we remove the largest factions first.
            factions.Sort((x, y) => y.GetAllSettlements().Count().CompareTo(x.GetAllSettlements().Count()));

            foreach (Faction faction in factions)
            {
                if (faction.IsPlayer) continue;

                List<Settlement> settlements = Find.WorldObjects.Settlements.Where(x => x.Faction == faction).ToList();
                if (settlements.Count() == 0) continue;

                // Find settlement furthest from all others
                Settlement settlement = settlements[0];
                float distance = 0;
                foreach (Settlement item in settlements)
                {
                    // Ignore the faction's own settlements, because we will be removing it.
                    float dist = settlements.Min(x => item.Faction == settlement.Faction ? 100000 : Find.WorldGrid.ApproxDistanceInTiles(item.Tile, x.Tile));
                    if (dist > distance)
                    {
                        settlement = item;
                        distance = dist;
                    }
                }

                // Remove all others
                settlements.Remove(settlement);
                foreach (Settlement item in settlements) item.Destroy();
            }
        }

        private void _InitMissingSettlements()
        {
            if (SettlementDetails is null) SettlementDetails = new Dictionary<Settlement, SettlementDetails>();
            foreach (Settlement settlement in Find.WorldObjects.Settlements)
            {
                if (settlement.Faction is null) continue;

                if (SettlementDetails.ContainsKey(settlement) || settlement.Faction.IsPlayer) continue;
                SettlementDetails sd = new SettlementDetails(settlement);
                SettlementDetails.Add(settlement, sd);

                sd.Level = (int)settlement.Faction.def.techLevel / 2 + 2;
                sd.BuildFacility(EmpireDefOf.Empire_Fac_Pasture, instant: true);
                // Food is always a priority, so we'll uplevel the pasture to the tech level.
                if (sd.HasFacility(EmpireDefOf.Empire_Fac_Pasture))
                    sd.ChangeFacilitySize(EmpireDefOf.Empire_Fac_Pasture, sd.Level - 1);

                sd.BuildFacility(EmpireDefOf.Empire_Fac_Lumberhut, instant: true);
                sd.BuildFacility(EmpireDefOf.Empire_Fac_Quarry, instant: true);
                sd.BuildFacility(EmpireDefOf.Empire_Fac_Farm, instant: true);
                sd.BuildFacility(EmpireDefOf.Empire_Fac_Mine, instant: true);


                if (sd.GetFacilities().Count() == 0)
                {
                    // No facilities, so we'll add a max-level gathering hut as a fallback
                    sd.BuildFacility(EmpireDefOf.Empire_Fac_GatheringHut, instant: true);
                    sd.ChangeFacilitySize(EmpireDefOf.Empire_Fac_GatheringHut, sd.Level - 1);
                }
            }
        }

        private void _InitTerritories()
        {
            if (TerritoryManager is null) TerritoryManager = new TerritoryManager();

            // Add claims for all settlements
            foreach (Settlement settlement in GetTrackedSettlements())
            {
                // Add claims doesn't remove claims, so it won't overwrite existing claims.
                if (TerritoryManager.SettlementHasClaims(settlement)) continue;
                TerritoryManager.AddClaims(settlement);
            }
        }

        private void _InitEvents()
        {
            if (EventManager is null) EventManager = new EventManager();

            // Nothing needed here yet.
        }

        private void _InitActions()
        {
            // Note: Make sure that resources are ticked before the AI, as the AI uses the resources.
            AddUpdateCall(new WorldCompAction(_TickResources, TICK_DAILY, tickOffset: 1));
            AddUpdateCall(new WorldCompAction(_TickPlayerTax, TICK_DAILY, tickOffset: 5));
            AddUpdateCall(new WorldCompAction(_TickAI, TICK_DAILY / 4, tickOffset: 10));

            AddUpdateCall(new WorldCompAction((EmpireWorldComp e) => EventManager.Tick(e), TICK_HOURLY, tickOffset: 30));

            // This adds missing Settlements and Factions, for compatibility, although things may not work as expected.
            AddUpdateCall(new WorldCompAction(_TickUpdateMissing, TICK_DAILY, tickOffset: 20));
        }

        private static void _TickUpdateMissing(EmpireWorldComp e)
        {
            e._InitPlayers();
            e._InitFactions();
            e._InitMissingSettlements();
            e._InitTerritories();
        }

        private static void _TickResources(EmpireWorldComp e)
        {
            foreach (Faction f in e.GetTrackedFactions())
            {
                Dictionary<ResourceDef, float> production = f.GetAdjustedResourceChange();
                Dictionary<ResourceDef, float> stockpile = f.GetStockpile();

                foreach (ResourceDef res in production.Keys)
                {
                    if (stockpile.ContainsKey(res))
                        stockpile[res] += production[res];
                    else
                        stockpile.Add(res, production[res]);

                    stockpile[res] = Math.Max(stockpile[res], 0);
                }
            }
        }

        private static void _TickPlayerTax(EmpireWorldComp e)
        {
            if (Find.WorldObjects.Settlements.Where(x => x.IsPlayerMap()).Count() == 0) return;
            if (!e.GetTrackedFactions().Contains(Faction.OfPlayer)) return;

            Dictionary<ResourceDef, float> production = Faction.OfPlayer.GetAdjustedResourceChange();

            float silver = 0;
            List<Thing> otherTax = new List<Thing>();

            foreach (ResourceDef res in production.Keys)
            {
                // First, compute how much tax we can actually get
                float tax = e.GetMaxTax(res);

                // Grab the tax that the player requested and deduct it
                TaxList requestedTax;
                if (! e.TaxDetails.TryGetValue(res, out requestedTax)) requestedTax = new TaxList();

                float valueOfRequestedTax = (int)Math.Ceiling(requestedTax.Things.Sum(x => x.MarketValue * x.stackCount));

                // There are many reasons for this (e.g. facility destroyed, etc.)
                float lastValue = valueOfRequestedTax;
                while (valueOfRequestedTax > tax)
                {
                    // First, try to multiply the stack count by a smaller number
                    foreach (Thing t in requestedTax.Things)
                        t.stackCount = (int)Math.Ceiling((double)t.stackCount * 3 / 4);

                    lastValue = valueOfRequestedTax;
                    valueOfRequestedTax = (int)Math.Ceiling(requestedTax.Things.Sum(x => x.MarketValue * x.stackCount));

                    // If we can't reduce it any further, just remove a random thing
                    if (lastValue == valueOfRequestedTax)
                    {
                        Thing t = requestedTax.Things.RandomElement();
                        requestedTax.Things.Remove(t);
                        valueOfRequestedTax = (int)Math.Ceiling(requestedTax.Things.Sum(x => x.MarketValue * x.stackCount));
                    }
                    else
                    {
                        // This means that something was reduced, so we'll just start this while loop over again
                    }
                }

                silver += tax - valueOfRequestedTax;
                foreach (Thing t in requestedTax.Things)
                {
                    t.stackCount *= 2;
                    Thing copy = t.SplitOff(t.stackCount / 2);

                    // Add to the tax list
                    otherTax.Add(copy);
                }
            }

            // Place it on the player's first settlement without a null map.
            Map map = Find.WorldObjects.Settlements.Where(x => x.IsPlayerMap()).First().Map;

            // First place silver
            if (silver > 0)
            {
                Thing t = ThingMaker.MakeThing(ThingDefOf.Silver);
                t.stackCount = (int)Math.Round(silver);
                TradeUtility.SpawnDropPod(DropCellFinder.TradeDropSpot(map), map, t);
            }
            // Then place the rest of the tax
            foreach (Thing t in otherTax)
            {
                TradeUtility.SpawnDropPod(DropCellFinder.TradeDropSpot(map), map, t);
            }
        }

        private static void _TickAI(EmpireWorldComp e)
        {
            foreach (Faction f in e.GetTrackedFactions())
            {
                if (f != Faction.OfPlayer) f.GetPlayer().MakeMove(e);
            }
        }

        public float GetMaxTax(ResourceDef resourceDef)
        {
            Dictionary<ResourceDef, float> production = Faction.OfPlayer.GetAdjustedResourceChange();
            Dictionary<ResourceDef, float> stockpile = Faction.OfPlayer.GetStockpile();

            float inStock = stockpile.ContainsKey(resourceDef) ? stockpile[resourceDef] : 0;
            float inProd = production.ContainsKey(resourceDef) ? production[resourceDef] : 0;
            float tax = inProd * TaxRate;
            tax = Math.Min(tax, inStock + inProd);

            return tax;
        }

        public void AddSettlement(Settlement settlement)
        {
            Find.WorldObjects.Add(settlement);
            SettlementDetails.Add(settlement, new SettlementDetails(settlement));
            TerritoryManager.AddClaims(settlement);
        }

        public void _DebugDoTax()
        {
            _TickPlayerTax(this);
        }

        private List<Settlement> _settlementDetailsExposeList1;
        private List<SettlementDetails> _settlementDetailsExposeList2;
        private List<Faction> _factionDetailsExposeList1;
        private List<FactionDetails> _factionDetailsExposeList2;
        public override void ExposeData()
        {
            //Scribe_Deep.Look(ref _factionController, "factionController");
            // TODO Save all data as deep (I think?) as this is where all the data actually lives;
            //     all others are references.

            // Actions should be added by everything else, as they aren't exposable.
            //Scribe_Collections.Look(ref _actions, "actions", LookMode.Deep);
            // Action/playercontroller aren't saved.
            Scribe_Deep.Look(ref _eventManager, "eventManager");
            Scribe_Deep.Look(ref _territoryManager, "territoryManager");

            Scribe_Collections.Look(ref _settlementDetails, "settlementDetails", LookMode.Reference, LookMode.Deep, ref _settlementDetailsExposeList1, ref _settlementDetailsExposeList2);
            Scribe_Collections.Look(ref _factionDetails, "factionDetails", LookMode.Reference, LookMode.Deep, ref _factionDetailsExposeList1, ref _factionDetailsExposeList2);

            Scribe_Collections.Look(ref _taxDetails, "taxDetails", LookMode.Def, LookMode.Deep);
            Scribe_Values.Look(ref _taxRate, "taxRate");
        }



        public class TaxList : IExposable
        {
            private List<Thing> _things;
            public List<Thing> Things
            {
                get => _things;
                set => _things = value;
            }
            public TaxList()
            {
                _things = new List<Thing>();
            }
            public void ExposeData()
            {
                Scribe_Collections.Look(ref _things, "things", LookMode.Deep);
            }
        }
    }
}