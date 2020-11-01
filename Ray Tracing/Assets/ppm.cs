using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class ppm : MonoBehaviour
{
    public Camera ThisCamera;
    public GameObject ThisLight;
    public int texture_width = 512;
    public int texture_height = 512;
    public int texture_SuperSample = 4;
    public int texture_boundtime = 3;
    public float InitR = 0.5f;
    public int Photon_iter_Time = 1;
    public int OneTimePhoton = 1000;
    Texture2D tex;

    Vector3[,,] HitPoint_position;
    int[,,] HitPoint_photonNum;
    float[,,] HitPoint_R;
    float[,,] HitPoint_Light;
    // Start is called before the first frame update
    void Start()
    {
        HitPoint_position = new Vector3[texture_width, texture_height, texture_SuperSample];
        HitPoint_photonNum = new int[texture_width, texture_height, texture_SuperSample];
        HitPoint_R = new float[texture_width, texture_height, texture_SuperSample];
        HitPoint_Light = new float[texture_width, texture_height, texture_SuperSample];
        for (int i = 0; i < texture_width; i++)
        {
            for (int j = 0; j < texture_height; j++)
            {
                for (int k = 0; k < texture_SuperSample; k++)
                {
                    HitPoint_photonNum[i, j, k] = 0;
                    HitPoint_R[i, j, k] = InitR;
                    HitPoint_Light[i, j, k] = 0.0f;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            System.Diagnostics.Stopwatch time = new System.Diagnostics.Stopwatch();
            time.Start();
            tex = new Texture2D(texture_width, texture_height);
            for (int i = 0; i < texture_width; i++)
            {
                for (int j = 0; j < texture_height; j++)
                {
                    for (int k = 0; k < texture_SuperSample; k++)
                    {
                        Ray pixelRay = ThisCamera.ScreenPointToRay(new Vector2(i, j));
                        RayCastPass(pixelRay, i, j, k);
                    }
                }
            }

            for (int t = 0; t < Photon_iter_Time; t++)
            {
                for (int i = 0; i < texture_width; i++)
                {
                    for (int j = 0; j < texture_height; j++)
                    {
                        for (int ph = 0; ph < OneTimePhoton; ph++)
                        {

                        }
                    }
                }
            }


            Debug.Log("Saving....");
            var bytes = tex.EncodeToPNG();
            File.WriteAllBytes(Path.Combine(Application.dataPath, "img_PPM.png"), bytes);
            time.Stop();
            Debug.Log("Ray tracing 執行 " + time.Elapsed.TotalSeconds + " 秒");
        }
    }


    void RayCastPass(Ray INPUT, int texture_x, int texture_y, int texture_sample)
    {
        RaycastHit MYRayHit;
        if (Physics.Raycast(INPUT, out MYRayHit, 2000f))
        {
            Renderer renderer = MYRayHit.collider.GetComponent<MeshRenderer>();

            if (MYRayHit.collider.tag == "Light")//done
            {
                HitPoint_position[texture_x, texture_y, texture_sample] = MYRayHit.point;


            }
            else if (MYRayHit.collider.tag == "lambertian")
            {
                HitPoint_position[texture_x, texture_y, texture_sample] = MYRayHit.point;
            }
            else if (MYRayHit.collider.tag == "metal")
            {
                float fuzz = 0.05f;
                Vector3 reflected = Vector3.Reflect(INPUT.direction.normalized, MYRayHit.normal);
                Ray scattered = new Ray(MYRayHit.point, reflected + fuzz * new Vector3(Random.Range(-1, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized);
                if (Vector3.Dot(scattered.direction, MYRayHit.normal) > 0)
                {
                    RayCastPass(scattered, texture_x, texture_y, texture_sample);
                }
            }
            else if (MYRayHit.collider.tag == "dielectric")
            {
                Vector3 out_normal = Vector3.zero;
                Ray scattered = new Ray(new Vector3(0, 0, 0), new Vector3(0, 0, 1));
                Vector3 reflected = reflect(INPUT.direction, MYRayHit.normal);
                float reffactor = 1.5f;
                float realfactor = 1.5f;
                Vector3 refracted = Vector3.zero;
                float reflectp = 0f;
                float cosine = 0f;
                if (Vector3.Dot(INPUT.direction, MYRayHit.normal) > 0)
                {
                    out_normal = -MYRayHit.normal;
                    realfactor = reffactor;
                    cosine = reffactor * Vector3.Dot(INPUT.direction, MYRayHit.normal) / INPUT.direction.magnitude;
                }
                else
                {
                    out_normal = MYRayHit.normal;
                    realfactor = 1.0f / reffactor;
                    cosine = -Vector3.Dot(INPUT.direction, MYRayHit.normal) / INPUT.direction.magnitude;
                }
                if (refract(INPUT.direction, out_normal, realfactor, ref refracted))
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
                RayCastPass(scattered, texture_x, texture_y, texture_sample);
            }







        }

    }

    void PhotonPass()
    {
        for (int i = 0; i < OneTimePhoton; i++)
        {

        }
    }


    //Color RHit(Ray Input, int BoundTime)
    //{
    //    RaycastHit MYRayHit;
    //    Color color = Color.black;

    //    //if (BoundTime >= texture_boundtime)
    //    //{
    //    //    Vector3 Ldir = ThisLight.GetComponent<Transform>().position - Input.origin;
    //    //    Ray NRay = new Ray(Input.origin, Ldir.normalized);
    //    //    if (Physics.Raycast(NRay, out MYRayHit, 2000f))
    //    //    {
    //    //        if (MYRayHit.collider.tag == "Light")//done
    //    //        {
    //    //            return Color.white * 4;
    //    //        }
    //    //    }
    //    //    return Color.black;
    //    //}


    //    if (Physics.Raycast(Input, out MYRayHit, 2000f) && (BoundTime < texture_boundtime))
    //    {
    //        Renderer renderer = MYRayHit.collider.GetComponent<MeshRenderer>();
    //        Ray scattered = new Ray();
    //        Color attenuation = Color.black;
    //        Color emitted = Color.black;

    //        if (MYRayHit.collider.tag == "Light")//done
    //        {

    //            return Color.white * 6;
    //        }
    //        else if (MYRayHit.collider.tag == "lambertian")
    //        {
    //            emitted = Color.black;
    //            Vector3 scatter_direction = MYRayHit.normal + new Vector3(Random.Range(-1, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    //            scattered = new Ray(MYRayHit.point, scatter_direction);
    //            getPixelColor(MYRayHit, ref attenuation);
    //            attenuation *= renderer.material.color;
    //        }
    //        else if (MYRayHit.collider.tag == "metal")
    //        {
    //            float fuzz = 0.1f;
    //            emitted = Color.black;
    //            Vector3 reflected = Vector3.Reflect(Input.direction.normalized, MYRayHit.normal);
    //            scattered = new Ray(MYRayHit.point, reflected + fuzz * new Vector3(Random.Range(-1, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized);
    //            getPixelColor(MYRayHit, ref attenuation);
    //            attenuation *= renderer.material.color;
    //            if (Vector3.Dot(scattered.direction, MYRayHit.normal) > 0)
    //            {

    //            }
    //            else
    //            {
    //                return Color.black;
    //            }
    //        }
    //        else if (MYRayHit.collider.tag == "dielectric")
    //        {
    //            Vector3 out_normal = Vector3.zero;
    //            Vector3 reflected = reflect(Input.direction, MYRayHit.normal);
    //            float reffactor = 1.5f;
    //            float realfactor = 1.5f;
    //            attenuation = Color.white;
    //            Vector3 refracted = Vector3.zero;
    //            float reflectp = 0f;
    //            float cosine = 0f;
    //            if (Vector3.Dot(Input.direction, MYRayHit.normal) > 0)
    //            {
    //                out_normal = -MYRayHit.normal;
    //                realfactor = reffactor;
    //                cosine = reffactor * Vector3.Dot(Input.direction, MYRayHit.normal) / Input.direction.magnitude;
    //            }
    //            else
    //            {
    //                out_normal = MYRayHit.normal;
    //                realfactor = 1.0f / reffactor;
    //                cosine = -Vector3.Dot(Input.direction, MYRayHit.normal) / Input.direction.magnitude;
    //            }
    //            if (refract(Input.direction, out_normal, realfactor, ref refracted))
    //            {
    //                float r = (1f - reffactor) / (1f + reffactor);
    //                r = r * r;
    //                reflectp = r + (1 - r) * Mathf.Pow((1 - cosine), 5);
    //            }
    //            else
    //            {
    //                scattered = new Ray(MYRayHit.point, reflected);
    //                reflectp = 1.0f;
    //            }
    //            if (Random48.Get() < reflectp)
    //            {
    //                scattered = new Ray(MYRayHit.point, reflected);
    //            }
    //            else
    //            {
    //                scattered = new Ray(MYRayHit.point, refracted);
    //            }
    //            emitted = Color.black;
    //        }
    //        return emitted + attenuation * RHit(scattered, ++BoundTime);
    //    }
    //    else
    //    {

    //    }
    //}
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
    void getPixelColor(RaycastHit hit, ref Color pixelColor)
    {
        if (hit.collider != null && hit.collider.GetComponent<MeshRenderer>() != null)
        {
            MeshRenderer render = hit.collider.GetComponent<MeshRenderer>();
            pixelColor = Color.black;
            if (render.material != null)
            {
                if (render.material.mainTexture != null)
                {
                    Texture2D tex = render.material.mainTexture as Texture2D;
                    Vector2 uv = hit.textureCoord;
                    uv.x *= tex.width;
                    uv.y *= tex.height;

                    Vector2 tiling = render.material.mainTextureScale;
                    pixelColor = tex.GetPixel(Mathf.FloorToInt(uv.x * tiling.x), Mathf.FloorToInt(uv.y * tiling.y));

                }
                else
                {
                    pixelColor = render.material.GetColor("_Color");
                }
                //Debug.Log("Pixel Color: " + render.material.GetColor("_Color"));
            }
        }
    }

}
