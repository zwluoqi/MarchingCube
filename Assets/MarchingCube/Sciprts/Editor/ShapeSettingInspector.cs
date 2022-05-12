﻿using System;
using UnityEditor;
using UnityEngine;

namespace MarchingCube.Sciprts
{
    [CustomEditor(typeof(MarchingCubeMesh))]
    public class ShapeSettingInspector : Editor
    {

        public MarchingCubeMesh marchingCubeMesh;
        private Editor shapeEditor;
        private Editor debugEditor;
        private void OnEnable()
        {
            marchingCubeMesh = target as MarchingCubeMesh;
            ;
        }

        private Vector3 pos;
        public override void OnInspectorGUI()
        {
            
            base.OnInspectorGUI();
            DrawSettingEditor(marchingCubeMesh.shapeSetting, marchingCubeMesh.OnShapeSetttingUpdated,
                ref marchingCubeMesh.shapeSetttingsFoldOut, ref shapeEditor);
            DrawSettingEditor(marchingCubeMesh.debugSetting, marchingCubeMesh.OnDebugSetttingUpdated,
                ref marchingCubeMesh.debugSetttingsFoldOut, ref debugEditor);
            if (pos != ((MarchingCubeMesh) target).transform.position)
            {
                marchingCubeMesh.OnPositionUpdated();
            }
        }
        
        private void DrawSettingEditor(ScriptableObject planetMeshShapeSettting, Action onShapeSetttingUpdated, ref bool planetMeshShpaeSetttingsFoldOut, ref Editor editor)
        {
            if (planetMeshShapeSettting != null)
            {
                planetMeshShpaeSetttingsFoldOut =
                    EditorGUILayout.InspectorTitlebar(planetMeshShpaeSetttingsFoldOut, planetMeshShapeSettting);
                if (planetMeshShpaeSetttingsFoldOut)
                {
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        CreateCachedEditor(planetMeshShapeSettting, null, ref editor);
                        editor.OnInspectorGUI();
                        if (check.changed)
                        {
                            if (onShapeSetttingUpdated != null)
                            {
                                onShapeSetttingUpdated();
                            }
                        }
                    }
                }
            }
        }

    }
}