using Il2Cpp;
using KogamaModFramework.Commands;
using KogamaWorldEdit.Common;
using KogamaWorldEdit.Generate;
using MelonLoader;

namespace KogamaWorldEdit.Commands;

public class ReplaceCommand : Command
{
    public override string Name => "/replace";

    public override void Execute(string[] args)
    {
        if (!int.TryParse(args[0], out _) && Materials.GetMaterialId(args[0]) != -999)
            args[0] = Materials.GetMaterialId(args[0]).ToString();

        if (args.Length >= 2 && !int.TryParse(args[1], out _) && Materials.GetMaterialId(args[1]) != -999)
            args[1] = Materials.GetMaterialId(args[1]).ToString();

        try
        {
            if (args.Length == 1)
            {
                if (args[0] == "-1")
                {
                    KogamaWorldEdit.AreaEditor.PerformAction(AreaEditorTool.ActionType.Clear);
                }
                else
                {
                    AreaEditorTool.oldMaterialId = -2;
                    AreaEditorTool.newMaterialId = int.Parse(args[0]);
                    KogamaWorldEdit.AreaEditor.PerformAction(AreaEditorTool.ActionType.Replace);
                }
            }
            else if (args.Length >= 2)
            {
                AreaEditorTool.oldMaterialId = int.Parse(args[0]);
                AreaEditorTool.newMaterialId = int.Parse(args[1]);
                KogamaWorldEdit.AreaEditor.PerformAction(AreaEditorTool.ActionType.Replace);
            }
            else
            {
                TextCommand.NotifyUser("Usage: /replace <material_id> or /replace <old_id> <new_id>");
            }
        }
        catch
        {
            TextCommand.NotifyUser("Error: Invalid material IDs!");
        }
    }
}