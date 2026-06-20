using Verse;
using RimWorld;

namespace SteelColony
{
    public class CompProperties_CentralControl : CompProperties
    {
        public CompProperties_CentralControl()
        {
            this.compClass = typeof(Comp_CentralControl);
        }
    }

    public class Comp_CentralControl : ThingComp
    {
        private CompPowerTrader powerComp;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            powerComp = this.parent.GetComp<CompPowerTrader>();
        }

        public bool Active
        {
            get
            {
                // Must be powered
                if (powerComp != null && !powerComp.PowerOn)
                {
                    return false;
                }
                
                // Research check
                ResearchProjectDef research = DefDatabase<ResearchProjectDef>.GetNamed("SC_DistributedOverseer", false);
                if (research != null && !research.IsFinished)
                {
                    return false;
                }

                return true;
            }
        }
    }

    public static class CentralControlUtility
    {
        public static bool IsCentralControlActive(Map map)
        {
            if (map == null) return false;

            var buildings = map.listerBuildings.allBuildingsColonist;
            for (int i = 0; i < buildings.Count; i++)
            {
                var b = buildings[i];
                if (b.def.defName == "SC_MechCentralControl")
                {
                    var cc = b.GetComp<Comp_CentralControl>();
                    if (cc != null && cc.Active)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
