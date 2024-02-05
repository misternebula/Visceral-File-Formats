using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor.TSGLoader
{
    class CollisionManager : EditorWindow
    {
        string findColliderButton = "Find Collider";
        string loadColliderButton = "Load Collider";
        string filePath = "No collider loaded!";

        [MenuItem("Window/Simpsons/Collision")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(CollisionManager));
        }

        void OnGUI()
        {
            GUILayout.Label("Load Mesh", EditorStyles.boldLabel);
            if (GUILayout.Button(findColliderButton))
            {
                filePath = EditorUtility.OpenFilePanel("Load Collider", "", "hkt.PS3,hko,hko.PS3");
            }
            GUILayout.Label(filePath, EditorStyles.miniLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(loadColliderButton))
            {
                if (filePath != "No collider loaded!")
                {
                    var filters = new List<MeshFilter>();
                    var (meshes, radii) = CollisionLoader.LoadTSGCollider(filePath);

                    Debug.Log($"Got {meshes.Count} meshes.");

                    foreach (var mesh in meshes)
                    {
                        var meshObj = new GameObject(mesh.name);
                        var mf = meshObj.AddComponent<MeshFilter>();
                        mf.sharedMesh = mesh;
                        filters.Add(mf);
                        meshObj.AddComponent<MeshCollider>();
                        meshObj.transform.localScale = new Vector3(-1, 1, 1);
                        meshObj.transform.localPosition = Vector3.zero;
                    }

                    foreach (var radius in radii)
                    {
	                    var meshObj = new GameObject("hkSphereShape");
	                    meshObj.AddComponent<SphereCollider>();
                        meshObj.GetComponent<SphereCollider>().radius = radius;
                    }

                    for (var i = 0; i < filters.Count; i++)
                    {
						var newFilePath = filePath.Substring(filePath.IndexOf("/build/PS3"));
						newFilePath = newFilePath.Substring(0, newFilePath.LastIndexOf("/"));

						var savePath = $"Assets/Collision{newFilePath}/{filters[i].sharedMesh.name}";

						var splits = savePath.Split("/");

						string storedPath = "Assets";
						for (int j = 1; j < splits.Length - 1; j++)
						{
							if (!AssetDatabase.IsValidFolder($"{storedPath}/{splits[j]}"))
							{
								AssetDatabase.CreateFolder(storedPath, splits[j]);
							}

							storedPath += $"/{splits[j]}";
						}

						AssetDatabase.CreateAsset(filters[i].sharedMesh, savePath + ".asset");
					}
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}