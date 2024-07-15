using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace PierceBugFix;

[BepInPlugin("PierceBugFix", "PierceBugFix", "1.0.0")]
public class Plugin : BasePlugin
{
    public override void Load()
    {
        // Plugin startup logic
        _ = Harmony.CreateAndPatchAll(typeof(Patch), "tru0067.PierceBugFix");
        Log.LogInfo("PierceBugFix is loaded!");
    }
}
