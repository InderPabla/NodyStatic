using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

public class CrystalMLP
{
    private IntPtr net;
    private int[] layers;

    [DllImport("CrystalNetworkAPI")]
    private static extern IntPtr CreateCrystalMLPAPI(int layerSize, int[] layers, float[] weights, float[] bias);

    [DllImport("CrystalNetworkAPI")]
    private static extern IntPtr FeedForwardCrystalMLPAPI(System.IntPtr net, float[] input);

    [DllImport("CrystalNetworkAPI")]
    private static extern int DestroyCrystalMLPAPI(System.IntPtr net);

    public CrystalMLP(int layerSize, int[] layers, float[] weights, float[] bias)
    {
        this.layers = layers;
        net = CreateCrystalMLPAPI(layerSize, layers, weights, bias);
    }

    public float[] FeedForward(float[] input)
    {
        IntPtr floatPtr = FeedForwardCrystalMLPAPI(this.net, input);
        int numberOfOutputNeurons = this.layers[this.layers.Length - 1];
        float[] output = new float[numberOfOutputNeurons];
        Marshal.Copy(floatPtr, output, 0, numberOfOutputNeurons);
        return output;
    }

    public unsafe float* _Unsafe_FeedForward(float[] input)
    {
        IntPtr floatPtr = FeedForwardCrystalMLPAPI(this.net, input);
        return (float*)floatPtr;
    }

    public void Destroy()
    {
        DestroyCrystalMLPAPI(net);
        net = IntPtr.Zero;
        layers = null;
    }
}
