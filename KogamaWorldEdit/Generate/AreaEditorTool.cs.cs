using System;
using Il2Cpp;
using Il2CppMV.Common;
using Il2CppMV.WorldObject;
using UnityEngine;
using MelonLoader;
using KogamaModFramework.Generation;
using Harmony;
using System.Security.Principal;

// Made By Veni
// Improved by 

namespace KogamaWorldEdit.Generate
{
    public class AreaEditorTool
    {

        public static AreaEditorTool Instance { get; set; }

        public enum ToolState { Idle, SelectingFirst, SelectingSecond, AreaSelected }
        public ToolState CurrentState = ToolState.Idle;

        public IntVector startPos;
        public IntVector endPos;
        private MVCubeModelBase targetModel;

        private GameObject selectionBoxObj;
        private SelectionBox selectionBoxScript;

        public static int selectedMaterialId = 1;
        public static int oldMaterialId = 21;
        public static int newMaterialId = 21;

        public static int StackCount { get; set; }
        public static string StackDirection { get; set; }


        public void Update()
        {

            if (MVGameControllerBase.GameSessionData == null) return;
            if (MVGameControllerBase.GameMode != MVGameMode.Edit) return;
            if (MVGameControllerBase.EditModeUI == null) return;

            //  (DesktopEditModeController -> StateMachine)
            var editUI = MVGameControllerBase.EditModeUI.TryCast<DesktopEditModeController>();
            if (editUI == null) return;

            var stateMachine = editUI.EditModeStateMachine;
            if (stateMachine == null) return;

            var modelingMachine = stateMachine.CubeModelingStateMachine;
            if (modelingMachine == null) return;

            var pickInfo = modelingMachine.SelectedCube;

            if (pickInfo != null && pickInfo.cube != null)
            {
                MVCubeModelBase hoveredModel = modelingMachine.TargetCubeModel;
                IntVector hoveredPos = pickInfo.iLocalPos;

                byte hoveredMat = 1;
                if (pickInfo.cube.FaceMaterials != null && pickInfo.cube.FaceMaterials.Length > 0)
                {
                    hoveredMat = pickInfo.cube.FaceMaterials[(int)pickInfo.pickedFace];
                }

                if (Input.GetMouseButtonDown(1) && Input.GetKey(KeyCode.LeftControl))
                {
                    HandleClick(hoveredModel, hoveredPos, hoveredMat);
                }

                if (CurrentState == ToolState.SelectingSecond && hoveredModel == targetModel)
                {
                    UpdateVisuals(startPos, hoveredPos);
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ResetTool();
            }
        }

        private void HandleClick(MVCubeModelBase model, IntVector pos, byte materialID)
        {
            if (CurrentState == ToolState.Idle || CurrentState == ToolState.AreaSelected)
            {
                targetModel = model;
                startPos = pos;
                CurrentState = ToolState.SelectingSecond;

                selectedMaterialId = materialID;
                TextCommand.NotifyUser($"Position 1: {pos}");
                MelonLogger.Msg($"Start: {pos}. Pobranno materiał ID: {materialID}");

                CreateVisuals(model);
                UpdateVisuals(startPos, startPos);
            }
            else if (CurrentState == ToolState.SelectingSecond)
            {
                if (model == targetModel)
                {
                    endPos = pos;
                    CurrentState = ToolState.AreaSelected;
                    UpdateVisuals(startPos, endPos);
                    MelonLogger.Msg($"Koniec: {pos}. Zaznaczono obszar.");

                    int sizeX = Mathf.Abs(endPos.x - startPos.x) + 1;
                    int sizeY = Mathf.Abs(endPos.y - startPos.y) + 1;
                    int sizeZ = Mathf.Abs(endPos.z - startPos.z) + 1;
                    int volume = sizeX * sizeY * sizeZ;

                    TextCommand.NotifyUser($"Position 2: {pos}");
                    TextCommand.NotifyUser($"Area size: {sizeX}x{sizeY}x{sizeZ} = {volume} blocks");
                }
                else
                {
                    MelonLogger.Msg("Błąd: Musisz zaznaczyć kostki w tym samym obiekcie!");
                    ResetTool();
                }
            }
        }

        // --- (Fill/Clear) ---

        public enum ActionType { Set, Clear, Replace, Stack }

        public void PerformAction(ActionType action)
        {
            if (targetModel == null) return;

            int minX = Mathf.Min(startPos.x, endPos.x); int maxX = Mathf.Max(startPos.x, endPos.x);
            int minY = Mathf.Min(startPos.y, endPos.y); int maxY = Mathf.Max(startPos.y, endPos.y);
            int minZ = Mathf.Min(startPos.z, endPos.z); int maxZ = Mathf.Max(startPos.z, endPos.z);

            targetModel.MakeUnique();

            if (action == ActionType.Set)
            {
                var cubesToAdd = new List<CubeData>();
                for (int x = minX; x <= maxX; x++)
                {
                    for (int y = minY; y <= maxY; y++)
                    {
                        for (int z = minZ; z <= maxZ; z++)
                        {
                            var pos = new IntVector((short)x, (short)y, (short)z);
                            byte[] materials = Cube.CreateMaterialArray((byte)newMaterialId);
                            byte[] corners = CubeDataPacker.CornersToByteArray(CubeBase.IdentityCorners);
                            cubesToAdd.Add(new CubeData(x, y, z, materials, corners));
                        }
                    }
                }
                if (cubesToAdd.Count > 0)
                {
                    MelonCoroutines.Start(CubeOperations.Add(targetModel, cubesToAdd));
                }
            }
            else if (action == ActionType.Clear)
            {
                var positionsToRemove = new List<IntVector>();
                for (int x = minX; x <= maxX; x++)
                {
                    for (int y = minY; y <= maxY; y++)
                    {
                        for (int z = minZ; z <= maxZ; z++)
                        {
                            var pos = new IntVector((short)x, (short)y, (short)z);
                            if (targetModel.GetCube(pos) != null)
                            {
                                positionsToRemove.Add(pos);
                            }
                        }
                    }
                }
                if (positionsToRemove.Count > 0)
                {
                    MelonCoroutines.Start(CubeOperations.Remove(targetModel, positionsToRemove));
                }
            }
            else if (action == ActionType.Replace)
            {
                var cubesToReplace = new List<CubeData>();
                var positionsToRemove = new List<IntVector>();

                for (int x = minX; x <= maxX; x++)
                {
                    for (int y = minY; y <= maxY; y++)
                    {
                        for (int z = minZ; z <= maxZ; z++)
                        {
                            var pos = new IntVector((short)x, (short)y, (short)z);
                            var existingCube = targetModel.GetCube(pos);

                            if (newMaterialId == -1 && existingCube != null && existingCube.FaceMaterials[0] == (byte)oldMaterialId)
                            {
                                positionsToRemove.Add(pos);
                            }
                            else if (oldMaterialId == -1 && existingCube == null)
                            {
                                byte[] materials = Cube.CreateMaterialArray((byte)newMaterialId);
                                byte[] corners = CubeDataPacker.CornersToByteArray(CubeBase.IdentityCorners);
                                cubesToReplace.Add(new CubeData(x, y, z, materials, corners));
                            }
                            else if (oldMaterialId != -1 && newMaterialId != -1 && existingCube != null && existingCube.FaceMaterials[0] == (byte)oldMaterialId)
                            {
                                byte[] materials = Cube.CreateMaterialArray((byte)newMaterialId);
                                cubesToReplace.Add(new CubeData(x, y, z, materials, existingCube.ByteCorners));
                            }
                            else if (oldMaterialId == -2 && existingCube != null)
                            {
                                byte[] materials = Cube.CreateMaterialArray((byte)newMaterialId);
                                cubesToReplace.Add(new CubeData(x, y, z, materials, existingCube.ByteCorners));
                            }
                        }
                    }
                }


                if (cubesToReplace.Count > 0)
                    MelonCoroutines.Start(CubeOperations.Add(targetModel, cubesToReplace));

                if (positionsToRemove.Count > 0)
                    MelonCoroutines.Start(CubeOperations.Remove(targetModel, positionsToRemove));
            }
            else if (action == ActionType.Stack)
            {
                var cubesToStack = new List<CubeData>();

                var sourceCubes = new List<(IntVector pos, Cube cube)>();
                for (int x = minX; x <= maxX; x++)
                {
                    for (int y = minY; y <= maxY; y++)
                    {
                        for (int z = minZ; z <= maxZ; z++)
                        {
                            var pos = new IntVector((short)x, (short)y, (short)z);
                            var cube = targetModel.GetCube(pos);
                            if (cube != null)
                            {
                                sourceCubes.Add((pos, cube));
                            }
                        }
                    }
                }

                if (sourceCubes.Count == 0)
                {
                    MelonLogger.Msg("No cubes to stack!");
                    return;
                }

                int sizeX = maxX - minX + 1;
                int sizeY = maxY - minY + 1;
                int sizeZ = maxZ - minZ + 1;

                IntVector offset = StackDirection switch
{
    "up" => new IntVector((short)0, (short)sizeY, (short)0),
    "down" => new IntVector((short)0, (short)(-sizeY), (short)0),
    "left" => new IntVector((short)(-sizeX), (short)0, (short)0),
    "right" => new IntVector((short)sizeX, (short)0, (short)0),
    "forward" => new IntVector((short)0, (short)0, (short)sizeZ),
    "back" => new IntVector((short)0, (short)0, (short)(-sizeZ)),
    _ => new IntVector((short)0, (short)0, (short)0)
};

                for (int i = 1; i <= StackCount; i++)
                {
                    foreach (var (sourcePos, sourceCube) in sourceCubes)
                    {
                        var newPos = new IntVector(
                            (short)(sourcePos.x + offset.x * i),
                            (short)(sourcePos.y + offset.y * i),
                            (short)(sourcePos.z + offset.z * i)
                        );

                        cubesToStack.Add(new CubeData(newPos.x, newPos.y, newPos.z, sourceCube.FaceMaterials, sourceCube.ByteCorners));
                    }
                }

                if (cubesToStack.Count > 0)
                {
                    MelonCoroutines.Start(CubeOperations.Add(targetModel, cubesToStack));
                }
            }

        }


        void CreateVisuals(MVCubeModelBase model)
        {
            if (selectionBoxObj != null) UnityEngine.Object.Destroy(selectionBoxObj);

            selectionBoxObj = new GameObject("ModSelectionBox");
            selectionBoxObj.transform.parent = model.GameObject.transform;
            selectionBoxObj.transform.localPosition = Vector3.zero;
            selectionBoxObj.transform.localRotation = Quaternion.identity;
            selectionBoxObj.transform.localScale = Vector3.one;

            selectionBoxScript = selectionBoxObj.AddComponent<SelectionBox>();
        }

        void UpdateVisuals(IntVector s, IntVector e)
        {
            if (selectionBoxScript == null) return;

            int minX = Mathf.Min(s.x, e.x); int maxX = Mathf.Max(s.x, e.x);
            int minY = Mathf.Min(s.y, e.y); int maxY = Mathf.Max(s.y, e.y);
            int minZ = Mathf.Min(s.z, e.z); int maxZ = Mathf.Max(s.z, e.z);

            Vector3 center = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, (minZ + maxZ) / 2f );
            Vector3 size = new Vector3(maxX - minX + 1, maxY - minY + 1, maxZ - minZ + 1);

            Bounds bounds = new Bounds(center, size);

            Vector3[] corners = SharedCubeFunctions.GetCorners(bounds);

            selectionBoxScript.FadeIn(0.5f, PrefabPool.Instance.SelectBoxMaterial, corners);
        }

        public void ResetTool()
        {
            if (selectionBoxObj != null) UnityEngine.Object.Destroy(selectionBoxObj);
            CurrentState = ToolState.Idle;
            targetModel = null;
        }
    }
}