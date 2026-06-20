using System;
using System.Collections.Generic;
using Verse;

namespace SteelColony
{
    public class LeaseContract : IExposable
    {
        public string id;
        public string title;
        public string description;
        public int durationTicks;
        public int ticksLeft;
        public List<string> requiredMechDefs = new List<string>();
        public int rewardValue;
        public bool accepted;
        public bool completed;

        // Default constructor for serialization
        public LeaseContract()
        {
        }

        public LeaseContract(string id, string title, string description, int durationTicks, List<string> requiredMechDefs, int rewardValue)
        {
            this.id = id;
            this.title = title;
            this.description = description;
            this.durationTicks = durationTicks;
            this.ticksLeft = durationTicks;
            this.requiredMechDefs = requiredMechDefs;
            this.rewardValue = rewardValue;
            this.accepted = false;
            this.completed = false;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref id, "id");
            Scribe_Values.Look(ref title, "title");
            Scribe_Values.Look(ref description, "description");
            Scribe_Values.Look(ref durationTicks, "durationTicks");
            Scribe_Values.Look(ref ticksLeft, "ticksLeft");
            Scribe_Collections.Look(ref requiredMechDefs, "requiredMechDefs", LookMode.Value);
            Scribe_Values.Look(ref rewardValue, "rewardValue");
            Scribe_Values.Look(ref accepted, "accepted");
            Scribe_Values.Look(ref completed, "completed");
        }
    }
}
