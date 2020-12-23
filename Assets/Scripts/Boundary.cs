using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boundary : MonoBehaviour
{

    private Rigidbody2D Rig;
    private float TimeCounter = 0f;
    private float MaxTimeCount = 10f;

   
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if(!Rig.isKinematic)
        {
            if (TimeCounter == 0f)
            {
                Vector2 RandomUnitVector = Quaternion.Euler(0, 0, UnityEngine.Random.Range(0f, 360f)) * Vector2.right;
                float RandomVeloMag = UnityEngine.Random.Range(2f/4f, 6f/4f);
                Vector2 Velo = RandomUnitVector * RandomVeloMag;
                Rig.velocity = Velo;
                //Rig.angularVelocity = UnityEngine.Random.Range(-15f,15f);
            }

            TimeCounter += Time.fixedDeltaTime;

            if (TimeCounter >= MaxTimeCount)
            {
                TimeCounter = 0f;
            }
        }
        
    }

    public static Boundary Init(string Name, Vector3 Scale, Vector3 Position, GameObject Parent, Boundary Prefab, bool Simulated)
    {
        Boundary B = Instantiate(Prefab);
        B.name = Name;
        B.transform.localScale = Scale;
        B.transform.position = Position;
        B.transform.parent = Parent.transform;
        B.Rig = B.GetComponent<Rigidbody2D>();
        B.Rig.isKinematic = !Simulated;
        return B;
    }

    public static void BoundariesInit(Boundary BoundaryPrefab)
    {
        GameObject BoundaryParent = GameObject.Find("Boundary");

        Init("WallUp"
            , new Vector3(NodyData.SpawnBoundary + 1, 1, 1), new Vector3(NodyData.SpawnBoundary / 2, NodyData.SpawnBoundary, 0)
            , BoundaryParent, BoundaryPrefab, false);
        Init("WallDown"
           , new Vector3(NodyData.SpawnBoundary + 1, 1, 1), new Vector3(NodyData.SpawnBoundary / 2, 0, 0)
           , BoundaryParent, BoundaryPrefab, false);
        Init("WallLeft"
           , new Vector3(1, NodyData.SpawnBoundary, 1)
           , new Vector3(0, NodyData.SpawnBoundary / 2, 0)
           , BoundaryParent, BoundaryPrefab, false);
        Init("WallRight"
           , new Vector3(1, NodyData.SpawnBoundary, 1)
           , new Vector3(NodyData.SpawnBoundary, NodyData.SpawnBoundary / 2, 0)
           , BoundaryParent, BoundaryPrefab, false);

        BoundariesCreateWallSim(BoundaryPrefab, BoundaryParent);
        
    }

    

    public static void BoundariesToggle(Boundary BoundaryPrefab)
    {
        GameObject BoundaryParent = GameObject.Find("Boundary");
        NodyData.IsBoundaryWallTypeSim = !NodyData.IsBoundaryWallTypeSim;
        if(NodyData.IsBoundaryWallTypeSim)
        {
            BoundariesCreateWallSim(BoundaryPrefab, BoundaryParent);
        }
        else
        {
            BoundariesCreateWallSeperator(BoundaryPrefab,BoundaryParent);
        }
    }

    private static void BoundariesDeleteWallSeperator(GameObject BoundaryParent)
    {
        Boundary[] BoundaryList = BoundaryParent.transform.GetComponentsInChildren<Boundary>();
        foreach (Boundary B in BoundaryList)
        {
            if (B.name == "WallSeperator")
            {
                Destroy(B.gameObject);
            }
        }
    }

    private static void BoundariesDeleteWallSim(GameObject BoundaryParent)
    {
        Boundary[] BoundaryList = BoundaryParent.transform.GetComponentsInChildren<Boundary>();
        foreach (Boundary B in BoundaryList)
        {
            if (B.name == "WallSim")
            {
                Destroy(B.gameObject);
            }
        }
    }

    private static void BoundariesCreateWallSim(Boundary BoundaryPrefab, GameObject BoundaryParent)
    {
        BoundariesDeleteWallSeperator(BoundaryParent);
        for (int i = 0; i < NodyData.BlockBoundaries.Length; i++)
            Init("WallSim", Vector3.one * NodyData.BlockBoundaries[i], NodyData.SpawnBoundary * Vector3.one / 2, BoundaryParent, BoundaryPrefab, true);
    }

    private static void BoundariesCreateWallSeperator(Boundary BoundaryPrefab, GameObject BoundaryParent)
    {
        BoundariesDeleteWallSim(BoundaryParent);
        for (int i = 0; i < NodyData.BoundarySeperators; i++)
            Init("WallSeperator", 
                new Vector3(NodyData.SpawnBoundary + 1, 1, 1), 
                new Vector3(NodyData.SpawnBoundary / 2, ((float)NodyData.SpawnBoundary/ (float)NodyData.BoundarySeperators) *(i+1f), 0),
                BoundaryParent, BoundaryPrefab, false);

        for (int i = 0; i < NodyData.BoundarySeperators; i++)
            Init("WallSeperator",
                new Vector3(1, NodyData.SpawnBoundary, 1),
                new Vector3(((float)NodyData.SpawnBoundary / (float)NodyData.BoundarySeperators) * (i + 1f), NodyData.SpawnBoundary / 2, 0),
                BoundaryParent, BoundaryPrefab, false);
    }
}
