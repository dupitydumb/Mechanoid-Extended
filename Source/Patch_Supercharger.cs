using HarmonyLib;
using RimWorld;
using Verse;

namespace SteelColony
{
    [HarmonyPatch(typeof(Building_MechCharger), "Tick")]
    public static class Patch_Building_MechCharger_Tick
    {
        private static readonly AccessTools.FieldRef<Building_MechCharger, Pawn> currentlyChargingMechRef = 
            AccessTools.FieldRefAccess<Building_MechCharger, Pawn>("currentlyChargingMech");

        public static void Postfix(Building_MechCharger __instance)
        {
            if (__instance.def.defName == "SC_SuperchargerStation")
            {
                Pawn mech = currentlyChargingMechRef(__instance);
                if (mech != null)
                {
                    var energyNeed = mech.needs?.TryGetNeed<Need_MechEnergy>();
                    if (energyNeed != null)
                    {
                        // Adding charge again to double the speed
                        energyNeed.CurLevel += Building_MechCharger.ChargePerDay / 60000f;
                    }
                }
            }
        }
    }
}
