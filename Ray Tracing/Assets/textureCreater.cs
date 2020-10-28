using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class textureCreater : MonoBehaviour
{
    public Camera ThisCamera;
    public GameObject ThisLight;
    public int texture_width = 512;
    public int texture_height = 512;
    public int texture_SuperSample = 32;
    public int texture_boundtime = 4;
    Texture2D tex;
    float[] Bterm = new float[5];

    // Start is called before the first frame update
    void Start()
    {
        Bterm[0] = 1;
        Bterm[1] = 0.5f;
        Bterm[2] = 0.5f * 0.5f;
        Bterm[3] = 0.5f * 0.5f * 0.5f;
        Bterm[4] = 0.5f * 0.5f * 0.5f * 0.5f;
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
                    tex.SetPixel(i, j, targetColor);
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
        //if (BoundTime == 0)
        //{
        //    if (Physics.Raycast(Input, out MYRayHit, 2000f))
        //    {
        //        Renderer renderer = MYRayHit.collider.GetComponent<MeshRenderer>();
        //        if (MYRayHit.collider.tag == "Light")
        //        {
        //            return Color.white;
        //        }
        //        else
        //        {
        //            Vector3 targetDir = MYRayHit.normal.normalized + new Vector3(Random.Range(-1, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;                    
        //            color = renderer.material.color * RHit(new Ray(MYRayHit.point, targetDir), BoundTime + 1);
        //        }
        //        //color = renderer.material.color;
        //        //}
        //        return color;
        //    }
        //}
        if (BoundTime >= texture_boundtime)
        {
            Vector3 Ldir = ThisLight.GetComponent<Transform>().position - Input.origin;
            Ray NRay = new Ray(Input.origin, Ldir.normalized);
            if (Physics.Raycast(NRay, out MYRayHit, 2000f))
            {
                if (MYRayHit.collider.tag == "Light")//done
                {
                    return Color.white * 3;
                }
            }
            return Color.black;
        }
        if (Physics.Raycast(Input, out MYRayHit, 2000f) && (BoundTime < texture_boundtime))
        {
            Renderer renderer = MYRayHit.collider.GetComponent<MeshRenderer>();
            Ray scattered = new Ray();
            Color attenuation = Color.black;
            Color emitted = Color.black;

            if (MYRayHit.collider.tag == "Light")//done
            {

                return Color.white * 3;
            }
            else if (MYRayHit.collider.tag == "lambertian")
            {
                emitted = Color.black;
                Vector3 scatter_direction = MYRayHit.normal + new Vector3(Random.Range(-1, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
                scattered = new Ray(MYRayHit.point, scatter_direction);
                attenuation = renderer.material.color * 0.5f;
            }
            else if (MYRayHit.collider.tag == "metal")
            {
                emitted = Color.black;


            }
            else if (MYRayHit.collider.tag == "dielectric ")
            {
                emitted = Color.black;
            }          
            return emitted + attenuation * RHit(scattered, ++BoundTime);
        }
        else
        {
            return Color.black;
        }
    }
}
