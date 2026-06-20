using System;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace SteelColony
{
    public static class LogisticsUtility
    {
        public static float CalculateTradePriceFactor(Map map)
        {
            if (map == null) return 0.70f;

            int highestIntellectual = 0;
            // Iterate over colonists and player mechs (if any have skills tracker patched)
            var pawns = map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < pawns.Count; i++)
            {
                var p = pawns[i];
                if (p.Faction == Faction.OfPlayer && p.skills != null)
                {
                    int skill = p.skills.GetSkill(SkillDefOf.Intellectual).Level;
                    if (skill > highestIntellectual)
                    {
                        highestIntellectual = skill;
                    }
                }
            }

            // Intellectual Level 20 results in 0.70 + 20 * 0.015 = 1.0f price factor.
            float factor = 0.70f + (highestIntellectual * 0.015f);
            return UnityEngine.Mathf.Clamp(factor, 0.70f, 1.0f);
        }

        public static LeaseContract GenerateLeaseContract()
        {
            string[] templates = new string[] { "Mining", "Medical", "Defense", "Repair", "Hauler" };
            string type = templates[Rand.Range(0, templates.Length)];
            
            string id = Guid.NewGuid().ToString().Substring(0, 8);
            string title = "";
            string description = "";
            int durationTicks = Rand.Range(3, 8) * 60000; // 3 to 8 days (1 day = 60000 ticks)
            List<string> requiredMechs = new List<string>();
            int rewardValue = 0;

            switch (type)
            {
                case "Mining":
                    title = "Remote Mining Excavation";
                    requiredMechs.Add("SC_Mech_MiningDrone");
                    int countMin = Rand.Range(1, 3);
                    for (int i = 1; i < countMin; i++)
                    {
                        requiredMechs.Add("SC_Mech_MiningDrone");
                    }
                    description = $"A friendly settlement needs {requiredMechs.Count} Mining Drone(s) to excavate a blocked mountain pass under extreme weather conditions.";
                    rewardValue = requiredMechs.Count * Rand.Range(400, 700);
                    break;

                case "Medical":
                    title = "Plague Relief Support";
                    requiredMechs.Add("Paramedic");
                    description = "An Outlander community is suffering from a toxic plague. They require 1 Paramedic mechanoid to assist their local doctors.";
                    rewardValue = Rand.Range(800, 1200);
                    break;

                case "Defense":
                    title = "Outpost Security Guard";
                    requiredMechs.Add("SC_Mech_Sentinel");
                    description = "A nearby trade hub is facing imminent pirate raid threats and requests 1 Sentinel mech to guard their perimeter.";
                    rewardValue = Rand.Range(1000, 1500);
                    break;

                case "Repair":
                    title = "Infrastructure Reconstruction";
                    requiredMechs.Add("SC_Mech_RepairDrone");
                    int countRep = Rand.Range(1, 3);
                    for (int i = 1; i < countRep; i++)
                    {
                        requiredMechs.Add("SC_Mech_RepairDrone");
                    }
                    description = $"A neighboring faction requires {requiredMechs.Count} Repair Drone(s) to patch automated power grids damaged by solar flare surges.";
                    rewardValue = requiredMechs.Count * Rand.Range(350, 600);
                    break;

                case "Hauler":
                    title = "Logistical Cargo Transport";
                    requiredMechs.Add("SC_Mech_HaulerMkII");
                    description = "A local orbital trade depot requests 1 Hauler Mk2 to transport and organize heavy cargo shipments.";
                    rewardValue = Rand.Range(500, 800);
                    break;
            }

            return new LeaseContract(id, title, description, durationTicks, requiredMechs, rewardValue);
        }

        public static void SendDropPodRewards(Map map, int silverValue, IntVec3 position)
        {
            if (map == null) return;

            List<Thing> rewards = new List<Thing>();
            
            // Value division: 60% silver, 20% plasteel, 20% components
            int silverAmount = (int)(silverValue * 0.6f);
            int plasteelAmount = (int)((silverValue * 0.2f) / 8f);
            int compAmount = (int)((silverValue * 0.2f) / 32f);

            if (silverAmount > 0)
            {
                Thing silver = ThingMaker.MakeThing(ThingDefOf.Silver);
                silver.stackCount = silverAmount;
                rewards.Add(silver);
            }
            if (plasteelAmount > 0)
            {
                Thing plasteel = ThingMaker.MakeThing(ThingDefOf.Plasteel);
                plasteel.stackCount = UnityEngine.Mathf.Max(1, plasteelAmount);
                rewards.Add(plasteel);
            }
            if (compAmount > 0)
            {
                Thing comp = ThingMaker.MakeThing(ThingDefOf.ComponentIndustrial);
                comp.stackCount = UnityEngine.Mathf.Max(1, compAmount);
                rewards.Add(comp);
            }

            // Rare bonus for high value contracts
            if (silverValue >= 1200)
            {
                Thing advComp = ThingMaker.MakeThing(ThingDefOf.ComponentSpacer);
                advComp.stackCount = 1;
                rewards.Add(advComp);
            }

            DropPodUtility.DropThingsNear(position, map, rewards);

            Messages.Message($"A cargo shuttle drops rewards (Value: {silverValue} silver equivalent) for the completed lease contract.", 
                new TargetInfo(position, map), MessageTypeDefOf.PositiveEvent);
        }
    }
}
