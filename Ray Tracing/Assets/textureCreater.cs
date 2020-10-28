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

                    //float scale = 1.0f / texture_SuperSample;
                    //targetColor.r = Mathf.Sqrt(scale * targetColor.r);
                    //targetColor.g = Mathf.Sqrt(scale * targetColor.g);
                    //targetColor.b = Mathf.Sqrt(scale * targetColor.b);

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
        
        //if (BoundTime >= texture_boundtime)
        //{
        //    Vector3 Ldir = ThisLight.GetComponent<Transform>().position - Input.origin;
        //    Ray NRay = new Ray(Input.origin, Ldir.normalized);
        //    if (Physics.Raycast(NRay, out MYRayHit, 2000f))
        //    {
        //        if (MYRayHit.collider.tag == "Light")//done
        //        {
        //            return Color.white * 4;
        //        }
        //    }
        //    return Color.black;
        //}


        if (Physics.Raycast(Input, out MYRayHit, 2000f) && (BoundTime < texture_boundtime))
        {
            Renderer renderer = MYRayHit.collider.GetComponent<MeshRenderer>();
            Ray scattered = new Ray();
            Color attenuation = Color.black;
            Color emitted = Color.black;

            if (MYRayHit.collider.tag == "Light")//done
            {

                return Color.white * 4;
            }
            else if (MYRayHit.collider.tag == "lambertian")
            {
                emitted = Color.black;
                Vector3 scatter_direction = MYRayHit.normal + new Vector3(Random.Range(-1, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
                scattered = new Ray(MYRayHit.point, scatter_direction);
                attenuation = renderer.material.color;
            }
            else if (MYRayHit.collider.tag == "metal")
            {
                float fuzz = 0.1f;
                emitted = Color.black;
                Vector3 reflected = Vector3.Reflect(Input.direction.normalized, MYRayHit.normal);
                scattered = new Ray(MYRayHit.point, reflected + fuzz * new Vector3(Random.Range(-1, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized);
                attenuation = renderer.material.color;
                if (Vector3.Dot(scattered.direction, MYRayHit.normal) > 0)
                {

                }
                else
                {
                    return Color.black;
                }
            }
            else if (MYRayHit.collider.tag == "dielectric")
            {
                Vector3 out_normal = Vector3.zero;
                Vector3 reflected = reflect(Input.direction, MYRayHit.normal);
                float reffactor = 1.5f;
                float realfactor = 1.5f;
                attenuation = Color.white;
                Vector3 refracted = Vector3.zero;
                float reflectp = 0f;
                float cosine = 0f;
                if (Vector3.Dot(Input.direction, MYRayHit.normal) > 0)
                {
                    out_normal = -MYRayHit.normal;
                    realfactor = reffactor;
                    cosine = reffactor * Vector3.Dot(Input.direction, MYRayHit.normal) / Input.direction.magnitude;
                }
                else
                {
                    out_normal = MYRayHit.normal;
                    realfactor = 1.0f / reffactor;
                    cosine = -Vector3.Dot(Input.direction, MYRayHit.normal) / Input.direction.magnitude;
                }
                if (refract(Input.direction, out_normal, realfactor, ref refracted))
                {
                    float r = (1f - reffactor) / (1f + reffactor);
                    r = r * r;
                    reflectp = r + (1 - r) * Mathf.Pow((1 - cosine), 5);
                }
                else
                {
                    scattered = new Ray(MYRayHit.point, reflected);
                    reflectp = 1.0f;
                }
                if (Random48.Get() < reflectp)
                {
                    scattered = new Ray(MYRayHit.point, reflected);
                }
                else
                {
                    scattered = new Ray(MYRayHit.point, refracted);
                }
                emitted = Color.black;
            }
            return emitted + attenuation * RHit(scattered, ++BoundTime);
        }
        else
        {
            return Color.black;
        }
    }
    Vector3 reflect(Vector3 v, Vector3 n)
    {
        return v - 2 * Vector3.Dot(v, n) * n;
    }

    bool refract(Vector3 v, Vector3 n, float reffactor, ref Vector3 refracted)
    {
        Vector3 v_norm = v.normalized;
        float dotvn = Vector3.Dot(v_norm, n);
        float checkrefract = 1.0f - reffactor * reffactor * (1.0f - dotvn * dotvn);
        if (checkrefract > 0)
        {
            refracted = reffactor * (v_norm - n * dotvn) - n * Mathf.Sqrt(checkrefract);
            return true;
        }
        else return false;
    }
}
