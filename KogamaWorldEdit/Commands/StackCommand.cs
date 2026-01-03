using Il2Cpp;
using KogamaModFramework.Commands;
using KogamaWorldEdit.Generate;
using MelonLoader;

namespace KogamaWorldEdit.Commands;

public class StackCommand : Command
{
    public override string Name => "/stack";

    public override void Execute(string[] args)
    {
        if (args.Length < 1)
        {
            MelonLogger.Msg("Usage: /stack <count> [direction]");
            MelonLogger.Msg("Directions: up, down, left, right, forward, back");
            return;
        }

        try
        {
            int count = int.Parse(args[0]);

            if (count <= 0)
            {
                MelonLogger.Error("Error: Count must be greater than 0!");
                return;
            }

            string direction;

            if (args.Length < 2)
            {
                direction = DetectDirectionFromCamera();
                if (direction == null)
                {
                    MelonLogger.Error("Error: Could not detect direction from camera!");
                    return;
                }
                MelonLogger.Msg($"Direction auto-detected: {direction}");
            }
            else
            {
                direction = args[1].ToLower();
            }

            if (direction != "up" && direction != "down" && direction != "left" && direction != "right" && direction != "forward" && direction != "back")
            {
                MelonLogger.Error("Error: Invalid direction! Use: up, down, left, right, forward, back");
                return;
            }

            AreaEditorTool.StackCount = count;
            AreaEditorTool.StackDirection = direction;
            KogamaWorldEdit.AreaEditor.PerformAction(AreaEditorTool.ActionType.Stack);
        }
        catch
        {
            MelonLogger.Error("Error: Invalid arguments!");
        }
    }

    private string DetectDirectionFromCamera()
    {
        var mainCamera = MVGameControllerBase.MainCameraManager;
        if (mainCamera?.ProtectedTransform == null) return null;

        var euler = mainCamera.ProtectedTransform.rotation.eulerAngles;
        float yaw = euler.y;
        float pitch = euler.x;

        if (pitch > 180f) pitch -= 360f;

        if (pitch > 45f)
            return "down";
        if (pitch < -45f)
            return "up";

        yaw = yaw % 360f;
        if (yaw < 45f || yaw >= 315f)
            return "forward";
        if (yaw >= 45f && yaw < 135f)
            return "right";
        if (yaw >= 135f && yaw < 225f)
            return "back";
        if (yaw >= 225f && yaw < 315f)
            return "left";

        return "forward";
    }
}