using System.Linq;
using Verse;
using RimWorld;
using HarmonyLib;

namespace SteelColony
{
    public class CompProperties_AutoRepairGantry : CompProperties
    {
        public int healAmount = 2; // HP healed per TickRare (4 seconds)
        public CompProperties_AutoRepairGantry()
        {
            this.compClass = typeof(Comp_AutoRepairGantry);
        }
    }

    public class Comp_AutoRepairGantry : ThingComp
    {
        public CompProperties_AutoRepairGantry Props => (CompProperties_AutoRepairGantry)this.props;

        private CompPowerTrader powerComp;
        private static readonly AccessTools.FieldRef<Building_MechCharger, Pawn> currentlyChargingMechRef = 
            AccessTools.FieldRefAccess<Building_MechCharger, Pawn>("currentlyChargingMech");

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            powerComp = this.parent.GetComp<CompPowerTrader>();
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            if (powerComp == null || !powerComp.PowerOn) return;

            Building_MechCharger charger = this.parent as Building_MechCharger;
            if (charger == null) return;

            Pawn mech = currentlyChargingMechRef(charger);
            if (mech == null) return;

            if (mech.health.hediffSet.hediffs.OfType<Hediff_Injury>().Any())
            {
                // Find an injury that can be healed
                Hediff_Injury injury = mech.health.hediffSet.hediffs
                    .OfType<Hediff_Injury>()
                    .FirstOrDefault(i => i.CanHealNaturally());

                if (injury != null)
                {
                    injury.Heal(Props.healAmount);
                    // Throw visual micro sparks to show active repairing
                    FleckMaker.ThrowMicroSparks(mech.DrawPos, mech.Map);
                }
            }
        }
    }
}
