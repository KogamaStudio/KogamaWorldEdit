using KogamaModFramework.Commands;
using KogamaWorldEdit.Generate;
using MelonLoader;
using KogamaWorldEdit;
using KogamaWorldEdit.Common;
using Il2Cpp;

namespace KogamaWorldEdit.Commands;

public class TestCommand : Command
{
    public override string Name => "/test";

    public override void Execute(string[] args)
    {
        var mainCamera = MVGameControllerBase.MainCameraManager;
        if (mainCamera?.ProtectedTransform != null)
        {
            var rotation = mainCamera.ProtectedTransform.rotation;
            TextCommand.NotifyUser($"{rotation}");
        }
    }
}

