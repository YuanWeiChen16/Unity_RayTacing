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
    reclight lights;
    // Start is called before the first frame update
    void Start()
    {
        lights = new reclight(ThisLight);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            //timer
            System.Diagnostics.Stopwatch time = new System.Diagnostics.Stopwatch();
            time.Start();
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
            time.Stop();
            Debug.Log("Ray tracing 執行 " + time.Elapsed.TotalSeconds + " 秒");
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
            float scatter_pdf = 1f;
            float pdf_val = 1f;
            float coef = 1f;
            if (MYRayHit.collider.tag == "Light")//done
            {
                return Color.white * 3;
            }
            else if (MYRayHit.collider.tag == "lambertian")
            {
                emitted = new Color(0.15f,0.15f,0.15f);
                //Vector3 scatter_direction = MYRayHit.normal + new Vector3(Random.Range(-1, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
                //scattered = new Ray(MYRayHit.point, scatter_direction);
                getPixelColor(MYRayHit, ref attenuation);
                attenuation *= renderer.material.color;
                Vector3[] onb = new Vector3[3];
                build_onb(MYRayHit.normal, ref onb);
                Vector3 cos_pdf = cos_pdf_generate(onb);
                Vector3 light_pdf = lights.generate(MYRayHit.point);

                Vector3 scatter_dir = (Random.Range(0f, 1f) < 0.5f) ? cos_pdf : light_pdf;
                scattered = new Ray(MYRayHit.point, scatter_dir.normalized);

                float cos = Vector3.Dot(scatter_dir.normalized, onb[2]);
                float cos_value = (cos > 0) ? (cos / 3.141592f) : 0;
                float light_value = lights.pdf_value(Input, MYRayHit.point, scatter_dir.normalized);
                pdf_val = 0.5f * cos_value + 0.5f * light_value;
                scatter_pdf = (cos < 0) ? 0 : (Vector3.Dot(MYRayHit.normal, scatter_dir.normalized) / 3.141592f);
                coef = scatter_pdf / pdf_val;
                if (coef > 1.2) coef = 1.2f;
                //scatter_pdf *= 1.2f;
            }
            else if (MYRayHit.collider.tag == "metal")
            {
                float fuzz = 0.01f;
                emitted = Color.black;
                Vector3 reflected = Vector3.Reflect(Input.direction.normalized, MYRayHit.normal);
                scattered = new Ray(MYRayHit.point, reflected + fuzz * Random48.random_cosine_direction());
                Vector3 outNor = MYRayHit.normal;
                getPixelNormal(MYRayHit,ref outNor);
                Vector3 reflectedn = Vector3.Reflect(Input.direction.normalized,  outNor);
                scattered = new Ray(MYRayHit.point, reflected + fuzz * new Vector3(Random.Range(-1, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized);
                getPixelColor(MYRayHit, ref attenuation);
                attenuation *= renderer.material.color;
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
                    //scattered = new Ray(MYRayHit.point, reflected);
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
            return emitted + attenuation * RHit(scattered, ++BoundTime) * coef;
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
    void build_onb(Vector3 n, ref Vector3[] onb)
    {
        onb[2] = n.normalized;
        Vector3 a;
        if (Mathf.Abs(onb[2].x) > 0.9)
            a = new Vector3(0, 1, 0);
        else a = new Vector3(1, 0, 0);
        onb[1] = Vector3.Cross(onb[2], a).normalized;
        onb[0] = Vector3.Cross(onb[2], onb[1]);
    }
    Vector3 cos_pdf_generate(Vector3[] onb)
    {
        Vector3 coef = random_cosine_direction();
        return coef.x * onb[0] + coef.y * onb[1] + coef.z * onb[2];
    }
    Vector3 random_cosine_direction()
    {
        float r1 = Random48.Get();
        float r2 = Random48.Get();
        float z = Mathf.Sqrt(1 - r2);
        float phi = 2 * 3.141592f * r1;
        float x = Mathf.Cos(phi) * Mathf.Sqrt(r2);
        float y = Mathf.Sin(phi) * Mathf.Sqrt(r2);
        return new Vector3(x, y, z);
    }
    void getPixelNormal(RaycastHit hit, ref Vector3 pixelNormal)
    {
        {
            if (hit.collider != null && hit.collider.GetComponent<MeshRenderer>() != null)
            {
                MeshRenderer render = hit.collider.GetComponent<MeshRenderer>();
                pixelNormal = new Vector3(0, 0, 1);
                if (render.material != null)
                {
                    if (render.material.GetTexture("_BumpMap") != null)
                    {
                        Texture2D tex = render.material.GetTexture("_BumpMap") as Texture2D;
                        Vector2 uv = hit.textureCoord;
                        uv.x *= tex.width;
                        uv.y *= tex.height;

                        Vector2 tiling = render.material.GetTextureScale("_BumpMap");
                        Color NC = tex.GetPixel(Mathf.FloorToInt(uv.x * tiling.x), Mathf.FloorToInt(uv.y * tiling.y));
                        NC.r = NC.r * 2 - 1;
                        NC.g = NC.g * 2 - 1;
                        NC.b = NC.b * 2 - 1;
                        pixelNormal = new Vector3(NC.r, NC.g, NC.b);
                    }
                    else
                    {
                        pixelNormal = hit.normal;
                    }
                    //Debug.Log("Pixel Color: " + render.material.GetColor("_Color"));
                }
            }
        }
    }
}

public class reclight
{
    public reclight(GameObject Light)
    {
        Bounds bound = Light.GetComponent<MeshFilter>().sharedMesh.bounds;
        max = new Vector2(bound.max.x, bound.max.z);
        min = new Vector2(bound.min.x, bound.min.z);
        k = Mathf.Abs(bound.max.y - bound.min.y);
    }
    public float pdf_value(Ray Input, Vector3 origin, Vector3 v)
    {
        RaycastHit rec;
        if (!Physics.Raycast(new Ray(origin, v), out rec, 100f))
            return 0;
        float t = rec.distance;
        float area = 60f;
        float distance_squared = t * t * v.sqrMagnitude;
        float cos = Mathf.Abs(Vector3.Dot(v, rec.normal)) / v.magnitude;
        return distance_squared / (cos * area);

    }

    public Vector3 random(Vector3 origin)
    {
        Vector3 point = new Vector3(Random.Range(min.x, max.x), k, Random.Range(min.y, max.y));
        return point - origin;
    }
    
    public Vector3 generate(Vector3 origin)
    {
        if (Random48.Get() < 0.5)
            return random(origin);
        else return random(origin);
    }
    Vector2 max = Vector2.zero;
    Vector2 min = Vector2.zero;
    float k = 0;
}
