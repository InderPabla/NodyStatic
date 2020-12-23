using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public delegate void RemoveNody(NodyStatic Nody);
public delegate void RemoveFood(GameObject Food);
public delegate void CreateNody(NodyStaticMeta Meta, Vector3 Position);
public delegate Vector2 RandomPositionWithNoColliderUnder();

public enum MutateType
{
    DEGREE, HUE, VALUE
}


public class NodyData
{
    public static System.Random Rand;
    public static Material MainBodyMaterial;
    public static Material MainBodyLineMaterial;
    public static Material FoodMaterial;
    public static RemoveNody _RemoveNody;
    public static RemoveFood _RemoveFood;
    public static CreateNody _CreateNody;
    public static RandomPositionWithNoColliderUnder _RPWNCU;
    public static PhysicsMaterial2D SlipperyPhysicsMat;
    public static LayerMask BoundaryLayer;
    public static LayerMask FoodLayer;
    public static LayerMask NodyLayer;
    public static Canvas _Canvas;
    public static Font _TextFont;
    public static int _SpeciesId = 0;
    public static Color DefaultEyeColor = Color.white;
    public static Color DefaultShellColor = Color.HSVToRGB(0.064f, 0.94f, 0.98f); //Ecstasy Orange Color
    public static Color DefaultSpikeColor = Color.red;
    public static float BiasValue = 1f;
    public static float NodyMassScalar = 1.1f;

    public static string CreatureListFolder = "CreatureList";
    public static string GameStateUUID;
    public static string SessionUUID;
    public static string LoadSessionUUD;
    public static float MaxCameraOrthoSize = 130;
    public static float MinCameraOrthoSize = 5f;
    public static int Seed = 100;

    public static float FixedDeltaTime = 0.02f;


    public static float BaseSize = 0.75f;
    public static float MinSize = 0.75f;
    public static float MaxSize = 1.25f;
    public static float EyeSize = 0.2f;
    public static float FoodSize = 1.0f; //0.6
    public static float ShellSize = 0.4f; //0.2
    public static float SpikeSize = 0.2f; //0.2
   
 
    public static int InitialSpanwCount = 20;
    public static int MinFoodCount = 50;
    public static int MaxFoodCount = 50;
    public static int SpawnBoundary = 300;
    public static int MinSpawnBoundary = 5;
    public static int MaxSpawnBoundary = SpawnBoundary - MinSpawnBoundary;
    public static int[] BlockBoundaries = new int[] {50};
    public static int BoundarySeperators = 2;
    public static bool IsBoundaryWallTypeSim = true;

    public static int MaxBirthChildCount = 3;

    public static float MinViewDistance = 5f;
    public static float MaxViewDistance = 35f;

    public static float MaxThrustPower = 10f;
    public static float MinThrustPower = 1f;

    public static int MinNodes = 1;
    public static int MaxNodes = 4;

    public static int MaxEyesPerNode = 3;
    public static int MinEyesPerNode = 1;

    public static int MaxShellPerNode = 0;
    public static int MinShellPerNode = 6*3;

    public static int MaxSpikePerNode = 0;
    public static int MinSpikePerNode = 4*3;

    public static float InitialHealth = 100f;
    public static float MaxShellHealth = 20f;

    public static float EyeLineWidth = 0.1f;
    public static float ForwardLineWidth = 0.5f;

    public static float ForwardVeloScalar = 30f/1.2f;
    public static float AngularVeloScalar = 300f;
    public static float AngularDrag = 0.01f;
    public static float ForwardDrag = 0.01f;

    public static float EyeLineZIndex = -0.2f;
    public static float EyePosZIndex = -0.1f;
    public static float ShellPosZIndex = 0f;
    public static float SpikePosZIndex = 0.1f; //Push spike behind the Nody

    public static int MinModFireNet = 1;
    public static int MaxModFireNet = 1;

    public static float HideAssetsAtTimeScale = 4f;
    public static float HideNodyOutsideViewportPadding = 0.075f;

    public static float EnergyLoss_WallTouch = 20f;
    public static float EnergyLoss_NetworkFired = 0.001f;
    public static float EnergyLoss_EyeUsedAtMaxViewRange = 0.001f;
    public static float EnergyLoss_PerShell = 0.001f; 
    public static float EnergyLoss_PerSpike = 0.002f; 
    public static float EnergyLoss_PerNode = 0.002f;
    public static float EnergyLoss_SpeedDeltaTime = 0.002f;
    public static float EnergyGain_PreyFoodEaten = 20f;
    public static float EnergyGain_PredFoodEaten = 20f;
    public static float EnergyLoss_Brith = 100f; 
    public static float EnergyGain_OtherNode = 2f;
    public static float EnergyGain_OtherNodeFractionGain = 0.95f;
    public static float EnergyLoss_Connection = 0.001f/10f;
    public static float EnergryLoss_ShellDamage = 1f;
    public static float EnergryLoss_FoodEatenSpikeCount = 3f;
    public static float EnergyLoss_AttackedSpecies = EnergryLoss_FoodEatenSpikeCount;

    public static float MinTimeLiveBeforeBirth = 10f;
    public static float MinHealthRequiredForBirth = 120f;
    public static float MinEnergyGainedFromFoodBeforeBirth = 100f;
    public static float CoolDownBirthTime = 3f;
    public static float MinLifeCreateValue = 0f;

    public static int Animation_MaxLowHealthBlinkerCount = 5;
    public static int Animation_MinHealthBeforeBlinker = 25;


    public static float MaxInitialWeight = 1f;
    public static float MinInitialWeight = -1f;
    public static float MaxWeightIncreaseScalar = 0.1f;
    public static int InitialMutationRounds = 100/3;
    public static float Prob_InitialInputConnection = 100f/100f;
    public static int MaxTotalHiddenNeuronCount = 20;
    public static int MemoryNeurons = 0;

    public static float Prob_MutateWeight = 1f / 100f;
    public static float Prob_IncreaseWeight = 25f / 100f;//33
    public static float Prob_DecreaseWeight = 25f / 100f;//33
    public static float Prob_RandomizeWeight = 1f / 100f;//5
    public static float Prob_FlipWeightPosNeg = 1f / 100f;//5
    public static float Prob_FlipWeightActive = 16f / 100f;//5
    public static float Prob_CreateNewNode = 5f / 100f;//5

    public static float Prob_MakeConnectionWithAnotherNode = 5f / 100f;
    public static float Prob_MutateNodeActivationType = 1f / 100f;

    public static float Prob_MutateValue = 10f / 100f;
    public static float Prob_IncreaseValue = 33f / 100f;
    public static float Prob_DecreaseValue = 33f / 100f;
    public static float Prob_RandomizeValue = 1f / 100f;
    public static float Prob_FlipValuePosNeg = 1f / 100f;

    public static float MaxValueIncreaseScalar = 0.1f;


    public static float InitialMinEyeDegree = -45f;
    public static float InitialMaxEyeDegree = 45f;
    public static float MinEyeDegree = -45f;
    public static float MaxEyeDegree = 45f;
    public static float InitialMinEyePerturbationDegree = 0f;
    public static float InitialMaxEyePerturbationDegree = 0f;
    public static float MinEyePerturbationDegree = -10f;
    public static float MaxEyePerturbationDegree = 10f;
    public static int MaxEyeTickCount = 1;

    public static float MaxNodyWaitSpawnTime = 20f;
    public static float MaxFoodSpawnTime = 3f;
    public static float MaxFoodSpawnTimeIntervalYears = 50f;
    public static float MaxRecalculateSpeciesTime = 2f;
    public static float MinHueDifferenceSpecies = 1f;
    public static bool ShouldSameSpeciesAffectEachOther = false;


    /*public static NodyBrainActivationType[] AllowedActivations 
        = new NodyBrainActivationType[] {NodyBrainActivationType.TANH, NodyBrainActivationType.FTANH, NodyBrainActivationType.DTANH, NodyBrainActivationType.SIGMOID,
            NodyBrainActivationType.DSIGMOID};*/

    public static NodyBrainActivationType[] AllowedActivations 
        = new NodyBrainActivationType[] {NodyBrainActivationType.TANH};
    public static CollisionDetectionMode2D CollisionType = CollisionDetectionMode2D.Discrete;



    public static void InitStaticData(int seed, RemoveNody removeNody
                                    ,RemoveFood removeFood, CreateNody createNody, RandomPositionWithNoColliderUnder rPWNCU
        , Canvas canvas, Font textFont) {
        MainBodyMaterial = new Material(Shader.Find("Lightweight Render Pipeline/Unlit"));
        MainBodyLineMaterial = new Material(Shader.Find("Lightweight Render Pipeline/Unlit"));
        MainBodyLineMaterial.SetColor("_BaseColor", Color.white);
        FoodMaterial = new Material(Shader.Find("Lightweight Render Pipeline/Unlit"));
        FoodMaterial.SetColor("_BaseColor", Color.green);

        SlipperyPhysicsMat = new PhysicsMaterial2D("SlipperyMat");
        SlipperyPhysicsMat.bounciness = 0.5f;
        SlipperyPhysicsMat.friction = 0f;

        Seed = seed;
        Rand = new System.Random(Seed);

        _RemoveNody = removeNody;
        _RemoveFood = removeFood;
        _CreateNody = createNody;
        _RPWNCU = rPWNCU;
                                       
        BoundaryLayer = LayerMask.NameToLayer("Boundary");
        FoodLayer = LayerMask.NameToLayer("Food");
        NodyLayer = LayerMask.NameToLayer("Nody");

        _Canvas = canvas;
        _TextFont = textFont;
    }

    public static string GenerateNewNodyName()
    {
        string[] consonants = { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "l", "n", "p", "q", "r", "s", "sh", "zh", "t", "v", "w", "x" };
        string[] vowels = { "a", "e", "i", "o", "u", "ae", "y" };
        string Name = "";
        Name += consonants[Rand.Next(consonants.Length)].ToUpper();
        Name += vowels[Rand.Next(vowels.Length)];
        int b = 2; //b tells how many times a new letter has been added. It's 2 right now because the first two letters are already in the name.
        int maxLength = (int)((Rand.NextDouble() * 8f) + 4f);
        while (b < maxLength)
        {
            Name += consonants[Rand.Next(consonants.Length)];
            b++;
            Name += vowels[Rand.Next(vowels.Length)];
            b++;
        }

        return Name;
    }

    public static float MutateValue(float v, float min, float max, float IncreaseScalar, MutateType type)
    {

        float Prob = (float)NodyData.Rand.NextDouble();

        if (Prob <= NodyData.Prob_MutateValue)
        {
            Prob = (float)NodyData.Rand.NextDouble();
            float ProbUpdate = 0f;

            ProbUpdate += NodyData.Prob_IncreaseValue;
            if (Prob < ProbUpdate)
            {
                float inc = (float)NodyData.Rand.NextDouble() * IncreaseScalar;
                v = v + (v * inc);
                return PostMutateValue(v, min, max, type);
            }

            ProbUpdate += NodyData.Prob_DecreaseValue;
            if (Prob < ProbUpdate)
            {
                float inc = (float)NodyData.Rand.NextDouble() * IncreaseScalar;
                v = v - (v * inc);
                return PostMutateValue(v, min, max, type);
            }

            ProbUpdate += NodyData.Prob_RandomizeValue;
            if (Prob < ProbUpdate)
            {
                float value = NodyData.RandFloat(min, max);
                v = value;
                return PostMutateValue(v, min, max, type);
            }
            
            if(type == MutateType.VALUE)
            {
                ProbUpdate += NodyData.Prob_FlipValuePosNeg;
                if (Prob < ProbUpdate)
                {
                    v *= -1f;
                    return PostMutateValue(v, min, max, type);
                }
            }
            
        }

        return v;
    }

    public static float PostMutateValue(float v, float min, float max, MutateType type)
    {
        if(type==MutateType.DEGREE)
        {
            return UtilityMath.Clamp0To360(v);
        }
        else if(type==MutateType.HUE)
        {
            if (v < 0f) return v + 1f;
            else if (v > 1f) return v - 1f;
            else return v;
        }

        if (v < min) return min;
        else if (v > max) return max;
        else return v;
    }

    public static int RandInt(int Min, int Max)
    {
        return (int)((float)Rand.NextDouble() * ((Max + 1) - Min) + Min);
    }

    public static float RandFloat(float Min, float Max)
    {
        return (float)Rand.NextDouble() * (Max - Min) + Min;
    }

    public static int SpeciesId {
        get
        {
            int Id = _SpeciesId;
            _SpeciesId = _SpeciesId + 1;
            return Id;
        }
    }


    public static NodyBrainActivationType RandomActivation()
    {
        return AllowedActivations[RandInt(0, AllowedActivations.Length - 1)];
    }

    public static string UUID
    {
        get
        {
            return System.Guid.NewGuid().ToString().Substring(0, 8);
        }
    }
}
