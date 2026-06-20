using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.Sound;
using RimWorld;
using UnityEngine;

namespace SteelColony
{
    public class Building_LogisticsTerminal : Building_Storage, IThingHolder
    {
        private ThingOwner<Pawn> leasedMechs;

        public Building_LogisticsTerminal()
        {
            leasedMechs = new ThingOwner<Pawn>(this);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref leasedMechs, "leasedMechs", this);
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return leasedMechs;
        }

        public void GetChildHolders(List<IThingHolder> outHolders)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outHolders, GetDirectlyHeldThings());
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            // Drop any leased mechs onto the ground if the terminal is destroyed
            if (leasedMechs.Count > 0)
            {
                leasedMechs.TryDropAll(this.Position, this.Map, ThingPlaceMode.Near);
            }
            base.Destroy(mode);
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.GetInspectString());
            if (leasedMechs.Count > 0)
            {
                sb.AppendLine();
                sb.Append($"Leased Mechanoids: {leasedMechs.Count} units currently off-map");
            }
            return sb.ToString().TrimEnd();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }

            // Must be powered to operate
            var power = this.GetComp<CompPowerTrader>();
            if (power != null && !power.PowerOn)
            {
                yield break;
            }

            // 1. Launch Stored Cargo
            yield return new Command_Action
            {
                defaultLabel = "Launch Cargo",
                defaultDesc = "Launch all stored tradeable items in this terminal directly into orbit for immediate logical trade sale.",
                icon = ContentFinder<Texture2D>.Get("UI/Commands/LaunchShip"),
                action = () => LaunchCargo()
            };

            // 2. Manage Leases
            yield return new Command_Action
            {
                defaultLabel = "Manage Leases",
                defaultDesc = "View and accept available mechanoid labor leasing contracts from other settlements.",
                icon = ContentFinder<Texture2D>.Get("UI/Commands/Draft"),
                action = () => OpenLeaseMenu()
            };
        }

        private void LaunchCargo()
        {
            List<Thing> thingsToExport = new List<Thing>();
            float totalValue = 0f;

            foreach (IntVec3 cell in this.OccupiedRect())
            {
                List<Thing> cellThings = this.Map.thingGrid.ThingsListAt(cell);
                for (int i = 0; i < cellThings.Count; i++)
                {
                    Thing t = cellThings[i];
                    // Verify it's a tradeable resource, not this building
                    if (t != this && t.def.category == ThingCategory.Item && t.def.defName != "SC_LogisticsTerminal")
                    {
                        thingsToExport.Add(t);
                        totalValue += t.MarketValue * t.stackCount;
                    }
                }
            }

            if (thingsToExport.Count == 0)
            {
                Messages.Message("No exportable trade items found in the logistics terminal's storage cells.", this, MessageTypeDefOf.RejectInput);
                return;
            }

            float priceFactor = LogisticsUtility.CalculateTradePriceFactor(this.Map);
            int payout = Mathf.RoundToInt(totalValue * priceFactor);

            // Despawn and destroy launched items
            foreach (Thing t in thingsToExport)
            {
                t.Destroy(DestroyMode.Vanish);
            }

            // Visual and audio effects
            SoundDef.Named("DropPod_Landed").PlayOneShot(new TargetInfo(this.Position, this.Map));
            FleckMaker.ThrowSmoke(this.Position.ToVector3Shifted(), this.Map, 2.5f);

            // Spawn rewards
            LogisticsUtility.SendDropPodRewards(this.Map, payout, this.Position);

            Messages.Message($"Cargo launched successfully! Stored value: {totalValue} silver. Mechnet pricing coefficient: {priceFactor:P0}. Payout: {payout} silver equivalent dropped.", this, MessageTypeDefOf.PositiveEvent);
        }

        private void OpenLeaseMenu()
        {
            var manager = this.Map.GetComponent<MapComponent_LogisticsManager>();
            if (manager == null) return;

            List<FloatMenuOption> options = new List<FloatMenuOption>();

            if (manager.availableContracts.Count == 0)
            {
                options.Add(new FloatMenuOption("No lease contracts currently available", null));
            }
            else
            {
                foreach (LeaseContract contract in manager.availableContracts)
                {
                    string optionText = $"{contract.title} ({contract.rewardValue} Silver) - Needs: {string.Join(", ", contract.requiredMechDefs)}";
                    options.Add(new FloatMenuOption(optionText, () => TryAcceptLease(contract, manager)));
                }
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        private void TryAcceptLease(LeaseContract contract, MapComponent_LogisticsManager manager)
        {
            // Verify if we have the required mechs
            List<Pawn> selectedMechs = new List<Pawn>();
            List<string> missingMechs = new List<string>();

            foreach (string reqDefName in contract.requiredMechDefs)
            {
                Pawn match = this.Map.mapPawns.AllPawnsSpawned.FirstOrDefault(p => 
                    p.Faction == Faction.OfPlayer && 
                    p.def.defName == reqDefName && 
                    !p.Drafted && 
                    !p.Downed && 
                    !selectedMechs.Contains(p));

                if (match != null)
                {
                    selectedMechs.Add(match);
                }
                else
                {
                    missingMechs.Add(reqDefName);
                }
            }

            if (missingMechs.Count > 0)
            {
                Messages.Message($"Missing required idle mechanoids for this lease. Missing: {string.Join(", ", missingMechs)}", this, MessageTypeDefOf.RejectInput);
                return;
            }

            // Accept and move mechs into terminal container
            contract.accepted = true;
            manager.availableContracts.Remove(contract);
            manager.activeContracts.Add(contract);

            foreach (Pawn mech in selectedMechs)
            {
                mech.jobs?.StopAll();
                mech.DeSpawn();
                this.leasedMechs.TryAdd(mech);
            }

            SoundDef.Named("DropPod_Landed").PlayOneShot(new TargetInfo(this.Position, this.Map));
            FleckMaker.ThrowSmoke(this.Position.ToVector3Shifted(), this.Map, 2.0f);

            Find.LetterStack.ReceiveLetter(
                "Lease Contract Started", 
                $"Your mechanoids ({string.Join(", ", selectedMechs.Select(m => m.LabelShort))}) have been loaded into the Mechnet Logistics Terminal and sent off-map for the lease contract:\n\n**{contract.title}**\n\nThey will return in {contract.durationTicks / 60000} days.", 
                LetterDefOf.PositiveEvent, 
                this
            );
        }

        public void ReturnLeasedMechs(LeaseContract contract)
        {
            List<Pawn> mechsToRelease = new List<Pawn>();

            foreach (string reqDefName in contract.requiredMechDefs)
            {
                Pawn mech = leasedMechs.InnerListForReading.FirstOrDefault(p => p.def.defName == reqDefName);
                if (mech != null)
                {
                    mechsToRelease.Add(mech);
                    leasedMechs.Remove(mech);
                }
            }

            // Spawn returned mechs near terminal
            foreach (Pawn mech in mechsToRelease)
            {
                GenSpawn.Spawn(mech, this.Position, this.Map);
            }

            // Drop rewards
            LogisticsUtility.SendDropPodRewards(this.Map, contract.rewardValue, this.Position);

            Find.LetterStack.ReceiveLetter(
                "Lease Contract Completed", 
                $"The contract '{contract.title}' has finished! Your mechanoids have been returned, and rewards worth {contract.rewardValue} silver have been dropshipped to your logistics terminal.", 
                LetterDefOf.PositiveEvent, 
                this
            );
        }
    }
}
