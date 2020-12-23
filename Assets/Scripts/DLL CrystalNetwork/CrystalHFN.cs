using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

public class CrystalHFN
{
    private System.IntPtr net;

    [DllImport("CrystalNetworkAPI")]
    private static extern IntPtr CreateCrystalHopfieldAPI(int numberOfNeurons, int inputSize, float[] initNeurons, float[] weights, float[] inputWeight);

    [DllImport("CrystalNetworkAPI")]
    private static extern IntPtr FeedForwardCrystalHopfieldAPI(System.IntPtr net, float[] input);

    [DllImport("CrystalNetworkAPI")]
    private static extern int DestroyCrystalHopfieldAPI(System.IntPtr net);

    private int numberOfNeurons;

    public CrystalHFN(int numberOfNeurons, int inputSize, float[] initNeurons, float[] weights, float[] inputWeight)
    {
        this.numberOfNeurons = numberOfNeurons;
        net = CreateCrystalHopfieldAPI(numberOfNeurons, inputSize, initNeurons, weights, inputWeight);
    }

    public float[] FeedForward(float[] input)
    {
        IntPtr floatPtr = FeedForwardCrystalHopfieldAPI(this.net, input);
        float[] output = new float[numberOfNeurons];
        Marshal.Copy(floatPtr, output, 0, numberOfNeurons);
        return output;
    }

    public unsafe float* _Unsafe_FeedForward(float[] input)
    {
        IntPtr floatPtr = FeedForwardCrystalHopfieldAPI(this.net, input);
        return (float*)floatPtr;
    }

    public void Destroy()
    {
        DestroyCrystalHopfieldAPI(net);
        net = IntPtr.Zero;
    }


}

