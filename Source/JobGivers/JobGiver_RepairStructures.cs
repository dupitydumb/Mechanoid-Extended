using Verse;
using Verse.AI;
using RimWorld;

namespace SteelColony
{
    public class JobGiver_RepairStructures : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            return TryGiveRepairJob(pawn);
        }

        public static Job TryGiveRepairJob(Pawn pawn)
        {
            Map map = pawn.Map;
            if (map == null) return null;

            Thing bestTarget = null;
            float bestDist = float.MaxValue;

            var buildings = map.listerBuildings.allBuildingsColonist;
            for (int i = 0; i < buildings.Count; i++)
            {
                var b = buildings[i];
                if (b.Faction == pawn.Faction && b.def.useHitPoints && b.HitPoints < b.MaxHitPoints)
                {
                    if (pawn.CanReserveAndReach(b, PathEndMode.Touch, Danger.Some))
                    {
                        float dist = (pawn.Position - b.Position).LengthHorizontal;
                        if (dist < bestDist)
                        {
                            bestTarget = b;
                            bestDist = dist;
                        }
                    }
                }
            }

            if (bestTarget != null)
            {
                return JobMaker.MakeJob(JobDefOf.Repair, bestTarget);
            }
            return null;
        }
    }
}
