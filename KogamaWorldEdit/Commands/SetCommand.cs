using KogamaModFramework.Commands;
using KogamaWorldEdit.Generate;
using MelonLoader;
using KogamaWorldEdit;
using KogamaWorldEdit.Common;
using Il2Cpp;

namespace KogamaWorldEdit.Commands;

public class SetCommand : Command
{
    public override string Name => "/set";

    public override void Execute(string[] args)
    {
        if (args.Length < 1)
        {
            TextCommand.NotifyUser("Usage: /set <material_id_or_name>");
            return;
        }

        try
        {
            int materialId = Materials.GetMaterialId(args[0]);

            if (materialId == -999)
            {
                TextCommand.NotifyUser($"Error: Unknown material '{args[0]}'!");
                return;
            }

            if (materialId == -1)
            {
                KogamaWorldEdit.AreaEditor.PerformAction(AreaEditorTool.ActionType.Clear);
            }
            else
            {
                AreaEditorTool.newMaterialId = materialId;
                KogamaWorldEdit.AreaEditor.PerformAction(AreaEditorTool.ActionType.Set);
            }
        }
        catch
        {
            TextCommand.NotifyUser("Error: Invalid material!");
        }
    }
}

    