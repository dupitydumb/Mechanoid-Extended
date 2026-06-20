using System;
using System.Collections.Generic;
using Verse;
using RimWorld;

namespace SteelColony
{
    public class MapComponent_LogisticsManager : MapComponent
    {
        public List<LeaseContract> availableContracts = new List<LeaseContract>();
        public List<LeaseContract> activeContracts = new List<LeaseContract>();

        private int nextContractTick = 30000; // Generate first contract after 12 hours (30000 ticks)

        public MapComponent_LogisticsManager(Map map) : base(map)
        {
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            // 1. Tick down active contracts
            for (int i = activeContracts.Count - 1; i >= 0; i--)
            {
                var contract = activeContracts[i];
                contract.ticksLeft--;

                if (contract.ticksLeft <= 0)
                {
                    CompleteContract(contract);
                    activeContracts.RemoveAt(i);
                }
            }

            // 2. Periodically generate available contracts (every 2-4 days)
            if (Find.TickManager.TicksGame >= nextContractTick)
            {
                nextContractTick = Find.TickManager.TicksGame + Rand.Range(120000, 240000); // 2 to 4 days

                if (availableContracts.Count < 3)
                {
                    var newContract = LogisticsUtility.GenerateLeaseContract();
                    availableContracts.Add(newContract);
                    Find.LetterStack.ReceiveLetter(
                        "New Lease Contract", 
                        $"A new contract has been offered to your Mechnet Logistics Terminal:\n\n**{newContract.title}**\n\n{newContract.description}\n\nRequired: {string.Join(", ", newContract.requiredMechDefs)}\nDuration: {newContract.durationTicks / 60000} days\nReward Value: {newContract.rewardValue} silver equivalent", 
                        LetterDefOf.PositiveEvent
                    );
                }
            }
        }

        private void CompleteContract(LeaseContract contract)
        {
            contract.completed = true;

            // Find a powered logistics terminal on the map to release the mechs
            var buildings = map.listerBuildings.allBuildingsColonist;
            Building_LogisticsTerminal terminal = null;
            for (int i = 0; i < buildings.Count; i++)
            {
                if (buildings[i] is Building_LogisticsTerminal blt)
                {
                    var power = blt.GetComp<CompPowerTrader>();
                    if (power == null || power.PowerOn)
                    {
                        terminal = blt;
                        break;
                    }
                }
            }

            if (terminal != null)
            {
                terminal.ReturnLeasedMechs(contract);
            }
            else
            {
                // Fallback: If no powered terminal was found, rewards and mechs are lost in transit.
                Find.LetterStack.ReceiveLetter(
                    "Lease Contract Lost", 
                    $"The lease contract '{contract.title}' completed, but your Mechnet Logistics Terminal was either powered down or destroyed. The leased mechanoids and contract rewards were lost in transit.", 
                    LetterDefOf.NegativeEvent
                );
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref availableContracts, "availableContracts", LookMode.Deep);
            Scribe_Collections.Look(ref activeContracts, "activeContracts", LookMode.Deep);
            Scribe_Values.Look(ref nextContractTick, "nextContractTick", 30000);
        }
    }
}
