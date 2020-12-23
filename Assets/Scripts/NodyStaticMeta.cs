using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Reflection;
using System;
using System.Runtime.Serialization;

[Serializable]
public class NodyActorMeta
{
    [NonSerialized]
    public Vector2 InitialSpawnOffset = Vector2.zero;
    [NonSerialized]
    public NodyNodeStaticMeta _Other;

    public int _OtherNodeIndex = -1;
    public S2 SInitialSpawnOffset = new S2(Vector2.zero);
    public float InitialUnitVectorDegree;

   
    private float _BaseSize;

    /**
     * Random actor location
     */
    public NodyActorMeta(NodyNodeStaticMeta otherNode)
    {
        InitialUnitVectorDegree = (float)NodyData.Rand.NextDouble() * 360f;
        _Other = otherNode;
        _OtherNodeIndex = _Other != null ? _Other.NodeIndex : -1;
        BaseSize = NodyData.RandFloat(NodyData.MinSize, NodyData.MaxSize);
    }

    /**
     * Random actor location
     */
    public NodyActorMeta(NodyNodeStaticMeta otherNode, float minDegree, float maxDegree)
    {
        InitialUnitVectorDegree = NodyData.RandFloat(minDegree, maxDegree);
        _Other = otherNode;
        _OtherNodeIndex = _Other != null ? _Other.NodeIndex : -1;
        BaseSize = NodyData.RandFloat(NodyData.MinSize, NodyData.MaxSize);
    }

    /**
     * Random actor location
     */
    public NodyActorMeta(NodyNodeStaticMeta otherNode, NodyActorMeta copy)
    {
        InitialUnitVectorDegree = copy.InitialUnitVectorDegree;
        _Other = otherNode;
        _OtherNodeIndex = _Other!=null ? _Other.NodeIndex : -1;
        BaseSize = copy.BaseSize;
    }

    [OnDeserialized()]
    internal void OnDeserializedMethod(StreamingContext context)
    {
        InitialSpawnOffset = SInitialSpawnOffset.Vector2;
    }

    public float BaseSize
    {
        get
        {
            return _BaseSize;
        }
        set
        {
            _BaseSize = value;
            CalculateOffset();
        }
    }

    public Vector2 WorldspaceLocation(Vector2 WorldspaceBody, float WorldspaceAngleDegree)
    {
        Vector2 LocalPos = Quaternion.AngleAxis(WorldspaceAngleDegree, Vector3.forward) * InitialSpawnOffset.normalized * InitialSpawnOffset.magnitude;
        return WorldspaceBody + LocalPos;
    }

    public void CalculateOffset()
    {
        if (_Other != null)
        {
            InitialSpawnOffset = _Other.InitialSpawnOffset + (InitialUnitVector * (_Other._BaseSize + _BaseSize));
            SInitialSpawnOffset = new S2(InitialSpawnOffset);
        }
    }

    public Vector2 InitialUnitVector
    {
        get
        {
            return Quaternion.Euler(0, 0, InitialUnitVectorDegree) * Vector2.right;
        }
    }
}

[Serializable]
public class NodyEyeMeta : NodyActorMeta
{
    public float ViewDistance;
    public float EyeRotationPerturbationDegree;

    //----------------------
    [NonSerialized]
    public float CurrentEyeViewDistance01 = -1;
    [NonSerialized]
    public Vector2 HitEnd;
    [NonSerialized]
    public Vector2 HitStart;
    [NonSerialized]
    public Vector2 WorldLookDirectionPerturbed;
    [NonSerialized]
    public float WorldEyeDirectionDegreePerturbed;
    [NonSerialized]
    public int EyeTickCount;

    /**
     * Random Eye location and view distance
     */
    public NodyEyeMeta(NodyNodeStaticMeta otherNode) : base(otherNode)
    {
        ViewDistance = NodyData.RandFloat(NodyData.MinViewDistance, NodyData.MaxViewDistance);
        BaseSize = NodyData.EyeSize;
        EyeRotationPerturbationDegree = NodyData.RandFloat(NodyData.InitialMinEyePerturbationDegree, NodyData.InitialMaxEyePerturbationDegree);
    }

    /**
    * Random Eye location and view distance
    */
    public NodyEyeMeta(NodyNodeStaticMeta otherNode, float minDegree, float maxDegree) : base(otherNode, minDegree,maxDegree)
    {
        ViewDistance = NodyData.RandFloat(NodyData.MinViewDistance, NodyData.MaxViewDistance);
        BaseSize = NodyData.EyeSize;
        EyeRotationPerturbationDegree = NodyData.RandFloat(NodyData.InitialMinEyePerturbationDegree, NodyData.InitialMaxEyePerturbationDegree);
    }

    /**
     * Copy Eye
     */
    public NodyEyeMeta(NodyNodeStaticMeta otherNode, NodyEyeMeta copy) : base(otherNode, copy)
    {
        ViewDistance = copy.ViewDistance;
        EyeRotationPerturbationDegree = copy.EyeRotationPerturbationDegree;
    }


    public void UpdateHitStart(Vector2 WorldspaceBody, float WorldspaceAngleDegree)
    {
        WorldEyeDirectionDegreePerturbed = WorldspaceAngleDegree + EyeRotationPerturbationDegree;
        WorldLookDirectionPerturbed = Quaternion.AngleAxis(WorldEyeDirectionDegreePerturbed, Vector3.forward) * (InitialSpawnOffset-_Other.InitialSpawnOffset).normalized;
        Vector2 LocalEyePos = Quaternion.AngleAxis(WorldspaceAngleDegree, Vector3.forward) * InitialSpawnOffset.normalized * InitialSpawnOffset.magnitude;
        HitStart = WorldspaceBody + LocalEyePos;
    }
}


[Serializable]
public class NodyShellMeta : NodyActorMeta
{
    public float ShellHealth;
    
    /**
     * Random Shell locatio
     */
    public NodyShellMeta(NodyNodeStaticMeta otherNode) : base(otherNode)
    {
        BaseSize = NodyData.ShellSize;
        ShellHealth = NodyData.MaxShellHealth;
    }

    /**
     * Copy shell
     */
    public NodyShellMeta(NodyNodeStaticMeta otherNode, NodyShellMeta copy) : base(otherNode, copy)
    {
        ShellHealth = NodyData.MaxShellHealth;
    }

    public void ApplyDamage(float Multiplier)
    {
        ShellHealth -= NodyData.EnergryLoss_ShellDamage * Multiplier;
        if (ShellHealth < 0f) ShellHealth = 0f;
    }

    public bool IsDead
    {
        get
        {
            return ShellHealth <= 0f;
        }
    }
}

[Serializable]
public class NodySpikeMeta : NodyActorMeta
{

    public NodySpikeMeta(NodyNodeStaticMeta otherNode) : base(otherNode)
    {
        BaseSize = NodyData.SpikeSize;
    }


    public NodySpikeMeta(NodyNodeStaticMeta otherNode, NodySpikeMeta copy) : base(otherNode, copy)
    {

    }
}

[Serializable]
public class NodyNodeStaticMeta : NodyActorMeta
{
    public List<NodyEyeMeta> EyeList;
    public List<NodyShellMeta> ShellList;
    public List<NodySpikeMeta> SpikeList;

    public int NodeIndex;

    /**
     * Initial Random Node Meta Generation
     */
    public NodyNodeStaticMeta(int nodeIndex, NodyNodeStaticMeta otherNode, NodyType Type) : base(otherNode)
    {
        if (otherNode == null && nodeIndex != 0)
        {
            throw new System.Exception(string.Format("Non Zero Node Index:{0} must be a must have other Node Provided", nodeIndex));
        }

        NodeIndex = nodeIndex;

        if(!IsMasterNode)
        {
            BaseSize = (float)NodyData.BaseSize;
        }

        EyeList = new List<NodyEyeMeta>();
        int EyeCount = NodyData.RandInt(NodyData.MinEyesPerNode, NodyData.MaxEyesPerNode);
        for(int i = 0; i <EyeCount;i++)
        {
            NodyEyeMeta Eye = new NodyEyeMeta(this,NodyData.InitialMinEyeDegree, NodyData.InitialMaxEyeDegree);
            EyeList.Add(Eye);
        }

        ShellList = new List<NodyShellMeta>();
        int ShellCount = Type == NodyType.PREY ? NodyData.RandInt(NodyData.MinShellPerNode, NodyData.MaxShellPerNode) : 0;
        for (int i = 0; i < ShellCount; i++)
        {
            NodyShellMeta Shell = new NodyShellMeta(this);
            ShellList.Add(Shell);
        }

        SpikeList = new List<NodySpikeMeta>();
        int SpikeCount = Type == NodyType.PRED ? NodyData.RandInt(NodyData.MinSpikePerNode, NodyData.MaxSpikePerNode) : 0;
        for (int i = 0; i < SpikeCount; i++)
        {
            NodySpikeMeta Spike = new NodySpikeMeta(this);
            SpikeList.Add(Spike);
        }
    }

    /**
     * Initial Random Node Meta Generation
     */
    public NodyNodeStaticMeta(int nodeIndex, NodyNodeStaticMeta otherNode, NodyNodeStaticMeta copy) : base(otherNode, copy)
    {
        if (otherNode == null && nodeIndex != 0)
        {
            throw new System.Exception(string.Format("Non Zero Node Index:{0} must be a must have other Node Provided", nodeIndex));
        }

        NodeIndex = nodeIndex;

        EyeList = new List<NodyEyeMeta>();
        for (int i = 0; i < copy.EyeList.Count; i++)
        {
            NodyEyeMeta Eye = new NodyEyeMeta(this, copy.EyeList[i]);
            EyeList.Add(Eye);
        }

        ShellList = new List<NodyShellMeta>();
        for (int i = 0; i < copy.ShellList.Count; i++)
        {
            NodyShellMeta Spike = new NodyShellMeta(this, copy.ShellList[i]);
            ShellList.Add(Spike);
        }

        SpikeList = new List<NodySpikeMeta>();
        for (int i = 0; i < copy.SpikeList.Count; i++)
        {
            NodySpikeMeta Spike = new NodySpikeMeta(this, copy.SpikeList[i]);
            SpikeList.Add(Spike);
        }
    }

    public bool IsMasterNode
    {
        get
        {
            return NodeIndex == 0;
        }
    }
}

public enum NodyType
{
    NONE, PREY, PRED 
}

[Serializable]
public class NodyStaticMeta
{

    public List<NodyNodeStaticMeta> AllNodesList;

    [NonSerialized]
    public List<NodyEyeMeta> AllEyeNodesList;
    [NonSerialized]
    public List<NodyShellMeta> AllShellNodesList;
    [NonSerialized]
    public List<NodySpikeMeta> AllSpikeNodesList;

    [NonSerialized]
    public Vector2 MainBodySpawnPosition;
    [NonSerialized]
    public float MainBodyRotationDegree;

    //----------------------
    //Saveable Data

 

    [NonSerialized]
    public Color NodyColor;
    [NonSerialized]
    public float NodyHueColor;
    [NonSerialized]
    public Color EyeColor;
    [NonSerialized]
    public Color ShellColor;
    [NonSerialized]
    public Color SpikeColor;

    public S3 SNodyColor;
    public S3 SEyeColor;
    public S3 SShellColor;
    public S3 SSpikeColor;

    public string Name;

    public int ChildCount = 0;

    public float CoolDownBirthTime;
    public float Health;
    public float TimeLived;

    public NodyBrain NEAT_Decoder;
    public int _InputNeuronCount;
    public int _OutputNeuronCount;
    public float[] _Inputs;
    public float[] _Outputs;
    public int _OutputMemoryNeuronStartIndex;

    public int SpeciesId;

    public float TotalEnergyGainedFromFood;
    public NodyType Type;
    //----------------------

    public float Input_AngularTurnNeg11;
    public bool Input_IsAccessoryTouching;
    public bool Input_IsTouchingWall;

    public float Output_ForwardVelo01;
    public float Output_AngularVeloNeg11;
    public float Output_AngularVeloBrakeNeg11;
    public float Output_SpeedDeltaTime;
    public float Output_CreateLife;

   
    public float NetHealthLossPerTick;
    public float NetHealtGainPerTick;


    public float Calc_BodyMass;
    public float Calc_BodyMassPow2;
    public float Calc_BodyMassSqrt2;
    public float Calc_MaxHealthAllowed;
    public float Calc_StaticEnergyLoss;
    public float Calc_MaxForwardVelo;
    public float Calc_MaxAngularVelo;


    public string UUID;


    //private CrystalMLP MLP_Encoder;
    //private int[] _Layers;
    //private float[] _Bias;
    //private float[] _Weights;


    /**
     * Initial Random Meta Generation
     */
    public NodyStaticMeta(int speciesId)
    {
        Name = NodyData.GenerateNewNodyName();
        UUID = NodyData.UUID;
        SpeciesId = speciesId;

        Health = NodyData.InitialHealth;
        AllNodesList = new List<NodyNodeStaticMeta>();
        AllEyeNodesList = new List<NodyEyeMeta>();
        AllShellNodesList = new List<NodyShellMeta>();
        AllSpikeNodesList = new List<NodySpikeMeta>();

        NodyColor = Color.HSVToRGB((float)NodyData.Rand.NextDouble(), 1f, 1f);
        EyeColor = NodyData.DefaultEyeColor;
        ShellColor = NodyData.DefaultShellColor;
        SpikeColor = NodyData.DefaultSpikeColor;

        Type = (float)NodyData.Rand.NextDouble() > 0.5f?NodyType.PREY:NodyType.PRED;

        int NumberOfNodes = (int)((NodyData.Rand.NextDouble() * (NodyData.MaxNodes - NodyData.MinNodes)) + NodyData.MinNodes);
        MainBodyRotationDegree = (float)NodyData.Rand.NextDouble() * 360f;

        for (int i = 0; i < NumberOfNodes; i++)
        {
            NodyNodeStaticMeta Node;
            NodyNodeStaticMeta OtherNode = null;
            if (i > 0)
            {
                int MinIndex = 0;
                int MaxIndex = i - 1;
                int OtherNodeIndex = (int)((float)NodyData.Rand.NextDouble() * (float)(MaxIndex - MinIndex) + (float)MinIndex);
                OtherNode = AllNodesList[OtherNodeIndex];
            }
            Node = new NodyNodeStaticMeta(i, OtherNode, Type);
            AllNodesList.Add(Node);

            AllEyeNodesList.AddRange(Node.EyeList);
            AllShellNodesList.AddRange(Node.ShellList);
            AllSpikeNodesList.AddRange(Node.SpikeList);
        }


        BuildBrain();
        InitAll();
    }

    public NodyStaticMeta(NodyStaticMeta copy, bool ToMutate)
    {
        Name = copy.Name;//NodyData.GenerateNewNodyName();
        UUID = NodyData.UUID;
        Health = NodyData.InitialHealth;

        AllNodesList = new List<NodyNodeStaticMeta>();
        AllEyeNodesList = new List<NodyEyeMeta>();
        AllShellNodesList = new List<NodyShellMeta>();
        AllSpikeNodesList = new List<NodySpikeMeta>();
  
        SpeciesId = copy.SpeciesId;
        NodyColor = copy.NodyColor;
        EyeColor = copy.EyeColor;
        ShellColor = copy.ShellColor;
        SpikeColor = copy.SpikeColor;
        Type = copy.Type;

        int NumberOfNodes = copy.AllNodesList.Count;
        MainBodyRotationDegree = copy.MainBodyRotationDegree;

        for (int i = 0; i < NumberOfNodes; i++)
        {
            NodyNodeStaticMeta Node;
            NodyNodeStaticMeta OtherNode = null;

            if (i > 0)
            {
                int OtherNodeIndex = copy.AllNodesList.IndexOf(copy.AllNodesList[i]._Other);
                OtherNode = AllNodesList[OtherNodeIndex];
            }

            Node = new NodyNodeStaticMeta(i, OtherNode, copy.AllNodesList[i]);

            AllNodesList.Add(Node);
            AllEyeNodesList.AddRange(Node.EyeList);
            AllShellNodesList.AddRange(Node.ShellList);
            AllSpikeNodesList.AddRange(Node.SpikeList);
        }

        _OutputMemoryNeuronStartIndex = copy._OutputMemoryNeuronStartIndex;
        _InputNeuronCount = copy._InputNeuronCount;
        _OutputNeuronCount = copy._OutputNeuronCount;

        _Inputs = new float[_InputNeuronCount];
        _Outputs = new float[_OutputNeuronCount];



        /*_Layers = new int[2] { _InputNeuronCount, 10 };
        int WeightCount = _Layers[0] * _Layers[1];
        _Bias = new float[2];
        _Weights = new float[WeightCount];
        for (int i = 0; i < WeightCount; i++) _Weights[i] = copy._Weights[i];
        for (int i = 0; i < _Bias.Length; i++) _Bias[i] = copy._Bias[i];
        NEAT_Decoder = new NodyBrain(copy.NEAT_Decoder, ToMutate);*/

        NEAT_Decoder = new NodyBrain(copy.NEAT_Decoder, ToMutate);

        if (ToMutate) MutateBody();
        //MLP_Encoder = new CrystalMLP(_Layers.Length, _Layers, _Weights, _Bias);

        InitAll(); 
    }

    private void InitAll()
    {
        InitVariables();
        InitEnergy();
        InitSerialized();
    }

    private void InitVariables()
    {
        float H,S,V;
        Color.RGBToHSV(NodyColor,out H,out S, out V);
        NodyHueColor = H;
    }

    private void InitSerialized()
    {
        SNodyColor = new S3(NodyColor);
        SEyeColor = new S3(EyeColor);
        SShellColor = new S3(ShellColor);
        SSpikeColor = new S3(SpikeColor);

    }


    [OnDeserialized()]
    internal void OnDeserializedMethod(StreamingContext context)
    {
        NodyColor = SNodyColor.Color;
        EyeColor = SEyeColor.Color;
        ShellColor = SShellColor.Color;
        SpikeColor = SSpikeColor.Color;


        AllEyeNodesList = new List<NodyEyeMeta>();
        AllShellNodesList = new List<NodyShellMeta>();
        AllSpikeNodesList = new List<NodySpikeMeta>();


        for (int i = 0; i < AllNodesList.Count; i++)
        {
            if (AllNodesList[i]._OtherNodeIndex >= 0)
                AllNodesList[i]._Other = AllNodesList[AllNodesList[i]._OtherNodeIndex];
            
            AllNodesList[i].BaseSize = AllNodesList[i].BaseSize;

            for (int j = 0; j < AllNodesList[i].EyeList.Count; j++)
            {
                if (AllNodesList[i].EyeList[j]._OtherNodeIndex >= 0)
                    AllNodesList[i].EyeList[j]._Other = AllNodesList[AllNodesList[i].EyeList[j]._OtherNodeIndex];
                AllNodesList[i].EyeList[j].BaseSize = AllNodesList[i].EyeList[j].BaseSize;
            }

            for (int j = 0; j < AllNodesList[i].ShellList.Count; j++)
            {
                if (AllNodesList[i].ShellList[j]._OtherNodeIndex >= 0)
                    AllNodesList[i].ShellList[j]._Other = AllNodesList[AllNodesList[i].ShellList[j]._OtherNodeIndex];
                AllNodesList[i].ShellList[j].BaseSize = AllNodesList[i].ShellList[j].BaseSize;
            }

            for (int j = 0; j < AllNodesList[i].SpikeList.Count; j++)
            {
                if (AllNodesList[i].SpikeList[j]._OtherNodeIndex >= 0)
                    AllNodesList[i].SpikeList[j]._Other = AllNodesList[AllNodesList[i].SpikeList[j]._OtherNodeIndex];
                AllNodesList[i].SpikeList[j].BaseSize = AllNodesList[i].SpikeList[j].BaseSize;
            }

            AllEyeNodesList.AddRange(AllNodesList[i].EyeList);
            AllShellNodesList.AddRange(AllNodesList[i].ShellList);
            AllSpikeNodesList.AddRange(AllNodesList[i].SpikeList);
        }
 
        if(Type==NodyType.NONE)
        {
            Type = AllSpikeNodesList.Count == 0?NodyType.PREY:NodyType.PRED;
        }


        InitVariables();
        InitEnergy();
    }


    /**
     * Initilize a random network
     */
    private void BuildBrain()
    {
        int HealthNeuron = 1;
        int IsAccessoryTouchingNeuron = 1;
        int NodyControlNeurons = 0; //[Current Angle]

        //5 = [Hit Distance, Angle of hit, Hit Type 1, Hit Type 2, Is Same Species]
        //4 = [Hit Distance, Hit Type 1, Hit Type 2, Is Same Species]
        //4 = [Hit Distance, Angle of hit, Hit Type 1,  Is Same Species]
        int NeuronsPerEye = 4;
        int NodyEyeNeurons = AllEyeNodesList.Count * NeuronsPerEye;

        //2 = //[Has Hit, Angle of hit]
        //1 = [Has Hit]
        int NeuronsPerNody = 1;
        int NodyNodeNeurons = AllNodesList.Count * NeuronsPerNody; 

        int BiasNeuron = 1;
        _InputNeuronCount = HealthNeuron  + IsAccessoryTouchingNeuron + NodyControlNeurons 
                            + NodyEyeNeurons + NodyNodeNeurons 
                            + BiasNeuron + NodyData.MemoryNeurons;

        int BodyAccelForward = 1; //Froward Velo
        int BodyAngularVelos = 2; //Break and Angular Velo
        int BodySpeedTimeDelta = 1;
        int GiveBrith = 1;
        _OutputNeuronCount = BodyAccelForward + BodyAngularVelos + BodySpeedTimeDelta + GiveBrith + NodyData.MemoryNeurons;
        _OutputMemoryNeuronStartIndex = _OutputNeuronCount - NodyData.MemoryNeurons;

        _Inputs = new float[_InputNeuronCount];
        _Outputs = new float[_OutputNeuronCount];

        /*_Layers = new int[2] { _InputNeuronCount, 10 };
        int WeightCount = _Layers[0] * _Layers[1];
        _Bias = new float[2];
        _Weights = new float[WeightCount];
        for (int i = 0; i < WeightCount; i++) _Weights[i] = NodyData.RandFloat(NodyData.MinInitialWeight, NodyData.MaxInitialWeight);
        for (int i = 0; i < _Bias.Length; i++) _Bias[i] = NodyData.RandFloat(NodyData.MinInitialWeight, NodyData.MaxInitialWeight);
        MLP_Encoder = new CrystalMLP(_Layers.Length, _Layers, _Weights, _Bias);
        NEAT_Decoder = new NodyBrain(_Layers[_Layers.Length - 1], _OutputNeuronCount);*/

        NEAT_Decoder = new NodyBrain(_InputNeuronCount, _OutputNeuronCount);
    }

    /**
     * TODO: Params could be converted into a struct and passed in
     * TODO: Energy + Health Concept?
     * Update Health
     */
    public void ApplyEnergyCostForTick()
    {
        if (IsDead) return;

        float ExtraScaticEnergyScalarLoss = 1f;
        if(Health<NodyData.InitialHealth) ExtraScaticEnergyScalarLoss += (1f - (Health / NodyData.InitialHealth));
 
        NetHealthLossPerTick = 0;
        NetHealtGainPerTick = 0;

        NetHealthLossPerTick += Calc_StaticEnergyLoss * ExtraScaticEnergyScalarLoss; //Static

        //Dynamic
        NetHealthLossPerTick += NodyData.EnergyLoss_NetworkFired;
        NetHealthLossPerTick += NodyData.EnergyLoss_WallTouch * (Input_IsTouchingWall ? 1 : 0); //Energy Loss If Touching Wall
        NetHealthLossPerTick += NodyData.EnergyLoss_SpeedDeltaTime * (Output_SpeedDeltaTime < 0 ? 0 : Output_SpeedDeltaTime);

        Health += (NetHealtGainPerTick - NetHealthLossPerTick);

        TimeLived += Time.fixedDeltaTime;

        CoolDownBirthTime += Time.fixedDeltaTime;

        UpdateHealth();
    }

    private void UpdateHealth()
    {
        if (Health > Calc_MaxHealthAllowed)
            Health = Calc_MaxHealthAllowed;
    }

    public void UpdateGainedSpikeDamage(NodyStaticMeta Other)
    {
        float Gained = NodyData.EnergyGain_OtherNode * Calc_BodyMass;
        Health += Gained * NodyData.EnergyGain_OtherNodeFractionGain;
        Other.Health -= Gained;
        TotalEnergyGainedFromFood += Gained;
        UpdateHealth();
    }

    public void UpdateGiveBirth(float EnergyLoss)
    {
        CoolDownBirthTime = 0f;
        Health -= EnergyLoss;
        UpdateHealth();
    }

    public void UpdateAteFood()
    {
        float GainFood = Type==NodyType.PREY ? NodyData.EnergyGain_PreyFoodEaten : NodyData.EnergyGain_PredFoodEaten;
        float Gained = GainFood - (NodyData.EnergryLoss_FoodEatenSpikeCount * AllSpikeNodesList.Count);
        Gained = Gained < 0f ? 0f : Gained;
        Health += Gained;
        TotalEnergyGainedFromFood += Gained;
        UpdateHealth();
    }

    public void UpdateLostSpeciesAttackPeneltyDamage()
    {
        Health -= NodyData.EnergyLoss_AttackedSpecies;
        Health -= 100f;
    }

    private void InitEnergy()
    {
        float EyeEnergyLoss = 0f;
        Calc_BodyMass = 0f;

        for (int i = 0; i < AllEyeNodesList.Count; i++)
        {
            EyeEnergyLoss += NodyData.EnergyLoss_EyeUsedAtMaxViewRange * (AllEyeNodesList[i].ViewDistance - NodyData.MinViewDistance) / (NodyData.MaxViewDistance - NodyData.MinViewDistance);
        }

        for (int i = 0; i < AllNodesList.Count; i++)
        {
            Calc_BodyMass += Mathf.Pow(AllNodesList[i].BaseSize,2)*Mathf.PI;
        }

        Calc_BodyMassSqrt2 = Mathf.Sqrt(Calc_BodyMass);
        Calc_BodyMassPow2 = Mathf.Pow(Calc_BodyMass, 2f);

        Calc_MaxHealthAllowed = NodyData.MinHealthRequiredForBirth + (NodyData.InitialHealth * Calc_BodyMass) /2f;

        Calc_StaticEnergyLoss = 0f;
        Calc_StaticEnergyLoss += EyeEnergyLoss;
        Calc_StaticEnergyLoss += NodyData.EnergyLoss_PerShell * AllShellNodesList.Count;
        Calc_StaticEnergyLoss += NodyData.EnergyLoss_PerSpike * AllSpikeNodesList.Count;
        Calc_StaticEnergyLoss += NodyData.EnergyLoss_PerNode * AllNodesList.Count; 


        float ConnectionRatio = (float)NEAT_Decoder.ConnectionArr.Length / 400f;
        if (ConnectionRatio < 1f) ConnectionRatio = 0f;
        Calc_StaticEnergyLoss += NodyData.EnergyLoss_Connection * ConnectionRatio;

        Calc_MaxForwardVelo = NodyData.ForwardVeloScalar;///BodyMassSqrt2;
        Calc_MaxAngularVelo = NodyData.AngularVeloScalar;///BodyMassSqrt2;
    }

    /**
     * Fire network
     */
    public void FireNetworks()
    {
        _Outputs = NEAT_Decoder.FeedForward(_Inputs);

        //float[] _Out = MLP_Encoder.FeedForward(_Inputs);
        //_Outputs = NEAT_Decoder.FeedForward(_Out);

    }

    /**
     * Check if the Nody is dead
     */
    public bool IsDead
    {
        get
        {
            return Health <= 0f;
        }
    }


    public bool WantsToGiveBirth
    {
        get
        {
            return Output_CreateLife > NodyData.MinLifeCreateValue;
        }
    }

    public bool ShouldGiveBirth
    {
        get
        {
            return Input_CanGiveBirth && WantsToGiveBirth;
        }
    }

    public bool Input_CanGiveBirth
    {
        get
        {
            return TimeLived >= NodyData.MinTimeLiveBeforeBirth
                && TotalEnergyGainedFromFood > NodyData.MinEnergyGainedFromFoodBeforeBirth
                && Health >= NodyData.MinHealthRequiredForBirth
                 && CoolDownBirthTime >= NodyData.CoolDownBirthTime;
        }
    }

    /**
     * Destory
     * - Destory Network
     */
    public void Destroy()
    {
        
    }

    /**
     * Create a mutated child version of the Meta object
     */
    public NodyStaticMeta CreateMutatedCopyChild()
    {
        return new NodyStaticMeta(this, true);
    }

    /**
     * Create a exact copy child version of the Meta object
     */
    public NodyStaticMeta CreateExactCopyChild()
    {
        return new NodyStaticMeta(this, false);
    }

    /**
     * Mutate the params of this meta
     */
    private void MutateBody()
    {
        float h, s, v;
        Color.RGBToHSV(NodyColor, out h, out s, out v);
        NodyColor = Color.HSVToRGB(NodyData.MutateValue(h, 0f, 1f, NodyData.MaxValueIncreaseScalar, MutateType.HUE), s, v);

        foreach (NodyNodeStaticMeta NodeMeta in AllNodesList)
        {
            NodeMeta.BaseSize = NodyData.MutateValue(NodeMeta.BaseSize, NodyData.MinSize, NodyData.MaxSize, NodyData.MaxValueIncreaseScalar, MutateType.VALUE);

            NodeMeta.InitialUnitVectorDegree = NodyData.MutateValue(NodeMeta.InitialUnitVectorDegree, 0, 360, NodyData.MaxValueIncreaseScalar * 0.5f, MutateType.DEGREE);
            NodeMeta.CalculateOffset();

            foreach (NodyEyeMeta EyeMeta in NodeMeta.EyeList)
            {
                EyeMeta.InitialUnitVectorDegree = NodyData.MutateValue(EyeMeta.InitialUnitVectorDegree, NodyData.MinEyeDegree, NodyData.MaxEyeDegree, NodyData.MaxValueIncreaseScalar, MutateType.VALUE);
                EyeMeta.EyeRotationPerturbationDegree = NodyData.MutateValue(EyeMeta.EyeRotationPerturbationDegree, NodyData.MinEyePerturbationDegree, NodyData.MaxEyePerturbationDegree, NodyData.MaxValueIncreaseScalar, MutateType.VALUE);

                //EyeMeta.InitialUnitVectorDegree = NodyData.MutateValue(EyeMeta.InitialUnitVectorDegree, 0f, 360f, NodyData.MaxValueIncreaseScalar, MutateType.DEGREE);
                //EyeMeta.EyeRotationPerturbationDegree = NodyData.MutateValue(EyeMeta.EyeRotationPerturbationDegree, 0f, 360f, NodyData.MaxValueIncreaseScalar, MutateType.DEGREE);

                EyeMeta.ViewDistance = NodyData.MutateValue(EyeMeta.ViewDistance, NodyData.MinViewDistance, NodyData.MaxViewDistance, NodyData.MaxValueIncreaseScalar, MutateType.VALUE);
                EyeMeta.CalculateOffset();
            }

            foreach (NodyShellMeta ShellMeta in NodeMeta.ShellList)
            {
                ShellMeta.InitialUnitVectorDegree = NodyData.MutateValue(ShellMeta.InitialUnitVectorDegree, 0, 360, NodyData.MaxValueIncreaseScalar, MutateType.DEGREE);
                ShellMeta.CalculateOffset();
            }

            foreach (NodySpikeMeta SpikeMeta in NodeMeta.SpikeList)
            {
                SpikeMeta.InitialUnitVectorDegree = NodyData.MutateValue(SpikeMeta.InitialUnitVectorDegree, 0, 360, NodyData.MaxValueIncreaseScalar, MutateType.DEGREE);
                SpikeMeta.CalculateOffset();
            }
        }
    }


    public bool IsSameSpecies(NodyStaticMeta Other)
    {
        if (SpeciesId == Other.SpeciesId)
        {
            if(Mathf.Abs(NodyHueColor-Other.NodyHueColor)<=NodyData.MinHueDifferenceSpecies)
            {
                return true; 
            }
        }
        
        return false;
    }

    public NodyStaticMeta CreateChild()
    {
        NodyStaticMeta ChildMeta = new NodyStaticMeta(this, true);
        ChildCount++;
        return ChildMeta;
    }

}

/*[Serializable]
public class NodyDataSerilized {

    public string Name;
    public Color NodyColor;
    public Color EyeColor;
    public Color ShellColor;
    public Color SpikeColor;

    public float CoolDownBirthTime;
    public float Health;
    public float TimeLived;

    public int _InputNeuronCount;
    public int _OutputNeuronCount;
    public int _OutputMemoryNeuronStartIndex;

    public int SpeciesId;
    public int NodyId;

    public float TotalEnergyGainedFromFood;


    public NodyDataSerilized(NodyStaticMeta c)
    {
        Name = c.Name;
        NodyColor = c.NodyColor;
        EyeColor = c.EyeColor;
        ShellColor = c.ShellColor;
        SpikeColor = c.SpikeColor;
        CoolDownBirthTime = c.CoolDownBirthTime;
        Health = c.Health;
        TimeLived = c.TimeLived;
        _InputNeuronCount = c._InputNeuronCount;
        _OutputNeuronCount = c._OutputNeuronCount;
        _OutputMemoryNeuronStartIndex = c._OutputMemoryNeuronStartIndex;
        SpeciesId = c.SpeciesId;
        NodyId = c.NodyId;
        TotalEnergyGainedFromFood = c.TotalEnergyGainedFromFood;
    }
}*/


