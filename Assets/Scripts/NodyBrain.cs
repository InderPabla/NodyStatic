using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using System.Linq;
using System;

[Serializable]
public enum NodyBrainActivationType
{
    TANH, FTANH, DTANH, SIGMOID, DSIGMOID, SIN, COS
}

[Serializable]
public struct NodyBrainConnectionMeta
{
    public int NodeIndex;
    public int OtherNodeIndex;
    public float Weight;
    public bool IsActive;

    public NodyBrainConnectionMeta(int nodeIndex, int otherNodeIndex, float weight)
    {
        NodeIndex = nodeIndex;
        OtherNodeIndex = otherNodeIndex;
        Weight = weight;
        IsActive = true;
    }
}

[Serializable]
public struct NodyBrainNodeMeta
{
    public int NodeIndex;
    public float Value;
    public NodyBrainActivationType Type;

    public NodyBrainNodeMeta(int nodeIndex, NodyBrainActivationType type)
    {
        NodeIndex = nodeIndex;
        Value = 0;
        Type = type;
    }
}

[Serializable]
public class NodyBrain
{

    private int InputCount = 0;
    private int OutputCount = 0;
    private int HiddenIndex = 0;

    private List<NodyBrainNodeMeta> NodeList;
    private List<NodyBrainConnectionMeta> ConnectionList;

    public NodyBrainNodeMeta[] NodeArr;
    public NodyBrainConnectionMeta[] ConnectionArr;

    /**
     * Generate random brain given input and output counts
     */
    public NodyBrain(int inputCount, int outputCount)
    {

        InputCount = inputCount;
        OutputCount = outputCount;
        HiddenIndex = InputCount + OutputCount;

        NodeList = new List<NodyBrainNodeMeta>();
        ConnectionList = new List<NodyBrainConnectionMeta>();

        //Add Input and output nodes
        for (int i = 0; i < HiddenIndex; i++)
        {
            AddNode(i, NodyData.RandomActivation());
        }


        //Form connections between input and output nodes
        for (int outputIndex = InputCount; outputIndex < NodeList.Count; outputIndex++)
        {
            for (int otherIndex = 0; otherIndex < InputCount; otherIndex++)
            {
                if(NodyData.RandFloat(0f,1f)<=NodyData.Prob_InitialInputConnection)
                {
                    float weight = NodyData.RandFloat(NodyData.MinInitialWeight, NodyData.MaxInitialWeight);
                    AddConnectionBetweenNodes(outputIndex, otherIndex, weight);
                }
            }
        }
        
        

        for (int i = 0; i < NodyData.InitialMutationRounds; i++)
        {
            Mutate();
        }

        CompileNetwork();
    }

    public NodyBrain(NodyBrain copy, bool ToMutate)
    {
        InputCount = copy.InputCount;
        OutputCount = copy.OutputCount;
        HiddenIndex = InputCount + OutputCount;

        NodeList = new List<NodyBrainNodeMeta>();
        ConnectionList = new List<NodyBrainConnectionMeta>();

        for (int n = 0; n < copy.NodeList.Count; n++)
        {
            NodyBrainNodeMeta N = copy.NodeList[n];
            N.Value = 0;
            NodeList.Add(N);
        }

        for (int c = 0; c < copy.ConnectionList.Count; c++)
        {
            NodyBrainConnectionMeta C = copy.ConnectionList[c];
            ConnectionList.Add(C);
        }

        if (ToMutate)
        {
            Mutate();
        }

        CompileNetwork();
    }

    private NodyBrainNodeMeta AddNode(int NodeIndex, NodyBrainActivationType type)
    {
        NodyBrainNodeMeta N = new NodyBrainNodeMeta(NodeIndex, type);
        NodeList.Add(N);
        return N;
    }

    private NodyBrainConnectionMeta AddConnectionBetweenNodes(int NodeIndex, int OtherNodeIndex, float Weight)
    {
        NodyBrainConnectionMeta C = new NodyBrainConnectionMeta(NodeIndex, OtherNodeIndex, Weight);
        ConnectionList.Add(C);
        return C;
    }

    private void AddNodeBetweenExistingConnection(int conIndex)
    {
        if (NodeList.Count - HiddenIndex >= NodyData.MaxTotalHiddenNeuronCount)
        {
            return;
        }

        NodyBrainConnectionMeta Con = ConnectionList[conIndex];

        NodyBrainNodeMeta N = AddNode(NodeList.Count, NodyData.RandomActivation());

        AddConnectionBetweenNodes(N.NodeIndex, Con.OtherNodeIndex, Con.Weight);
        AddConnectionBetweenNodes(Con.NodeIndex, N.NodeIndex, 1f);
    }

    private void Mutate()
    {
        Dictionary<int, bool[]> NodeConDict = new Dictionary<int, bool[]>();

        int ConnectionListCount = ConnectionList.Count;

        float NewNodeProb = NodyData.Prob_CreateNewNode;
        float IncProb = NewNodeProb + NodyData.Prob_IncreaseWeight;
        float DecProb = IncProb + NodyData.Prob_DecreaseWeight;
        float RandProb = DecProb + NodyData.Prob_RandomizeWeight;
        float PosNegProb = RandProb + NodyData.Prob_FlipWeightPosNeg;
        float ToggleActiveProb = PosNegProb + NodyData.Prob_FlipWeightActive;

        for (int c = 0; c < ConnectionListCount; c++)
        {
            bool MutateConnection = (float)NodyData.Rand.NextDouble() <= NodyData.Prob_MutateWeight; 
            NodyBrainConnectionMeta C = ConnectionList[c];

            if (MutateConnection)
            {
                float ProbMutateType = (float)NodyData.Rand.NextDouble();

                if(ProbMutateType <= NewNodeProb)
                {
                    AddNodeBetweenExistingConnection(c);
                    C.IsActive = false;
                }
                else if(ProbMutateType <= IncProb)
                {
                    float inc = (float)NodyData.Rand.NextDouble() * NodyData.MaxWeightIncreaseScalar;
                    C.Weight = C.Weight + (C.Weight * inc);
                }
                else if (ProbMutateType <= DecProb)
                {
                    float inc = (float)NodyData.Rand.NextDouble() * NodyData.MaxWeightIncreaseScalar;
                    C.Weight = C.Weight - (C.Weight * inc);
                }
                else if (ProbMutateType <= RandProb)
                {
                    float weight = NodyData.RandFloat(NodyData.MinInitialWeight, NodyData.MaxInitialWeight);
                    C.Weight = weight;
                }
                else if (ProbMutateType <= PosNegProb)
                {
                    C.Weight *= -1f;
                }
                else if (ProbMutateType <= ToggleActiveProb)
                {
                    C.IsActive = !C.IsActive;
                }
            }

            ConnectionList[c] = C;
        }

        for (int n = 0; n < NodeList.Count; n++)
        {
            NodyBrainNodeMeta node = NodeList[n];

            if ((float)NodyData.Rand.NextDouble() < NodyData.Prob_MutateNodeActivationType)
            {
                node.Type = NodyData.RandomActivation();
                NodeList[n] = node;
            }

            NodeConDict.Add(n, new bool[NodeList.Count]);
        }

        for (int c = 0; c < ConnectionList.Count; c++)
        {
            NodyBrainConnectionMeta C = ConnectionList[c];

            if (!NodeConDict.ContainsKey(C.NodeIndex))
            {
                throw new System.Exception(string.Format("Node Index:{0} does not exist in dictionary", C.NodeIndex));
            }

            if (NodeConDict[C.NodeIndex][C.OtherNodeIndex])
            {
                throw new System.Exception(string.Format("Node Index:{0}, Other Node Index:{1} duplicate in the connection list.", C.NodeIndex, C.OtherNodeIndex));
            }

            NodeConDict[C.NodeIndex][C.OtherNodeIndex] = true;
        }

        for (int n = InputCount; n < NodeList.Count; n++)
        {
            if ((float)NodyData.Rand.NextDouble() <= NodyData.Prob_MakeConnectionWithAnotherNode)
            {
                int OtherNodeIndex = NodyData.RandInt(0, NodeList.Count - 1);
                if (!NodeConDict[n][OtherNodeIndex])
                {
                    NodeConDict[n][OtherNodeIndex] = true;
                    float weight = NodyData.RandFloat(NodyData.MinInitialWeight, NodyData.MaxInitialWeight);
                    AddConnectionBetweenNodes(n, OtherNodeIndex, weight);
                }
            }
        }

    }

    private void CompileNetwork()
    {
        NodeArr = NodeList.ToArray();

        ConnectionList.Sort((a, b) => {
            if (a.NodeIndex < b.NodeIndex)
            {
                return -1;
            }
            else if (a.NodeIndex > b.NodeIndex)
            {
                return 1;
            }
            else
            {
                if (a.OtherNodeIndex < b.OtherNodeIndex)
                {
                    return -1;
                }
                else if (a.OtherNodeIndex > b.OtherNodeIndex)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        });

        ConnectionArr = ConnectionList.FindAll(v => v.IsActive).ToArray();
    }

    public void Destroy()
    {

    }

    public float[] FeedForward(float[] Inputs)
    {

        float[] NodeValues = new float[NodeArr.Length];
        float[] Outputs = new float[OutputCount];

        for (int i = 0; i < InputCount; i++)
        {
            NodeValues[i] = Inputs[i];
            NodeArr[i].Value = Inputs[i];
        }

        for (int i = 0; i < ConnectionArr.Length; i++)
        {
            NodyBrainConnectionMeta C = ConnectionArr[i];
            NodeValues[C.NodeIndex] += (NodeArr[C.OtherNodeIndex].Value * C.Weight);

        }


        for (int i = InputCount; i < NodeArr.Length; i++)
        {
            NodeArr[i].Value = Activate(NodeValues[i], NodeArr[i].Type);
           //NodeArr[i].Value = Activate(NodeValues[i], NodyBrainActivationType.TANH);
        }

        int j = 0;
        for (int i = InputCount; i < InputCount + OutputCount; i++)
        {
            Outputs[j] = NodeArr[i].Value;
            j++;
        }

        return Outputs;
    }

    public static float Activate(float a, NodyBrainActivationType type)
    {
        switch (type)
        {
            case NodyBrainActivationType.TANH: return math.tanh(a);
            case NodyBrainActivationType.FTANH: return FastTanH(a);
            case NodyBrainActivationType.DTANH: return DTanH(a);
            case NodyBrainActivationType.SIGMOID: return Sigmoid(a);
            case NodyBrainActivationType.DSIGMOID: return DSigmoid(a);
            case NodyBrainActivationType.SIN: return Mathf.Sin(a);
            case NodyBrainActivationType.COS: return Mathf.Cos(a);
            
            default: return math.tanh(a);
        }
    }


    public static float FastTanH(float x)
    {
        if (x < -3)
            return -1;
        else if (x > 3)
            return 1;
        else
            return x * (27 + x * x) / (27 + 9 * x * x);
    }

    public static float Sigmoid(float value)
    {
        return 1.0f / (1.0f + Mathf.Exp(-value));
    }

    public static float DSigmoid(float value)
    {
        float D = 1.0f / (1.0f + Mathf.Exp(-value));
        return D * (1f- D);
    }

    public static float DTanH(float value)
    {
        float D = math.tanh(value);
        return 1f - Mathf.Pow(D, 2);
    }
}
