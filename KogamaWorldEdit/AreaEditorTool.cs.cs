using System;
using Il2Cpp;
using Il2CppMV.Common;
using Il2CppMV.WorldObject;
using UnityEngine;
using MelonLoader;

// Made By Veni
// Improved by 

namespace TestMod.Features
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

        public int selectedMaterialId = 1;
        private bool showGui = false;

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

                if (UnityEngine.Input.GetMouseButtonDown(1))
                {
                    HandleClick(hoveredModel, hoveredPos, hoveredMat);
                }

                if (CurrentState == ToolState.SelectingSecond && hoveredModel == targetModel)
                {
                    UpdateVisuals(startPos, hoveredPos);
                }
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
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
                MelonLogger.Msg($"Start: {pos}. Pobranno materiał ID: {materialID}");

                CreateVisuals(model);
                UpdateVisuals(startPos, startPos);
                showGui = true;
            }
            else if (CurrentState == ToolState.SelectingSecond)
            {
                if (model == targetModel)
                {
                    endPos = pos;
                    CurrentState = ToolState.AreaSelected;
                    UpdateVisuals(startPos, endPos);
                    MelonLogger.Msg($"Koniec: {pos}. Zaznaczono obszar.");
                }
                else
                {
                    MelonLogger.Msg("Błąd: Musisz zaznaczyć kostki w tym samym obiekcie!");
                    ResetTool();
                }
            }
        }


        public void DrawImGuiMenu()
        {
            if (!showGui) return;
            if (MVGameControllerBase.GameMode != MVGameMode.Edit) return;

            if (selectedMaterialId < 1) selectedMaterialId = 1;
            if (selectedMaterialId > 63) selectedMaterialId = 63;

            if (!showGui)
            {
                ResetTool();
            }
        }

        // --- (Fill/Clear) ---

        public enum ActionType { Fill, Clear, Replace }

        public void PerformAction(ActionType action)
        {
            if (targetModel == null) return;

            int minX = Mathf.Min(startPos.x, endPos.x); int maxX = Mathf.Max(startPos.x, endPos.x);
            int minY = Mathf.Min(startPos.y, endPos.y); int maxY = Mathf.Max(startPos.y, endPos.y);
            int minZ = Mathf.Min(startPos.z, endPos.z); int maxZ = Mathf.Max(startPos.z, endPos.z);

            targetModel.MakeUnique();

            int operations = 0;

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    for (int z = minZ; z <= maxZ; z++)
                    {
                        IntVector pos = new IntVector((short)x, (short)y, (short)z);
                        var existingCube = targetModel.GetCube(pos);

                        if (action == ActionType.Clear)
                        {
                            if (existingCube != null)
                            {
                                targetModel.RemoveCube(pos);
                                operations++;
                            }
                        }
                        else if (action == ActionType.Fill)
                        {
                            if (existingCube == null)
                            {
                                Cube newCube = new Cube(
                                    CubeDataPacker.CornersToByteArray(CubeBase.IdentityCorners),
                                    Cube.CreateMaterialArray((byte)selectedMaterialId)
                                );
                                targetModel.AddCube(pos, newCube);
                                operations++;
                            }
                        }
                        else if (action == ActionType.Replace)
                        {
                            if (existingCube != null)
                            {
                                Cube existing = targetModel.GetCube(pos);
                                Cube newCube = new Cube(
                                    existing.ByteCorners,
                                    Cube.CreateMaterialArray((byte)selectedMaterialId)
                                );
                                targetModel.RemoveCube(pos);
                                targetModel.AddCube(pos, newCube);
                                operations++;
                            }
                        }
                    }
                }
            }

            if (operations > 0)
            {
                targetModel.HandleDelta();
                MelonLogger.Msg($"Sukces! Zaktualizowano {operations} kostek w jednej paczce.");
            }
            else
            {
                MelonLogger.Msg("Brak zmian do wprowadzenia.");
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
            showGui = false;
        }
    }
}