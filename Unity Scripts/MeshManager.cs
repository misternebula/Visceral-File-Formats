using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor.TSGLoader
{
    class MeshManager : EditorWindow
    {
        string findMeshButton = "Find Mesh";
        string loadMeshGeoButton = "Load Mesh";
        string filePath = "No mesh loaded!";
        int splitCount;
        int[] splitStarts;
        bool autoCombine = false;

        [MenuItem("Window/Simpsons/Mesh")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(MeshManager));
        }

        void OnGUI()
        {
            GUILayout.Label("Load Mesh", EditorStyles.boldLabel);
            if (GUILayout.Button(findMeshButton))
            {
                filePath = EditorUtility.OpenFilePanel("Load Mesh", "", "rws.PS3.preinstanced");
            }
            GUILayout.Label(filePath, EditorStyles.miniLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(loadMeshGeoButton))
            {
                if (filePath != "No mesh loaded!")
                {
                    var filters = new List<MeshFilter>();
                    var meshes = MeshLoader.LoadTSGMesh(filePath);
                    foreach (var mesh in meshes)
                    {
                        var meshObj = new GameObject(mesh.name);
                        var mf = meshObj.AddComponent<MeshFilter>();
                        mf.sharedMesh = mesh;
                        filters.Add(mf);
                        meshObj.AddComponent<MeshRenderer>();
                        var tsg = meshObj.AddComponent<TSGMesh>();
                        tsg.SetReferenceMesh(mesh);
                        meshObj.transform.localScale = new Vector3(-1, 1, 1);
                        meshObj.transform.localPosition = Vector3.zero;
                    }

                    if (autoCombine)
                    {
                        var combine = new CombineInstance[filters.Count];
                        var i = 0;
                        while (i < filters.Count)
                        {
                            combine[i].mesh = filters[i].sharedMesh;
                            combine[i].transform = filters[i].transform.localToWorldMatrix;
                            DestroyImmediate(filters[i].gameObject);
                            i++;
                        }
                        var obj2 = new GameObject("COMBINED");
                        var mf2 = obj2.AddComponent<MeshFilter>();
                        mf2.sharedMesh = new Mesh();
                        mf2.sharedMesh.CombineMeshes(combine, false);
                        obj2.AddComponent<MeshRenderer>();
                        obj2.transform.position = Vector3.zero;

                        var split = filePath.Split('/');
                        int index;
                        if (split.ToList().Contains("props"))
                        {
                            index = split.ToList().FindIndex(x => x == "props");
                        }
                        else
                        {
                            index = split.ToList().FindIndex(x => x == "environs");
                        }

                        if (!AssetDatabase.IsValidFolder($"Assets/Meshes/{split[index + 1]}"))
                        {
                            AssetDatabase.CreateFolder("Assets/Meshes", split[index + 1]);
                        }
                        var savePath = $"Assets/Meshes/{split[index + 1]}/{split[split.Length - 2]}{mf2.sharedMesh.name}-combined.asset";
                        AssetDatabase.CreateAsset(mf2.sharedMesh, savePath);
                    }
                    else
                    {
                        for (var i = 0; i < filters.Count; i++)
                        {
                            var newFilePath = filePath.Substring(filePath.IndexOf("/build/PS3"));
                            newFilePath = newFilePath.Substring(0, newFilePath.LastIndexOf("/"));

                            var savePath = $"Assets/Meshes{newFilePath}/{filters[i].sharedMesh.name}";

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

                            //return;

							/*var split = filePath.Split('/');
                            int index;
                            if (split.ToList().Contains("props"))
                            {
                                index = split.ToList().FindIndex(x => x == "props");
                            }
                            else
                            {
                                index = split.ToList().FindIndex(x => x == "environs");
                            }

                            if (!AssetDatabase.IsValidFolder($"Assets/Meshes/{split[index + 1]}"))
                            {
                                AssetDatabase.CreateFolder("Assets/Meshes", split[index + 1]);
                            }
                            if (!AssetDatabase.IsValidFolder($"Assets/Meshes/{split[index + 1]}/{split[index + 3]}"))
                            {
                                AssetDatabase.CreateFolder($"Assets/Meshes/{split[index + 1]}", split[index + 3]);
                            }
                            filters[i].sharedMesh.name = $"{split[index + 3]}{filters[i].sharedMesh.name}";
                            filters[i].gameObject.name = filters[i].sharedMesh.name;
                            var savePath = $"Assets/Meshes/{split[index + 1]}/{split[index + 3]}/{split[split.Length - 2]}{filters[i].sharedMesh.name}.asset";
                            AssetDatabase.CreateAsset(filters[i].sharedMesh, savePath);*/
                        }
                    }
                }
            }
            autoCombine = EditorGUILayout.Toggle("Combine submeshes?", autoCombine);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            GUILayout.Label("Selected Mesh", EditorStyles.boldLabel);
            var obj = Selection.activeGameObject;
            MeshFilter filter = null;
            if (obj == null)
            {
                GUILayout.Label("No object selected", EditorStyles.miniLabel);
            }
            else
            {
                filter = obj.GetComponent<MeshFilter>();
                if (filter != null)
                {
                    GUILayout.Label(filter.sharedMesh.name, EditorStyles.miniLabel);
                    GUILayout.Label($"Verts : {filter.sharedMesh.vertices.Length}", EditorStyles.miniLabel);
                    GUILayout.Label($"Tris : {filter.sharedMesh.triangles.Length}", EditorStyles.miniLabel);
                    GUILayout.Label($"Faces : {filter.sharedMesh.triangles.Length / 3}", EditorStyles.miniLabel);
                }
            }
        }
    }
}