using Verse;
using HarmonyLib;

namespace SteelColony
{
    public class ModMain : Mod
    {
        public ModMain(ModContentPack content) : base(content)
        {
            Log.Message("[Steel Colony] Initializing assembly patches...");
            var harmony = new Harmony("yourname.steelcolony");
            harmony.PatchAll();
            Log.Message("[Steel Colony] Assembly patches applied successfully!");
        }
    }
}
