using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Il2Cpp;

namespace KogamaWorldEdit.Commands;

[HarmonyPatch]
internal class CommandManager
{
    private static Dictionary<string, Action<string[]>> Commands = new();

    public static void Register(string cmd, Action<string[]> handler)
    {
        Commands[cmd] = handler;
    }

    public static bool Execute(string cmd, string[] args)
    {
        if (Commands.TryGetValue(cmd.ToLower(), out var handler))
        {
            handler?.Invoke(args);
            return true;
        }
        return false;
    }

    [HarmonyPatch(typeof(SendMessageControl), "HandleChatCommands")]
    [HarmonyPrefix]
    internal static bool HandleChatPrefix(string chatMsg)
    {
        if (chatMsg.StartsWith("//"))
        {
            string[] parts = chatMsg.Substring(2).Split(' ');
            string cmd = parts[0];
            string[] args = parts.Skip(1).ToArray();

            if (!Execute(cmd, args)) return true;

            return false;
        }

        return true;
    }
}

