using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor.TSGLoader
{
    public static class MeshLoader
    {
        public static List<Mesh> LoadTSGMesh(string filePath)
        {
            var bytes = File.ReadAllBytes(filePath);

            var file = new string[bytes.Length];

            for (var i = 0; i < bytes.Length; i++)
            {
                file[i] = bytes[i].ToString("X2");
            }

            var meshList = new List<Mesh>();

            var eaMeshHeader = new string[4];
            eaMeshHeader[0] = "33";
            eaMeshHeader[1] = "EA";
            eaMeshHeader[2] = "00";
            eaMeshHeader[3] = "00";

            var headerStarts = Search(file, eaMeshHeader);
            for (var h = 0; h < headerStarts.Count; h++)
            {
                if (GetIntData(headerStarts[h], 8, file, false) != 0x2D00021C)
                {
                    continue;
                }
                Debug.Log($"header {h}");
                // headerstarts[h] is the byte 33 of 33EA0000
                var faceDataOffset = GetIntData(headerStarts[h], 16, file, true);
                var meshChunkStart = headerStarts[h] + 24;
                var tableLoc = headerStarts[h] + 44;
                var mDataTableCount = GetIntData(tableLoc, 0, file, false);
                var mDataSubCount = GetIntData(tableLoc, 4, file, false);
                var mDataSubStart = tableLoc + 8 + (8 * mDataTableCount);

                int offsetTrack;
                for (var i = 0; i < mDataSubCount; i++)
                {
                    var mesh = new Mesh();
                    var vertList = new List<Vector3>();
                    var uv1List = new List<Vector2>();
                    var uv2List = new List<Vector2>();
                    var normalList = new List<Vector3>();
                    var triangleList = new List<int>();
                    var vertColorList = new List<Color32>();

                    offsetTrack = mDataSubStart + (i * 12) + 8;

                    var offset = GetIntData(offsetTrack, 0, file, false);
                    var chunkHead = offset + meshChunkStart + 12;

                    var vertCountDataOffset = GetIntData(chunkHead, 0, file, false) + meshChunkStart;
                    var vertChunkTotalSize = GetIntData(vertCountDataOffset, 0, file, false);
                    var vertChunkEntrySize = GetIntData(vertCountDataOffset, 4, file, false);
                    var vertCount = vertChunkTotalSize / vertChunkEntrySize;
                    var vertStart = GetIntData(vertCountDataOffset, 16, file, false) + faceDataOffset + meshChunkStart;
                    var lengthOfStripBlock = GetIntData(vertCountDataOffset, 40, file, false) / 2;
                    var faceStart = GetIntData(vertCountDataOffset, 48, file, false) + faceDataOffset + meshChunkStart;

                    var tempList = new List<int>();
                    var StripList = new List<int[]>();

                    for (var f = 0; f < lengthOfStripBlock; f++)
                    {
                        var indice = GetIntData(faceStart, f * 2, file, false, 2);
                        if (indice == 65535)
                        {
                            StripList.Add(tempList.ToArray());
                            tempList.Clear();
                        }
                        else
                        {
                            tempList.Add(indice);
						}
                    }

                    foreach (var strip in StripList)
                    {
                        foreach (var face in StripToFaces(strip.ToList()))
                        {
                            triangleList.Add(face[0]);
                            triangleList.Add(face[1]);
                            triangleList.Add(face[2]);
                        }
                    }

                    int offsetTracker;
                    for (var v = 0; v < vertCount; v++)
                    {
                        offsetTracker = vertStart + (v * vertChunkEntrySize);
                        var thing = string.Join("", file.Skip(offsetTracker).Take(12));
                        vertList.Add(GetXYZ(thing));

                        var normals = string.Join("", file.Skip(offsetTracker + 12).Take(4));
                        normalList.Add(GetNormals(normals));

                        var vertexColor = string.Join("", file.Skip(offsetTracker + 16).Take(4));
                        vertColorList.Add(GetVertColor(vertexColor));

						var uv1 = string.Join("", file.Skip(offsetTracker + vertChunkEntrySize - 8).Take(8));
                        uv1List.Add(GetUV(uv1));
                        var uv2 = string.Join("", file.Skip(offsetTracker + vertChunkEntrySize - 16).Take(8));
                        uv2List.Add(GetUV(uv2));
                    }

                    mesh.SetVertices(vertList);
                    mesh.SetTriangles(triangleList, 0);
                    mesh.SetUVs(1, uv1List);
                    mesh.SetUVs(0, uv2List);
                    mesh.SetNormals(normalList);
                    mesh.SetColors(vertColorList);
                    //mesh.RecalculateNormals();
                    mesh.name = $"EAMesh{h}Submesh{i}";
                    meshList.Add(mesh);
                }
            }
            return meshList;
        }

        static List<int[]> StripToFaces(List<int> strip)
        {
            var dict = new Dictionary<int, int>();
            foreach (var index in strip)
            {
	            try
	            {
		            dict.Add(index, 0);
	            }
	            catch
	            {
		            dict[index] += 1;
	            }
            }

            var tmpTable = new List<int[]>();
            var flipped = false;
			for (var i = 0; i < strip.Count - 2; i++)
            {
				if (flipped)
                {
					tmpTable.Add(new int[] { strip[i + 2], strip[i + 1], strip[i] });
					dict[strip[i]] += 1;
                    dict[strip[i + 1]] += 1;
                    dict[strip[i + 2]] += 1;
                }
                else
                {
					tmpTable.Add(new int[] { strip[i + 1], strip[i + 2], strip[i] });
					dict[strip[i]] += 1;
                    dict[strip[i + 1]] += 1;
                    dict[strip[i + 2]] += 1;
                }

                flipped = !flipped;
            }

            return tmpTable;
        }

        static Color32 GetVertColor(string hex)
        {
	        var r = new string(hex.Skip(0).Take(2).ToArray());
	        r = Convert.ToString(Convert.ToInt64(r, 16), 2);
	        var g = new string(hex.Skip(2).Take(2).ToArray());
	        g = Convert.ToString(Convert.ToInt64(g, 16), 2);
	        var b = new string(hex.Skip(4).Take(2).ToArray());
	        b = Convert.ToString(Convert.ToInt64(b, 16), 2);
	        var a = new string(hex.Skip(6).Take(2).ToArray());
	        a = Convert.ToString(Convert.ToInt64(a, 16), 2);

	        return new Color32(
		        (byte)Convert.ToInt32(r, 2),
		        (byte)Convert.ToInt32(g, 2),
		        (byte)Convert.ToInt32(b, 2),
		        (byte)Convert.ToInt32(a, 2));
        }

        static Vector2 GetUV(string hex)
        {
            var u = new string(hex.Skip(0).Take(8).ToArray());
            u = Convert.ToString(Convert.ToInt64(u, 16), 2);
            var v = new string(hex.Skip(8).Take(8).ToArray());
            v = Convert.ToString(Convert.ToInt64(v, 16), 2);

            return new Vector2
            {
                x = GetFloatFromBinary(u),
                y = 1-GetFloatFromBinary(v)
            };
        }

        static Vector3 GetXYZ(string hex)
        {
            var x = new string(hex.Skip(0).Take(8).ToArray());
            x = Convert.ToString(Convert.ToInt64(x, 16), 2);
            var y = new string(hex.Skip(8).Take(8).ToArray());
            y = Convert.ToString(Convert.ToInt64(y, 16), 2);
            var z = new string(hex.Skip(16).Take(8).ToArray());
            z = Convert.ToString(Convert.ToInt64(z, 16), 2);

            return new Vector3
            {
                x = GetFloatFromBinary(x),
                y = GetFloatFromBinary(y),
                z = GetFloatFromBinary(z)
            };
        }

        static Vector3 GetNormals(string hex)
        {
			var binarystring = string.Join(string.Empty,
				hex.Select(
					c => Convert.ToString(Convert.ToInt32(c.ToString(), 16), 2).PadLeft(4, '0')
				)
			);

			var zBinary = binarystring.Skip(00).Take(10).ToArray();
			var yBinary = binarystring.Skip(10).Take(11).ToArray();
			var xBinary = binarystring.Skip(21).Take(11).ToArray();

			var zInt = Convert.ToInt32(new string(zBinary.Skip(1).ToArray()), 2);
			var yInt = Convert.ToInt32(new string(yBinary.Skip(1).ToArray()), 2);
			var xInt = Convert.ToInt32(new string(xBinary.Skip(1).ToArray()), 2);

			var z = zBinary[0] == '0' ? zInt : zInt - 512f;
			var y = yBinary[0] == '0' ? yInt : yInt - 1024f;
	        var x = xBinary[0] == '0' ? xInt : xInt - 1024f;

	        z /= 511f;
	        y /= 1023f;
	        x /= 1023f;

			return new Vector3(x, y, z).normalized;
        }

		public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        static float GetFloatFromBinary(string binary)
        {
            if (binary == "0")
            {
                return 0;
            }

            if (binary.Length < 32)
            {
                binary = new string('0', 32 - binary.Length) + binary;
            }

            var sign = int.Parse(binary[0].ToString()) == 1 ? -1 : 1;

            var exponentS = string.Join("", binary.Skip(1).Take(8));
            var exponent = Convert.ToInt32(exponentS, 2);
            var mantissa = string.Join("", binary.Skip(9).Take(23));
            var manTotal = 0f;
            for (var i = 1; i < 24; i++)
            {
                if (mantissa[i - 1] == '1')
                {
                    manTotal += (float)(1 / Math.Pow(2, i));
                }
            }
            manTotal += 1f;

            var value = (float)(sign * Math.Pow(2, -(127 - exponent)) * manTotal);

            return value;
        }

        static int GetIntData(int start, int offset, string[] file, bool reverse, int takeAmount = 4)
        {
            try
            {
                return reverse
                    ? int.Parse(string.Join("", file.Skip(start + offset).Take(takeAmount).Reverse()), System.Globalization.NumberStyles.HexNumber)
                    : int.Parse(string.Join("", file.Skip(start + offset).Take(takeAmount)), System.Globalization.NumberStyles.HexNumber);
            }
            catch
            {
                Debug.LogError("Error while getting int data from " + string.Join("", file.Skip(start + offset).Take(takeAmount)));
            }
            return 1;
        }

        static List<int> Search(string[] fileToSearch, string[] whatToFind)
        {
            var indexPlaces = new List<int>();
            for (var i = 0; i <= fileToSearch.Length - whatToFind.Length; i++)
            {
                if (IsMatch(fileToSearch, whatToFind, i))
                {
                    indexPlaces.Add(i);
                }
            }
            return indexPlaces;
        }

        static bool IsMatch(string[] fileToSearch, string[] whatToFind, int start)
        {
            if (whatToFind.Length + start > fileToSearch.Length)
            {
                return false;
            }
            else
            {
                for (var i = 0; i < whatToFind.Length; i++)
                {
                    if (whatToFind[i] != fileToSearch[i + start])
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }
}
