using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace SteelColony
{
    public class CompProperties_SentinelPost : CompProperties
    {
        public CompProperties_SentinelPost()
        {
            this.compClass = typeof(Comp_SentinelPost);
        }
    }

    public class Comp_SentinelPost : ThingComp
    {
        public IntVec3 postPosition = IntVec3.Invalid;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref postPosition, "postPosition", IntVec3.Invalid);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var g in base.CompGetGizmosExtra())
            {
                yield return g;
            }

            if (parent.Faction == Faction.OfPlayer)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Set Hold Post",
                    defaultDesc = "Designate this sentinel's current tile as its permanent guard post. It will return here when idle and defend this area.",
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/ForceShieldOnOff", false) ?? BaseContent.BadTex,
                    action = delegate
                    {
                        postPosition = parent.Position;
                        Messages.Message("Guard post set at " + postPosition.ToString(), parent, MessageTypeDefOf.TaskCompletion, false);
                    }
                };

                if (postPosition.IsValid)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "Clear Hold Post",
                        defaultDesc = "Clear the guard post. The sentinel will behave normally.",
                        icon = ContentFinder<Texture2D>.Get("UI/Commands/Halt", false) ?? BaseContent.BadTex,
                        action = delegate
                        {
                            postPosition = IntVec3.Invalid;
                            Messages.Message("Guard post cleared.", parent, MessageTypeDefOf.TaskCompletion, false);
                        }
                    };
                }
            }
        }
    }
}
