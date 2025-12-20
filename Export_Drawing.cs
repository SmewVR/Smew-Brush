
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

using TMPro;

namespace QvPen.UdonScript
{
    public class Export_Drawing : UdonSharpBehaviour
    {
        [SerializeField]
        private QvPen_Pen pen;

        // Variables for LineRenderer _Export (smew 6/28/2024)
        private LineRenderer lineRenderer;
        private string formatPos;

        //for Baked Mesh export method
        private Mesh testmesh;
        private Vector3[] mesh_vertices;

        // normals added 4/22/2025
        private Vector3[] mesh_normals;

        private int[] mesh_tris;

        private Vector2[] mesh_uvs;
        private int v1;
        private int v2;
        private int v3;

        private int vt1;
        private int vt2;
        private int vt3;

        private int vn1;
        private int vn2;
        private int vn3;

        private int object_counter;

        private int vertexOffset;
        public TMP_InputField export_text_field;
        // public TMP_Text export_text_display;

        public string Interact()
        {
            if (export_text_field) 
                export_text_field.text = _Export();

            return ""; 

        }
        public string _Export()
        {
            formatPos = "";
            formatPos += string.Format("# OBJ file created in the Smew Brush! VRChat world \n {0} \n https://vrchat.com/home/world/wrld_0097c2e4-634a-445b-a43d-9585cb6df959 \n\n", System.DateTime.Now.ToString());
            object_counter = 0;
            vertexOffset = 0;

            foreach (Transform ink in pen.inkPoolSynced)
            {

                // call GetPositions() with the array to populate it with line positions
                lineRenderer = ink.GetComponent<LineRenderer>();
                //simplify number of points (uncomment once working)
                lineRenderer.Simplify(0.001f);

                Mesh testmesh = new Mesh();

                //bake mesh to access full vertex data
                lineRenderer.BakeMesh(testmesh, true);

                mesh_vertices = testmesh.vertices;
                mesh_tris = testmesh.triangles;
                mesh_normals = testmesh.normals;
                mesh_uvs = testmesh.uv;
                
                Debug.Log(mesh_vertices);
                Debug.Log(mesh_vertices.Length);

                formatPos += "o Object_" + object_counter.ToString() + "\n";
                object_counter++;

                // adjust to keep the uv map
                // Adjust vertices to keep the UV map
                // 1. Calculate the UV coordinates for each vertex based on its position.
                // 2. Create a new array to store the UV coordinates.
                // 3. Iterate through the vertices and assign UV coordinates to each vertex.
                // 4. Assign the UV coordinates array to the mesh's UV property.

                // export vertex data
                for (int i = 0; i < mesh_vertices.Length; i++)
                    formatPos += string.Format("v {0} {1} {2}", mesh_vertices[i].x, mesh_vertices[i].y, mesh_vertices[i].z) + "\n";

                Debug.Log(formatPos);

                // // export face data
                // for (int i = 0; i < mesh_tris.Length; i += 3)
                //     // Note: OBJ format uses 1-based 
                //     formatPos += string.Format("f {0}/1/1 {1}/1/1 {2}/1/1", mesh_tris[i] + 1 + vertexOffset, mesh_tris[i + 1] + 1 + vertexOffset, mesh_tris[i + 2] + 1 + vertexOffset) + "\n";
                
                //NORMALS vn x y z
                // Write normals (fallback if empty)
                if (mesh_normals == null || mesh_normals.Length == 0)
                {
                    for (int i = 0; i < mesh_vertices.Length; i++)
                        formatPos += "vn 0 1 0\n"; // default up normal
                }
                else
                {
                    for (int i = 0; i < mesh_normals.Length; i++)
                    {
                        Vector3 n = mesh_normals[i];
                        formatPos += $"vn {n.x} {n.y} {n.z}\n";
                    }
                }
                // UVs vt x y
                    // added 4/22/2025
                // Write UVs (simple planar mapping using x/z)
                for (int i = 0; i < mesh_uvs.Length; i++)
                {
                    Vector2 uv = mesh_uvs[i];
                    float u = uv.x;
                    float vCoord = uv.y;
                    formatPos += $"vt {u} {vCoord}\n";
                }

                // FACES f / / /
                // Write faces with 1-based indices and matching v/vt/vn
                for (int i = 0; i < mesh_tris.Length; i += 3)
                {
                    int v1 = mesh_tris[i] + 1 + vertexOffset;
                    int v2 = mesh_tris[i + 1] + 1 + vertexOffset;
                    int v3 = mesh_tris[i + 2] + 1 + vertexOffset;
 
                    formatPos +=  $"f {v1}/{}/{} {v2}/{}/{} {v3}/{}/{}\n";
                }

                // // export normals
                // for (int i = 0; i < mesh_vertices.Length; i++)
                //    formatPos += string.Format("vn {0} {1} {2}", mesh_vertices[i].x, mesh_vertices[i].y, mesh_vertices[i].z) + "\n";
                // // export UVs
                // for (int i = 0; i < mesh_vertices.Length; i++)
                //    formatPos += string.Format("vt {0} {1}", mesh_vertices[i].x, mesh_vertices[i].y) + "\n";

                vertexOffset += mesh_vertices.Length;
            }
    
            // Trim any trailing whitespace or newline characters
            formatPos = formatPos.Trim();
            
            return formatPos;
        }

    }

}