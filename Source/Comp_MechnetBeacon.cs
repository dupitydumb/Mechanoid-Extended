using System.Collections.Generic;
using Verse;
using RimWorld;

namespace SteelColony
{
    public class CompProperties_MechnetBeacon : CompProperties
    {
        public float radius = 12f;
        public CompProperties_MechnetBeacon()
        {
            this.compClass = typeof(Comp_MechnetBeacon);
        }
    }

    public class Comp_MechnetBeacon : ThingComp
    {
        public CompProperties_MechnetBeacon Props => (CompProperties_MechnetBeacon)this.props;

        private CompPowerTrader powerComp;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            powerComp = this.parent.GetComp<CompPowerTrader>();
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            if (powerComp == null || powerComp.PowerOn)
            {
                ApplyBuffsInRadius();
            }
        }

        private void ApplyBuffsInRadius()
        {
            Map map = this.parent.Map;
            if (map == null) return;

            float r = Props.radius;
            IntVec3 pos = this.parent.Position;

            IReadOnlyList<Pawn> pawns = map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn p = pawns[i];
                if (p.RaceProps.IsMechanoid && p.Faction == this.parent.Faction)
                {
                    if ((p.Position - pos).LengthHorizontalSquared <= r * r)
                    {
                        HediffDef hediffDef = DefDatabase<HediffDef>.GetNamed("SC_MechnetBoost", false);
                        if (hediffDef != null)
                        {
                            Hediff hediff = p.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                            if (hediff == null)
                            {
                                hediff = HediffMaker.MakeHediff(hediffDef, p);
                                hediff.Severity = 1f;
                                p.health.AddHediff(hediff);
                            }
                            else
                            {
                                var comp = hediff.TryGetComp<HediffComp_Disappears>();
                                if (comp != null)
                                {
                                    comp.ticksToDisappear = 400; // Refresh duration (~6.6 seconds)
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
