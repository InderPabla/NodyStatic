using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
//using UnityEngine;

//Recurrent neural network
public class RNN 
{



    private int calculations;

    private int[] hiddenLayers;
    //private int[] hiddenLayerRNNFixed;

    private float[][] neurons;
    private float[][][] weights;


    private float BIAS = 0.25f;


    public RNN(int[] hiddenLayers)
    {


        this.hiddenLayers = new int[hiddenLayers.Length];
        //this.hiddenLayerRNNFixed = new int[hiddenLayers.Length];

        for (int i = 0; i < hiddenLayers.Length; i++)
        {
            this.hiddenLayers[i] = hiddenLayers[i];

            /*if (i < hiddenLayers.Length - 2)
            {
                this.hiddenLayerRNNFixed[i] = hiddenLayers[i] + hiddenLayers[i + 1];
            }
            else
            {
                this.hiddenLayerRNNFixed[i] = hiddenLayers[i];
            }

            if (i > 0)
                calculations += (hiddenLayers[i] * this.hiddenLayerRNNFixed[i - 1]);*/

        }

        InitilizeNeurons();
        InitilizeWeights();
       
    }

  
    private void InitilizeNeurons()
    {

        //Neuron Initilization
        /*List<float[]> neuronsList = new List<float[]>();

        for (int i = 0; i < hiddenLayerRNNFixed.Length; i++) //run through all layers
        {
            neuronsList.Add(new float[hiddenLayerRNNFixed[i]]); //add layer to neuron list
        }
        neurons = neuronsList.ToArray(); //convert list to array*/

        List<float[]> neuronsList = new List<float[]>();

        for (int i = 0; i < hiddenLayers.Length; i++) //run through all layers
        {
            neuronsList.Add(new float[hiddenLayers[i]]); //add layer to neuron list
        }
        neurons = neuronsList.ToArray(); //convert list to array
    }

    //create a static weights matrix
    private void InitilizeWeights()
    {
        //Weights Initilization

        List<float[][]> weightsList = new List<float[][]>();

        for (int i = 1; i < neurons.Length; i++)
        {
            List<float[]> layerWeightsList = new List<float[]>(); //layer weights list

            for (int j = 0; j < neurons[i].Length; j++)
            {
                int neuronsInPreviousLayer = hiddenLayers[i - 1];
                //int neuronsInPreviousLayer = hiddenLayerRNNFixed[i - 1];

                if ((i >= neurons.Length - 2) || (j < hiddenLayers[i]))
                {
                    //neuronsInPreviousLayer = hiddenLayers[i];
                    float[] neuronWeights = new float[neuronsInPreviousLayer]; //neruons weights

                    //set the weights randomly between 1 and -1
                    for (int k = 0; k < neuronsInPreviousLayer; k++)
                    {
                        neuronWeights[k] = UnityEngine.Random.Range(-0.5f, 0.5f);
                    }

                    layerWeightsList.Add(neuronWeights);
                }
                else
                {
                    float[] neuronWeights = new float[0]; //neruons weights

                    layerWeightsList.Add(neuronWeights);
                }


            }
            weightsList.Add(layerWeightsList.ToArray());
        }

        weights = weightsList.ToArray(); 
    }


    //neural network feedword by matrix operation
    public float[] FeedForward_Fast_Tanh(float[] inputs)
    {
        //add inputs to the neurons matrix
        for (int i = 0; i < inputs.Length; i++)
        {
            neurons[0][i] = inputs[i];
        }

        int weightLayerIndex = 0;

        // run through all neurons starting from the second layer
        for (int i = 1; i < neurons.Length; i++) //layers
        {
            //context layer copy
            /*if (i < hiddenLayers.Length - 1)
            {
                Buffer.BlockCopy(neurons[i], 0, neurons[i - 1], hiddenLayers[i - 1] * 4, (hiddenLayerRNNFixed[i - 1] - hiddenLayers[i - 1]) * 4);
            }*/

            for (int j = 0; j < hiddenLayers[i]; j++) //neurons of this layers minus the context neurons!
            {
                float value = BIAS;

                for (int k = 0; k < hiddenLayers[i - 1]; k++) //neurons of the previous layer
                {
                    value += weights[weightLayerIndex][j][k] * neurons[i - 1][k];
                }

                if (value < -3)
                    neurons[i][j] = -1;
                else if (value > 3)
                    neurons[i][j] = 1;
                else
                    neurons[i][j] = value * (27 + value * value) / (27 + 9 * value * value);

            }

            weightLayerIndex++;
        }

        return neurons[hiddenLayers.Length - 1]; //return output field
    }

    public float[] FeedForward_Slow_Tanh(float[] inputs)
    {
        //add inputs to the neurons matrix
        for (int i = 0; i < inputs.Length; i++)
        {
            neurons[0][i] = inputs[i];
        }

        int weightLayerIndex = 0;

        // run through all neurons starting from the second layer
        for (int i = 1; i < neurons.Length; i++) //layers
        {
            /*//context layer copy
            if (i < hiddenLayers.Length - 1)
            {
                //int k = 0;
                //for (int j = hiddenLayers[i - 1]; j < hiddenLayerRNNFixed[i - 1]; j++, k++)
                //{
                   // neurons[i - 1][j] = neurons[i][k];
                //}
                Buffer.BlockCopy(neurons[i], 0, neurons[i - 1], hiddenLayers[i - 1] * 4, (hiddenLayerRNNFixed[i - 1] - hiddenLayers[i - 1]) * 4);

            }*/

            for (int j = 0; j < hiddenLayers[i]; j++) //neurons of this layers minus the context neurons!
            {
                float value = BIAS;

                for (int k = 0; k < neurons[i - 1].Length; k++) //neurons of the previous layer
                {
                    value += weights[weightLayerIndex][j][k] * neurons[i - 1][k];
                }


                neurons[i][j] = (float)Math.Tanh(value);
            }

            weightLayerIndex++;
        }

        return neurons[hiddenLayers.Length - 1]; //return output field
    }




}