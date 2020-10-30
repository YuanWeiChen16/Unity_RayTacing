using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RAY : MonoBehaviour
{
    public int secondCount = 200;
    public Ray MyRay;
    public RaycastHit MYRayHit;
    public Vector3 MyRayHitStart;
    public Vector3 MyRayHitEnd;
    public Camera ThisCamera;
    public Ray[] SecondRay;
    public RaycastHit[] SecondRayHit;
    public Vector3 startPoint;
    bool status = false;
    // Start is called before the first frame update
    void Start()
    {
        MyRayHitStart = ThisCamera.GetComponent<Transform>().position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            startPoint.y += 0.5f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            startPoint.y -= 0.5f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            startPoint.x += 0.5f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            startPoint.x -= 0.5f;
        }

        MyRay = ThisCamera.ScreenPointToRay(startPoint);

        if (Physics.Raycast(MyRay, out MYRayHit, 2000f))
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                status = true;
                SecondRay = new Ray[secondCount];
                SecondRayHit = new RaycastHit[secondCount];
                for (int i = 0; i < secondCount; i++)
                {
                    SecondRay[i] = new Ray(MYRayHit.point, new Vector3(Random.Range(-1f, 1f) + MYRayHit.normal.x, Random.Range(-1f, 1f) + MYRayHit.normal.y, Random.Range(-1f, 1f) + MYRayHit.normal.z));
                    Physics.Raycast(SecondRay[i], out SecondRayHit[i], 5000f);
                }
            }
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(MyRayHitStart, MYRayHit.point);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(MyRay);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(MYRayHit.point, 0.25f);

        if(MYRayHit.collider != null && MYRayHit.collider.GetComponent<MeshRenderer>() != null)
        {
            MeshRenderer render = MYRayHit.collider.GetComponent<MeshRenderer>();
            Color pixelColor = Color.white;
            if (render.material != null)
            {
                if(render.material.mainTexture != null)
                {
                    Texture2D tex = render.material.GetTexture("_MainTex") as Texture2D;
                    Vector2 uv = MYRayHit.textureCoord;
                    uv.x *= tex.width;
                    uv.y *= tex.height;

                    Vector2 tiling = render.material.mainTextureScale;
                    pixelColor = tex.GetPixel(Mathf.FloorToInt(uv.x * tiling.x), Mathf.FloorToInt(uv.y * tiling.y));

                }
                else
                {
                    pixelColor = render.material.GetColor("_Color");
                }
                Debug.Log("Pixel Color: " + pixelColor);
            }
        }
        if(status)
        {
            for (int i = 0; i < secondCount; i++)
            {

                Gizmos.color = Color.white;
                Gizmos.DrawLine(MYRayHit.point, SecondRayHit[i].point);
                Gizmos.color = Color.cyan;
                Gizmos.DrawRay(SecondRay[i]);
                Gizmos.color = Color.yellow;
                if (SecondRayHit[i].collider.tag == "Light")
                {
                    Gizmos.color = Color.blue;
                }
                Gizmos.DrawSphere(SecondRayHit[i].point, 0.25f);
            }
        }
       
    }
}
