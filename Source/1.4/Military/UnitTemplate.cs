using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Empire_Rewritten.Military
{
    public class UnitTemplate : ILoadReferenceable, IExposable
    {
        private string _name;
        public string Name => _name;

        private List<Apparel> _apparel;
        public List<Apparel> Apparel => _apparel;

        private List<ThingWithComps> _weapons;
        public List<ThingWithComps> Weapons => _weapons;
        public ThingWithComps PrimaryWeapon => _weapons?.FirstOrDefault();
        public List<ThingWithComps> Inventory => _weapons?.Skip(1).ToList();

        private int _skillLevel;
        public int SkillLevel => _skillLevel;

        private string _loadID;
        public UnitTemplate()
        {
            _loadID = "unitTemplate_" + Guid.NewGuid().ToString();
            _apparel = new List<Apparel>();
            _weapons = new List<ThingWithComps>();
        }

        public UnitTemplate(string name, int skillLevel) : this()
        {
            _name = name;
            _skillLevel = skillLevel;
        }

        public Pawn CreateNewPawn(XenotypeDef xenotype)
        {
            PawnGenerationRequest request = new PawnGenerationRequest(
                PawnKindDefOf.Colonist,
                Faction.OfPlayer,
                forceGenerateNewPawn: true,
                colonistRelationChanceFactor: 0f,
                mustBeCapableOfViolence: true,
                allowFood: false,
                prohibitedTraits: new List<TraitDef>(),
                allowedXenotypes: new List<XenotypeDef>() { xenotype },
                biologicalAgeRange: new FloatRange(20, 40)
                );

            Pawn pawn = PawnGenerator.GeneratePawn(request);

            // Set the pawn's shooting and melee skills
            pawn.skills.GetSkill(SkillDefOf.Shooting).Level = _skillLevel;
            pawn.skills.GetSkill(SkillDefOf.Melee).Level = _skillLevel;

            // Add apparel
            pawn.apparel.DestroyAll();
            foreach (Apparel apparel in _apparel)
            {
                Apparel duplicate = (Apparel)ThingMaker.MakeThing(apparel.def, apparel.Stuff);

                if (apparel.PawnCanWear(pawn) && pawn.apparel.CanWearWithoutDroppingAnything(duplicate.def))
                    pawn.apparel.Wear(duplicate);
            }

            // Add weapons and inventory
            pawn.equipment.DestroyAllEquipment();
            if (_weapons.Count > 0)
            {
                // Add primary weapon
                ThingWithComps duplicate = (ThingWithComps)ThingMaker.MakeThing(PrimaryWeapon.def, PrimaryWeapon.Stuff);

                if (pawn.equipment.Primary == null)
                {
                    pawn.equipment.AddEquipment(duplicate);
                }

                // Add all other weapons as inventory
                foreach (ThingWithComps item in Inventory)
                {
                    duplicate = (ThingWithComps)ThingMaker.MakeThing(item.def, item.Stuff);
                    pawn.inventory.innerContainer.TryAdd(duplicate);
                }
            }

            if (!pawn.IsWorldPawn())
                Find.WorldPawns.PassToWorld(pawn);

            return pawn;
        }

        public bool AddApparel(Apparel apparel)
        {
            CompBiocodable compBiocodable = apparel.TryGetComp<CompBiocodable>();
            if (compBiocodable != null) return false;

            _apparel.Add(apparel);
            return true;
        }

        public bool AddWeapon(ThingWithComps weapon)
        {
            CompBiocodable compBiocodable = weapon.TryGetComp<CompBiocodable>();
            if (compBiocodable != null) return false;

            _weapons.Add(weapon);
            return true;
        }

        public int UnitStrength()
        {
            // TODO Balance this; I'm just guessing here
            // For higher-tier, we have ~ 20 base, 40 for skill, 50 for 3k apparel, and 50 for 2k weapon
            return 20 + _skillLevel * 2 + (int)_apparel.Sum(a => a.MarketValue) / 60 + (int)_weapons.Sum(w => w.MarketValue) / 40;
        }

        public string GetUniqueLoadID()
        {
            return _loadID;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref _name, "name");
            Scribe_Collections.Look(ref _apparel, "apparel", LookMode.Deep);
            Scribe_Collections.Look(ref _weapons, "weapons", LookMode.Deep);
            Scribe_Values.Look(ref _skillLevel, "skillLevel");
            Scribe_Values.Look(ref _loadID, "loadID");
        }
    }
}
