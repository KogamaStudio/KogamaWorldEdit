using System;
using System.Collections.Generic;
using System.ComponentModel;
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

    [HarmonyPatch(typeof(SendMessageControl), "HandleChatCommands")]
    [HarmonyPrefix]
    internal static bool HandleChatPrefix(string chatMsg)
    {
        if (!chatMsg.StartsWith("//")) return true;

        string text = chatMsg.Substring(2);
        string[] parts = text.Split(' ');
        string cmd = parts[0].ToLower();
        string[] args = new string[parts.Length - 1];
        Array.Copy(parts, 1, args, 0, parts.Length - 1);

        if (Commands.TryGetValue(cmd, out var handler))
        {
            handler(args);
            return false;
        }

        return true;
    }
}

