using System.Collections.Generic;
using Verse;
using RimWorld;

namespace SteelColony
{
    public class CompProperties_SubcoreBuffer : CompProperties
    {
        public CompProperties_SubcoreBuffer()
        {
            this.compClass = typeof(Comp_SubcoreBuffer);
        }
    }

    public class Comp_SubcoreBuffer : ThingComp
    {
        private CompPowerTrader powerComp;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            powerComp = this.parent.GetComp<CompPowerTrader>();
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            UpdateBandwidth();
        }

        public override void PostDeSpawn(Map map, DestroyMode mode)
        {
            base.PostDeSpawn(map, mode);
            // Clean up the hediff from any mechanitor when this rack is uninstalled or destroyed
            RemoveHediff(map);
        }

        public int CalculateStoredBandwidth()
        {
            if (powerComp != null && !powerComp.PowerOn)
            {
                return 0;
            }

            Map map = this.parent.Map;
            if (map == null) return 0;

            int bandwidth = 0;
            foreach (IntVec3 cell in this.parent.OccupiedRect())
            {
                List<Thing> things = cell.GetThingList(map);
                for (int i = 0; i < things.Count; i++)
                {
                    Thing thing = things[i];
                    if (thing.def.defName == "SubcoreBasic")
                    {
                        bandwidth += 1 * thing.stackCount;
                    }
                    else if (thing.def.defName == "SubcoreRegular" || thing.def.defName == "SubcoreStandard")
                    {
                        bandwidth += 2 * thing.stackCount;
                    }
                    else if (thing.def.defName == "SubcoreHigh")
                    {
                        bandwidth += 4 * thing.stackCount;
                    }
                }
            }
            return bandwidth;
        }

        private void UpdateBandwidth()
        {
            Map map = this.parent.Map;
            if (map == null) return;
            UpdateBandwidthForMap(map);
        }

        private void RemoveHediff(Map map)
        {
            if (map == null) return;
            UpdateBandwidthForMap(map);
        }

        private void UpdateBandwidthForMap(Map map)
        {
            int totalBandwidth = 0;
            var buildings = map.listerBuildings.allBuildingsColonist;
            for (int i = 0; i < buildings.Count; i++)
            {
                var b = buildings[i];
                if (b.def.defName == "SC_SubcoreBuffer")
                {
                    if (!b.Spawned) continue;

                    var comp = b.GetComp<Comp_SubcoreBuffer>();
                    if (comp != null)
                    {
                        totalBandwidth += comp.CalculateStoredBandwidth();
                    }
                }
            }

            // Find player mechanitor on this map
            Pawn mechanitor = null;
            IReadOnlyList<Pawn> pawns = map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn p = pawns[i];
                if (p.Faction == Faction.OfPlayer && p.mechanitor != null)
                {
                    mechanitor = p;
                    break;
                }
            }

            if (mechanitor != null)
            {
                HediffDef hediffDef = DefDatabase<HediffDef>.GetNamed("SC_SubcoreBufferBandwidth", false);
                if (hediffDef != null)
                {
                    Hediff hediff = mechanitor.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                    if (totalBandwidth > 0)
                    {
                        if (hediff == null)
                        {
                            hediff = HediffMaker.MakeHediff(hediffDef, mechanitor);
                            hediff.Severity = totalBandwidth;
                            mechanitor.health.AddHediff(hediff);
                        }
                        else
                        {
                            hediff.Severity = totalBandwidth;
                        }
                    }
                    else if (hediff != null)
                    {
                        mechanitor.health.RemoveHediff(hediff);
                    }
                }
            }
        }
    }
}
