using HarmonyLib;
using Il2Cpp;
using Il2CppSystem.Reflection;
using MelonLoader;
using MV.Common;

namespace KogamaWorldEdit.AntiAntiCheat;

[HarmonyPatch]
internal static class AntiBan
{
    private static bool DebugMode = false;

    // tysm beckowl

    [HarmonyPatch(typeof(CheatHandling), "Init")]
    [HarmonyPatch(typeof(CheatHandling), "ExecuteBan")]
    [HarmonyPatch(typeof(CheatHandling), "CheatSoftwareRunningDetected")]
    [HarmonyPatch(typeof(CheatHandling), "TextureHackDetected")]
    [HarmonyPatch(typeof(CheatHandling), "MachineBanDetected")]
    [HarmonyPatch(typeof(CheatHandling), "ObscuredCheatingDetected")]
    [HarmonyPatch(typeof(MVNetworkGame.OperationRequests), "Ban", typeof(int), typeof(MVPlayer), typeof(string))]
    //[HarmonyPatch(typeof(MVNetworkGame.OperationRequests), "Ban", typeof(MV.Common.CheatType))]
    [HarmonyPatch(typeof(MVNetworkGame.OperationRequests), "Expel")]
    [HarmonyPatch(typeof(MVNetworkGame.OperationRequests), "Kick")]
    [HarmonyPrefix]
    private static bool BlockAll(MethodBase __originalMethod)
    {
        if (DebugMode)
        {
            TextCommand.NotifyUser($"[AnitBan] Blocked {__originalMethod.Name}");
            MelonLogger.Msg($"[AnitBan] Blocked {__originalMethod.Name}");
        }
        return false;
    }

}