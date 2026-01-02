using Il2Cpp;
using HarmonyLib;

namespace KogamaWorldEdit.AntiAntiCheat;

[HarmonyPatch]
internal class ThemeCrashFix
{
    [HarmonyPatch(typeof(ThemeSkybox), "Activate")]
    [HarmonyPrefix]
    private static bool ThemeActivatePrefix()
    {
        return false;
    }
}

