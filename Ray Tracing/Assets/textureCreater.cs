using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class textureCreater : MonoBehaviour
{
    public Camera ThisCamera;
    public int texture_width = 512;
    public int texture_height = 512;
    public int texture_SuperSample = 32;
    public int texture_boundtime = 4;    
    Texture2D tex;
    // Start is called before the first frame update
    void Start()
    {


    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            tex = new Texture2D(texture_width, texture_height);
            for (int i = 0; i < texture_width; i++)
            {
                for (int j = 0; j < texture_height; j++)
                {
                    Color targetColor = Color.black;
                    for (int k = 0; k < texture_SuperSample; k++)
                    {
                        Ray pixelRay = ThisCamera.ScreenPointToRay(new Vector2(i + Random.Range(-1f, 1f), j + Random.Range(-1f, 1f)));
                        targetColor += RHit(pixelRay, 0);
                    }
                    targetColor = targetColor / texture_SuperSample;
                    targetColor.a = 1.0f;
                    tex.SetPixel(i, j, targetColor );
                }
            }
            Debug.Log("Saving....");
            var bytes = tex.EncodeToPNG();
            File.WriteAllBytes(Path.Combine(Application.dataPath, "img.png"), bytes);
        }

    }


    Color RHit(Ray Input, int BoundTime)
    {
        RaycastHit MYRayHit;
        Color color = Color.black;
        if (Physics.Raycast(Input, out MYRayHit, 2000f) && (BoundTime < texture_boundtime))
        {
            Renderer renderer = MYRayHit.collider.GetComponent<MeshRenderer>();

            //Mesh hitMesh = MYRayHit.collider.GetComponent<MeshFilter>().sharedMesh;
            //int Tri_Idx = MYRayHit.triangleIndex;
            //int Tri_Idx1 = hitMesh.triangles[Tri_Idx * 3];
            //int Tri_Idx2 = hitMesh.triangles[Tri_Idx * 3 + 1];
            //int Tri_Idx3 = hitMesh.triangles[Tri_Idx * 3 + 2];
            //int materialIdx = -1;
            //for (int k = 0; k < hitMesh.subMeshCount; k++)
            //{
            //    int[] tri = hitMesh.GetTriangles(k);
            //    for (int tr = 0; tr < tri.Length; tr = tr + 3)
            //    {
            //        if ((tri[tr + 0] == Tri_Idx1) && (tri[tr + 1] == Tri_Idx2) && (tri[tr + 2] == Tri_Idx3))
            //        {
            //            materialIdx = k;
            //            break;
            //        }
            //    }
            //    if (materialIdx != -1)
            //    {
            //        break;
            //    }
            //}
            //Texture2D texture2D = renderer.materials[materialIdx].mainTexture as Texture2D;
            //if (texture2D != null)
            //{
            //    Vector2 pCoord = MYRayHit.textureCoord;
            //    pCoord.x *= texture2D.width;
            //    pCoord.y *= texture2D.height;
            //    Vector2 tiling = renderer.materials[materialIdx].mainTextureScale;
            //    color = texture2D.GetPixel(Mathf.FloorToInt(pCoord.x * tiling.x), Mathf.FloorToInt(pCoord.y * tiling.y));
            //}
            //else
            //{
            if (MYRayHit.collider.tag == "Light")
            {
                return renderer.material.color;
            }
            else
            {
                Vector3 targetDir = MYRayHit.normal.normalized + new Vector3(Random.Range(-1, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;

                color = 0.5f * renderer.material.color +  RHit(new Ray(MYRayHit.point, targetDir), BoundTime + 1);

            }
            //color = renderer.material.color;
            //}
            return color;
        }
        else
        {
            return Color.black;
        }
    }
}
