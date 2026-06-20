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

        private void UpdateBandwidth()
        {
            Map map = this.parent.Map;
            if (map == null) return;

            int bandwidth = 0;
            if (powerComp == null || powerComp.PowerOn)
            {
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
                            bandwidth += 3 * thing.stackCount;
                        }
                        else if (thing.def.defName == "SubcoreHigh")
                        {
                            bandwidth += 8 * thing.stackCount;
                        }
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
                    break; // Apply to the first mechanitor found (typically the only one)
                }
            }

            if (mechanitor != null)
            {
                HediffDef hediffDef = DefDatabase<HediffDef>.GetNamed("SC_SubcoreBufferBandwidth", false);
                if (hediffDef != null)
                {
                    Hediff hediff = mechanitor.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                    if (bandwidth > 0)
                    {
                        if (hediff == null)
                        {
                            hediff = HediffMaker.MakeHediff(hediffDef, mechanitor);
                            hediff.Severity = bandwidth;
                            mechanitor.health.AddHediff(hediff);
                        }
                        else
                        {
                            hediff.Severity = bandwidth;
                        }
                    }
                    else if (hediff != null)
                    {
                        mechanitor.health.RemoveHediff(hediff);
                    }
                }
            }
        }

        private void RemoveHediff(Map map)
        {
            if (map == null) return;
            HediffDef hediffDef = DefDatabase<HediffDef>.GetNamed("SC_SubcoreBufferBandwidth", false);
            if (hediffDef == null) return;

            IReadOnlyList<Pawn> pawns = map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn p = pawns[i];
                if (p.Faction == Faction.OfPlayer && p.mechanitor != null)
                {
                    Hediff hediff = p.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                    if (hediff != null)
                    {
                        p.health.RemoveHediff(hediff);
                    }
                }
            }
        }
    }
}
