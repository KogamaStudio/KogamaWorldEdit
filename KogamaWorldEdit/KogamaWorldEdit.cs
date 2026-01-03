using MelonLoader;
using Il2Cpp;
using KogamaWorldEdit.Common;
using UnityEngine;
using Il2CppInterop.Runtime;
using KogamaWorldEdit.Commands;
using KogamaWorldEdit.Generate;
using KogamaModFramework.Commands;

[assembly: MelonInfo(typeof(KogamaWorldEdit.KogamaWorldEdit), "KogamaWorldEdit", "0.1.0-dev", "Amuarte & Veni")]
[assembly: MelonGame("Multiverse ApS", "KoGaMa")]

namespace KogamaWorldEdit;

public class KogamaWorldEdit : MelonMod
{
    public static bool Initialized = false;
    public static AreaEditorTool AreaEditor;

    public override void OnInitializeMelon()
    {
        AreaEditor = new AreaEditorTool();

        CommandManager.Initialize();
        CommandManager.Register(new SetCommand());
        CommandManager.Register(new ReplaceCommand());
        CommandManager.Register(new StackCommand());
        CommandManager.Register(new TestCommand());
        //CommandManager.Register("clear", (args) => areaEditor.PerformAction(AreaEditorTool.ActionType.Clear));
        //CommandManager.Register("replace", (args) => areaEditor.PerformAction(AreaEditorTool.ActionType.Replace));
        //CommandManager.Register("mat", (args) => areaEditor.selectedMaterialId = int.Parse(args[0]));
        //CommandManager.Register("reset", (args) => areaEditor.ResetTool());
        //CommandManager.Register("status", (args) =>
        //{
        //    TextCommand.NotifyUser($"Status: {areaEditor.CurrentState}");
        //    TextCommand.NotifyUser($"Material ID: {areaEditor.selectedMaterialId}");
        //});
        //CommandManager.Register("info", (args) =>
        //{
        //    if (areaEditor.CurrentState == AreaEditorTool.ToolState.AreaSelected)
        //    {
        //        int dx = Mathf.Abs(areaEditor.startPos.x - areaEditor.endPos.x) + 1;
        //        int dy = Mathf.Abs(areaEditor.startPos.y - areaEditor.endPos.y) + 1;
        //        int dz = Mathf.Abs(areaEditor.startPos.z - areaEditor.endPos.z) + 1;
        //        TextCommand.NotifyUser($"Wymiary: {dx}x{dy}x{dz} (Kostek: {dx * dy * dz})");
        //    }
        //    else
        //    {
        //        TextCommand.NotifyUser("Brak zaznaczonego obszaru");
        //    }
        //});
        //CommandManager.Register("help", (args) =>
        //{
        //    TextCommand.NotifyUser("<b>KogamaWorldEdit Commands:</b>");
        //    TextCommand.NotifyUser("//set - Fill area with material");
        //    TextCommand.NotifyUser("//clear - Clear area");
        //    TextCommand.NotifyUser("//replace - Replace material");
        //    TextCommand.NotifyUser("//mat <id> - Set material ID (0-68)");
        //    TextCommand.NotifyUser("//status - Show current status");
        //    TextCommand.NotifyUser("//info - Show area dimensions");
        //    TextCommand.NotifyUser("//reset - Reset tool");
        //});
    }

    public override void OnUpdate()
    {
        if (MVGameControllerBase.IsInitialized && !Initialized)
        {
            Initialized = true;
            TextCommand.NotifyUser("<b>KogamWorldEdit</b> v0.1.0 loaded!");
            TextCommand.NotifyUser("Type //help for available commands");

        }

        AreaEditor?.Update();
    }

}
