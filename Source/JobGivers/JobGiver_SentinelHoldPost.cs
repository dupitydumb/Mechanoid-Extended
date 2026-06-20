using System.Linq;
using Verse;
using Verse.AI;
using RimWorld;

namespace SteelColony
{
    public class JobGiver_SentinelHoldPost : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            var postComp = pawn.GetComp<Comp_SentinelPost>();
            if (postComp == null || !postComp.postPosition.IsValid)
            {
                return null;
            }

            IntVec3 postPos = postComp.postPosition;
            Map map = pawn.Map;
            if (map == null) return null;

            // 1. Check for threats within range
            Thing target = FindEnemyTarget(pawn);
            if (target != null)
            {
                // Shoot immediately from current position
                Job shootJob = JobMaker.MakeJob(JobDefOf.AttackStatic, target);
                shootJob.maxNumStaticAttacks = 1;
                return shootJob;
            }

            // 2. Return to post if away
            if (pawn.Position != postPos)
            {
                return JobMaker.MakeJob(JobDefOf.Goto, postPos);
            }

            // 3. Stand guard (Wait)
            Job waitJob = JobMaker.MakeJob(JobDefOf.Wait, 100);
            return waitJob;
        }

        private Thing FindEnemyTarget(Pawn pawn)
        {
            float range = 30f;
            if (pawn.equipment?.Primary != null)
            {
                range = pawn.equipment.Primary.def.Verbs.FirstOrDefault()?.range ?? 30f;
            }

            Map map = pawn.Map;
            Thing bestTarget = null;
            float bestDist = float.MaxValue;

            var targets = map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < targets.Count; i++)
            {
                Pawn p = targets[i];
                if (p.Faction != null && p.Faction.HostileTo(pawn.Faction) && !p.Downed && !p.Dead)
                {
                    float dist = (p.Position - pawn.Position).LengthHorizontal;
                    if (dist <= range && dist < bestDist)
                    {
                        if (GenSight.LineOfSight(pawn.Position, p.Position, map, true))
                        {
                            bestTarget = p;
                            bestDist = dist;
                        }
                    }
                }
            }
            return bestTarget;
        }
    }
}
