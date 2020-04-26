﻿using UnityEditor;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

#if UNITY_EDITOR
namespace Voxelizer
{
    public class VoxelMenu : EditorWindow
    {
        [MenuItem("Window/Voxelize Sprite")]
        public static void ShowWindow ()
        {
            EditorWindow.GetWindow<VoxelMenu>("Voxelizer Menu");
        }

        private Sprite _sprite;
        private Material _material;
        private bool _useMeshOptimizer;
        private bool _saveMesh;
        private bool _createNewGameObject = true;

        private void OnGUI()
        {
            _sprite = (Sprite)EditorGUILayout.ObjectField("Selected Sprite", _sprite, typeof(Sprite), true);

            string debugText = null;

            if (_sprite == null)
            {
                GUI.enabled = false;
                debugText = "Need to Select a Sprite";
            }
            else
            {
                if (_sprite.texture.isReadable == false)
                {
                    GUI.enabled = false;
                    debugText = "Read/Write needs to be enabled in the sprite's Import Settings";
                }
                else if (_sprite.texture.format != TextureFormat.RGBA32)
                {
                    debugText = "For best results, set sprite compression format to RGBA32 before converting";
                }
            }

            _material = (Material)EditorGUILayout.ObjectField("Rendering material", _material, typeof(Material), true);
            EditorGUILayout.Space();


            GUILayout.BeginVertical("HelpBox");
            _useMeshOptimizer = EditorGUILayout.Toggle("Use Mesh Optimizer", _useMeshOptimizer);
            EditorGUILayout.HelpBox("Unity's mesh optimizer optimizes the Mesh data to improve rendering performance. This operation can take a few seconds or more for complex meshes", MessageType.Info);
            GUILayout.EndVertical();
            EditorGUILayout.Space();


            _saveMesh = EditorGUILayout.Toggle("Save Mesh To File", _saveMesh);
            _createNewGameObject = EditorGUILayout.Toggle("Add Mesh to scene", _createNewGameObject);
            EditorGUILayout.Space();


            if (GUILayout.Button("Create"))
            {
                VoxelizeSprite();
            }
            GUI.enabled = true;

            if (debugText != null)
            {
                EditorGUILayout.HelpBox(debugText, MessageType.Warning);
            }
        }

        private void VoxelizeSprite()
        {
            var timer = Stopwatch.StartNew();

            Mesh mesh = Voxelizer.Util.VoxelizeTexture2D(_sprite.texture);

            if (_useMeshOptimizer)
            {
                MeshUtility.Optimize(mesh);
            }

            timer.Stop();

            string meshName = _sprite.name + "_3D";
            mesh.name = meshName;
            Debug.Log(string.Format("[Voxelizer] {0}: Mesh created after {1} milliseconds", meshName, timer.ElapsedMilliseconds));

            if (_saveMesh)
            {
                SaveMeshToFile(mesh);
            }

            if (_createNewGameObject)
            {
                CreateVoxelGameObject(mesh);
            }
        }

        private void CreateVoxelGameObject(Mesh mesh)
        {
            var sprite3D = new GameObject(_sprite.name + "_3D");

            var meshFilter = sprite3D.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;

            var meshRenderer = sprite3D.AddComponent<MeshRenderer>();
            meshRenderer.material = _material == null ? Resources.Load<Material>("Materials/3DSpriteMaterial") : _material;
        }

        private void SaveMeshToFile(Mesh mesh)
        {
            string path = EditorUtility.SaveFilePanel("Save textures to folder", "Assets/", mesh.name, "obj");

            path = FileUtil.GetProjectRelativePath(path);

            if (string.IsNullOrEmpty(path) == false)
            {
                AssetDatabase.CreateAsset(mesh, path);
                AssetDatabase.SaveAssets();
            }
        }
    }
}
#endif