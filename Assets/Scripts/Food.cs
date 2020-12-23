using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{

    private VisualFrameType FrameType = VisualFrameType.NONE;
    private MeshRenderer FoodRen;
    private MeshFilter Filter;
    private Rigidbody2D Rig;
    float CurrentTime = 0f;
    float MaxTime = 10f;

    void FixedUpdate()
    {
        CurrentTime += Time.fixedDeltaTime;

        if(CurrentTime >= MaxTime)
        {
            ResetPosition();
        }
    }

    public void ResetTime()
    {
        CurrentTime = 0f;
    }

    public void ResetPosition()
    {
        ResetTime();
        transform.position = NodyData._RPWNCU();
        Rig.angularVelocity = 0f;
        Rig.velocity = Vector2.zero;
    }

    /*public void OnCollisionEnter2D(Collision2D Col)
    {
        ResetTime();
    }*/

    // Update is called once per frame
    void Update()
    {
        if (FrameType == VisualFrameType.SHOWN)
        {
            if (!ShouldShowFood())
            {
                FrameType = VisualFrameType.DESTROY;
            }
        }
        else if (FrameType == VisualFrameType.CREATE)
        {
            CreateVisualAssets();
            FrameType = VisualFrameType.SHOWN;
        }
        else if (FrameType == VisualFrameType.DESTROY)
        {
            DestroyVisualAssets();
            FrameType = VisualFrameType.HIDDEN;
        }
        else if (FrameType == VisualFrameType.HIDDEN)
        {
            if (ShouldShowFood())
            {
                FrameType = VisualFrameType.CREATE;
            }
        }
    }

    private void CreateVisualAssets()
    {
        FoodRen = gameObject.AddComponent<MeshRenderer>();
        FoodRen.sharedMaterial = NodyData.FoodMaterial;
        Filter = gameObject.AddComponent<MeshFilter>();
        Filter.sharedMesh = MeshGenerator.CircleMesh(2, NodyData.FoodSize);
    }

    private void DestroyVisualAssets()
    {

        Destroy(FoodRen);
        Destroy(Filter);
    }

    private bool IsFoodOnScreen
    {
        get
        {
            float Min = 0;
            float Max = 1f;
            Vector2 PosToViewport = Camera.main.WorldToViewportPoint(transform.position);
            return PosToViewport.x >= Min
                && PosToViewport.x <= Max
                && PosToViewport.y >= Min
                && PosToViewport.y <= Max;
        }
    }

    private bool ShouldShowFood()
    {
        return Time.timeScale < NodyData.HideAssetsAtTimeScale && IsFoodOnScreen;
    }

    public static Food Init (Vector3 SpawnPos, WorldManager Manager)
    {
        GameObject FoodObj = new GameObject();
        FoodObj.transform.parent = Manager.transform;
        FoodObj.name = "Food";
        Food F = FoodObj.AddComponent<Food>();
        FoodObj.layer = NodyData.FoodLayer;
        FoodObj.transform.position = SpawnPos;
        CircleCollider2D Col2d = FoodObj.AddComponent<CircleCollider2D>();
        Col2d.radius = NodyData.FoodSize;
        Col2d.isTrigger = false;

        F.Rig = FoodObj.AddComponent<Rigidbody2D>();
        F.Rig.mass = 0.01f;
        F.Rig.gravityScale = 0f;
        F.Rig.drag = 0.5f;
        F.Rig.angularDrag = 0.5f;

        F.FrameType = VisualFrameType.HIDDEN;
        return F;
    }
}
