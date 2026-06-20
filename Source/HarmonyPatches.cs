using System;
using System.Reflection;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace SteelColony
{
    // 1. Prioritize Repair Drone Repair Jobs
    [HarmonyPatch(typeof(JobGiver_Work), "TryGiveJob")]
    public static class Patch_JobGiver_Work_TryGiveJob
    {
        public static bool Prefix(Pawn pawn, ref Job __result)
        {
            if (pawn.def.defName == "SC_Mech_RepairDrone")
            {
                Job repairJob = JobGiver_RepairStructures.TryGiveRepairJob(pawn);
                if (repairJob != null)
                {
                    __result = repairJob;
                    return false; // Bypass original work giver loop
                }
            }
            return true;
        }
    }

    // 2. Initialize Skills Component for Research Unit
    [HarmonyPatch(typeof(PawnComponentsUtility), "CreateInitialComponents")]
    public static class Patch_PawnComponentsUtility_CreateInitialComponents
    {
        public static void Postfix(Pawn pawn)
        {
            if (pawn.def.defName == "SC_Mech_ResearchUnit")
            {
                if (pawn.skills == null)
                {
                    pawn.skills = new Pawn_SkillTracker(pawn);
                }
            }
        }
    }

    [HarmonyPatch(typeof(PawnComponentsUtility), "AddComponentsForSpawn")]
    public static class Patch_PawnComponentsUtility_AddComponentsForSpawn
    {
        public static void Postfix(Pawn pawn)
        {
            if (pawn.def.defName == "SC_Mech_ResearchUnit")
            {
                if (pawn.skills == null)
                {
                    pawn.skills = new Pawn_SkillTracker(pawn);
                }
            }
        }
    }

    // 3. Set Fixed Skill Levels for Steel Colony Mechanoids
    [HarmonyPatch(typeof(SkillRecord), "get_Level")]
    public static class Patch_SkillRecord_get_Level
    {
        public static void Postfix(SkillRecord __instance, ref int __result)
        {
            Pawn pawn = __instance.Pawn;
            if (pawn == null || pawn.def == null) return;

            string defName = pawn.def.defName;
            if (defName == "SC_Mech_ResearchUnit" && __instance.def == SkillDefOf.Intellectual)
            {
                __result = 6;
            }
            else if (defName == "SC_Mech_MiningDrone" && __instance.def == SkillDefOf.Mining)
            {
                __result = 8;
            }
            else if (defName == "SC_Mech_CookUnit" && __instance.def == SkillDefOf.Cooking)
            {
                __result = 6;
            }
            else if (defName == "SC_Mech_WardenUnit" && __instance.def == SkillDefOf.Social)
            {
                __result = 4;
            }
            else if (defName == "SC_Mech_MedicUnit" && __instance.def == SkillDefOf.Medicine)
            {
                __result = 8;
            }
            else if (defName == "SC_Mech_FabricatorUnit" && __instance.def == SkillDefOf.Crafting)
            {
                __result = 7;
            }
            else if (defName == "SC_Mech_AnimalHandler" && __instance.def == SkillDefOf.Animals)
            {
                __result = 5;
            }
        }
    }

    // 4. Set Taming/Training Chance for Animal Handler Mech
    [HarmonyPatch(typeof(StatWorker), "GetValue", new Type[] { typeof(StatRequest), typeof(bool) })]
    public static class Patch_StatWorker_GetValue
    {
        private static readonly AccessTools.FieldRef<StatWorker, StatDef> statFieldRef =
            AccessTools.FieldRefAccess<StatWorker, StatDef>("stat");

        public static void Postfix(StatWorker __instance, StatRequest req, ref float __result)
        {
            StatDef stat = statFieldRef(__instance);
            if (stat == StatDefOf.TameAnimalChance || stat == StatDefOf.TrainAnimalChance)
            {
                if (req.HasThing && req.Thing is Pawn pawn && pawn.def.defName == "SC_Mech_AnimalHandler")
                {
                    if (stat == StatDefOf.TameAnimalChance)
                    {
                        __result = 0.15f; // Flat base taming chance (equivalent to mid-skill human)
                    }
                    else
                    {
                        __result = 0.40f; // Flat base training chance
                    }
                }
            }
        }
    }

    // 5. Expand Mechanitor Control Range Map-wide when Relay/Central Hub Active
    [HarmonyPatch(typeof(MechanitorUtility), "InMechanitorCommandRange")]
    public static class Patch_MechanitorUtility_InMechanitorCommandRange
    {
        public static void Postfix(Pawn mech, LocalTargetInfo target, ref bool __result)
        {
            if (__result) return;

            Map map = mech.Map;
            if (map != null)
            {
                var buildings = map.listerBuildings.allBuildingsColonist;
                for (int i = 0; i < buildings.Count; i++)
                {
                    var b = buildings[i];
                    if (b.def.defName == "SC_MechControlRelay" || b.def.defName == "SC_MechCentralControl")
                    {
                        var powerComp = b.GetComp<CompPowerTrader>();
                        if (powerComp == null || powerComp.PowerOn)
                        {
                            __result = true;
                            return;
                        }
                    }
                }
            }
        }
    }

    // 6. Suppress Command Radius Circle when Control Relay / Central Hub Active
    [HarmonyPatch(typeof(Pawn_MechanitorTracker), "DrawCommandRadius")]
    public static class Patch_Pawn_MechanitorTracker_DrawCommandRadius
    {
        public static bool Prefix(Pawn_MechanitorTracker __instance)
        {
            Pawn pawn = __instance.Pawn;
            Map map = pawn?.MapHeld;
            if (map != null)
            {
                var buildings = map.listerBuildings.allBuildingsColonist;
                for (int i = 0; i < buildings.Count; i++)
                {
                    var b = buildings[i];
                    if (b.def.defName == "SC_MechControlRelay" || b.def.defName == "SC_MechCentralControl")
                    {
                        var powerComp = b.GetComp<CompPowerTrader>();
                        if (powerComp == null || powerComp.PowerOn)
                        {
                            return false; // Skip drawing command radius
                        }
                    }
                }
            }
            return true;
        }
    }

    // 7. Prevent Feral State for Colony Mechs when Central Control Hub Active
    [HarmonyPatch(typeof(CompOverseerSubject), "get_State")]
    public static class Patch_CompOverseerSubject_get_State
    {
        public static bool Prefix(CompOverseerSubject __instance, ref OverseerSubjectState __result)
        {
            if (__instance.parent.Faction == Faction.OfPlayer && CentralControlUtility.IsCentralControlActive(__instance.parent.MapHeld))
            {
                __result = OverseerSubjectState.Overseen;
                return false; // Bypass original state logic
            }
            return true;
        }
    }

    // 8. Prevent Connection Loss / Feral Tick when Central Control Hub Active
    [HarmonyPatch(typeof(CompOverseerSubject), "CompTick")]
    public static class Patch_CompOverseerSubject_CompTick
    {
        public static bool Prefix(CompOverseerSubject __instance)
        {
            if (__instance.parent.Faction == Faction.OfPlayer && CentralControlUtility.IsCentralControlActive(__instance.parent.MapHeld))
            {
                return false; // Skip original tick behavior to prevent disconnects
            }
            return true;
        }
    }

    // 9. Force Mechs to Work Mode when Central Control Hub Active
    [HarmonyPatch(typeof(MechanitorUtility), "GetMechWorkMode")]
    public static class Patch_MechanitorUtility_GetMechWorkMode
    {
        public static void Postfix(Pawn pawn, ref MechWorkModeDef __result)
        {
            if (pawn.Faction == Faction.OfPlayer && CentralControlUtility.IsCentralControlActive(pawn.MapHeld))
            {
                __result = MechWorkModeDefOf.Work;
            }
        }
    }

    // 10. Central Control Hub Bandwidth Bonus
    [HarmonyPatch(typeof(Pawn_MechanitorTracker), "get_TotalBandwidth")]
    public static class Patch_Pawn_MechanitorTracker_TotalBandwidth
    {
        public static void Postfix(Pawn_MechanitorTracker __instance, ref int __result)
        {
            Pawn pawn = __instance.Pawn;
            if (pawn != null && pawn.MapHeld != null)
            {
                var buildings = pawn.MapHeld.listerBuildings.allBuildingsColonist;
                for (int i = 0; i < buildings.Count; i++)
                {
                    var b = buildings[i];
                    if (b.def.defName == "SC_MechCentralControl")
                    {
                        var cc = b.GetComp<Comp_CentralControl>();
                        if (cc != null && cc.Active)
                        {
                            __result += 15;
                        }
                    }
                }
            }
        }
    }

    // 11. Custom Advanced Band Node Bandwidth Contribution (+5 instead of +1)
    [HarmonyPatch(typeof(Hediff_BandNode), "get_AdditionalBandwidth")]
    public static class Patch_Hediff_BandNode_get_AdditionalBandwidth
    {
        private static readonly MethodInfo compBandNodeStateGetter =
            AccessTools.PropertyGetter(typeof(CompBandNode), "State");

        public static void Postfix(Hediff_BandNode __instance, ref int __result)
        {
            Pawn mechanitor = __instance.pawn;
            if (mechanitor == null) return;

            Map map = mechanitor.MapHeld;
            if (map == null) return;

            int totalBandwidth = 0;
            var buildings = map.listerBuildings.allBuildingsColonist;
            for (int i = 0; i < buildings.Count; i++)
            {
                var b = buildings[i];
                var bandNode = b.GetComp<CompBandNode>();
                if (bandNode != null && bandNode.tunedTo == mechanitor)
                {
                    BandNodeState state = (BandNodeState)compBandNodeStateGetter.Invoke(bandNode, null);
                    if (state == BandNodeState.Tuned)
                    {
                        var powerComp = b.GetComp<CompPowerTrader>();
                        if (powerComp == null || powerComp.PowerOn)
                        {
                            if (b.def.defName == "SC_AdvancedBandNode")
                            {
                                totalBandwidth += 5;
                            }
                            else if (b.def.defName == "SC_MechControlRelay")
                            {
                                totalBandwidth += 10;
                            }
                            else
                            {
                                totalBandwidth += 1; // Standard vanilla band node
                            }
                        }
                    }
                }
            }
            __result = totalBandwidth;
        }
    }
}
