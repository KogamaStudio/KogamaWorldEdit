using Il2Cpp;
using HarmonyLib;

namespace KogamaWorldEdit.AntiAntiCheat;

[HarmonyPatch]
internal class ThemeCrashFix
{
    [HarmonyPatch(typeof(Theme), "Initialize", typeof(int))]
    [HarmonyPrefix]
    private static bool BlockThemeInitialize(int woid) => false;
}

