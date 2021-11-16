﻿using System.Collections.Generic;
using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;

public class NeuralNetwork
{
    //fundamental 
    public int[] layers;//layers
    private float[][] neurons;//neurons
    private float[][] biases;//biasses
    private float[][][] weights;//weights
    private int[] activations;//layers

    //genetic
    public float weightMin = -0.5f;
    public float weightMax = 0.5f;
    public float BiasMin = -0.5f;
    public float BiasMax = 0.5f;
    public float fitness = 0;//fitness

    //backprop
    public float learningRate = 0.01f;//learning rate
    public float cost = 0;

    private float[][] deltaBiases;//biasses
    private float[][][] deltaWeights;//weights
    private int deltaCount;

    public NeuralNetwork(int[] layers, string[] layerActivations, float weight, float bias, float learningRate)
    {
        weightMin = -weight;
        weightMax = weight;
        BiasMin = -bias;
        BiasMax = bias;
        this.learningRate = learningRate;

        this.layers = new int[layers.Length];
        for (int i = 0; i < layers.Length; i++)
        {
            this.layers[i] = layers[i];
        }
        activations = new int[layers.Length - 1];
        for(int i = 0; i < layers.Length-1; i++)
        {
            string action = layerActivations[i];
            switch (action)
            {
                case "sigmoid":
                    activations[i] = 0;
                    break;
                case "tanh":
                    activations[i] = 1;
                    break;
                case "relu":
                    activations[i] = 2;
                    break;
                case "leakyrelu":
                    activations[i] = 3;
                    break;
                default:
                    activations[i] = 2;
                    break;
            }
        }
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        InitNeurons();
        var initNeuronsTime = stopwatch.ElapsedMilliseconds;
        stopwatch.Restart();
        InitBiases();
        var initBiasesTime = stopwatch.ElapsedMilliseconds;
        stopwatch.Restart();
        InitWeights();
        var initWeightsTime = stopwatch.ElapsedMilliseconds; // 20x20x3: 967ms 30x30x3: 2900ms 50x50x3: 17.000ms 80x80x3: 100.000ms 100x100x3: 262.000ms
        stopwatch.Stop();
    }


    private void InitNeurons()//create empty storage array for the neurons in the network.
    {
        List<float[]> neuronsList = new List<float[]>();
        for (int i = 0; i < layers.Length; i++)
        {
            neuronsList.Add(new float[layers[i]]);
        }
        neurons = neuronsList.ToArray();
    }

    private void InitBiases()//initializes random array for the biases being held within the network.
    {
        List<float[]> biasList = new List<float[]>();
        for (int i = 0; i < layers.Length; i++)
        {
            float[] bias = new float[layers[i]];
            for (int j = 0; j < layers[i]; j++)
            {
                bias[j] = NextFloat(BiasMin, BiasMax);
            }
            biasList.Add(bias);
        }
        biases = biasList.ToArray();
    }

    private void InitWeights()//initializes random array for the weights being held in the network.
    {
        List<float[][]> weightsList = new List<float[][]>();        
        for (int i = 1; i < layers.Length; i++)//For each layer
        {
            List<float[]> layerWeightsList = new List<float[]>();
            int neuronsInPreviousLayer = layers[i - 1];
            for (int j = 0; j < neurons[i].Length; j++)//For each neuron in layer
            {
                float[] neuronWeights = new float[neuronsInPreviousLayer];
                
                //for (int k = 0; k < neuronsInPreviousLayer; k++)//For each previous neuron in the layer before connected 
                //{
                //    neuronWeights[k] = NextFloat(weightMin, weightMax);
                //}

                //STILL HAS TO BE TESTED -------------------------------
                Parallel.For(0, neuronsInPreviousLayer, k => {
                    neuronWeights[k] = NextFloat(weightMin, weightMax);
                });
                //------------------------------------------------------

                layerWeightsList.Add(neuronWeights);
            }
            weightsList.Add(layerWeightsList.ToArray());
        }
        weights = weightsList.ToArray();
    }

    public float[] FeedForward(float[] inputs)//feed forward, inputs >==> outputs.
    {
        for (int i = 0; i < inputs.Length; i++)
        {
            neurons[0][i] = inputs[i];
        }
        for (int i = 1; i < layers.Length; i++)
        {
            int layer = i-1;
            for (int j = 0; j < neurons[i].Length; j++)
            {
                float value = 0f;
                for (int k = 0; k < neurons[i - 1].Length; k++)
                {
                    value += weights[i - 1][j][k] * neurons[i - 1][k];
                }
                neurons[i][j] = activate(value + biases[i][j],layer);
            }
        }
        var outputLayer = neurons[neurons.Length - 1];
        return outputLayer;
    }
    //Backpropagation implemtation down until mutation.
    public float activate(float value, int layer)//all activation functions
    {
        switch (activations[layer])
        {
            case 0:
                return sigmoid(value);
            case 1:
                return tanh(value);
            case 2:
                return relu(value);
            case 3:
                return leakyrelu(value);
            default:
                return relu(value);
        }
    }
    public float activateDer(float value, int layer)//all activation function derivatives
    {
        switch (activations[layer])
        {
            case 0:
                return sigmoidDer(value);
            case 1:
                return tanhDer(value);
            case 2:
                return reluDer(value);
            case 3:
                return leakyreluDer(value);
            default:
                return reluDer(value);
        }
    }

    public float sigmoid(float x)//activation functions and their corrosponding derivatives
    {
        float k = (float)Math.Exp(x);
        return k / (1.0f + k);
    }
    public float tanh(float x)
    {
        return (float)Math.Tanh(x);
    }
    public float relu(float x)
    {
        return (0 >= x) ? 0 : x;
    }
    public float leakyrelu(float x)
    {
        return (0 >= x) ? 0.01f * x : x;
    }
    public float sigmoidDer(float x)
    {
        return x * (1 - x);
    }
    public float tanhDer(float x)
    {
        return 1 - (x * x);
    }
    public float reluDer(float x)
    {
        return (0 >= x) ? 0 : 1;
    }
    public float leakyreluDer(float x)
    {
        return (0 >= x) ? 0.01f : 1;
    }

    //Back propagation is where you let the network learn data beforehand. Iguess
    // This is for inputing data and expected data so the network can already adjust to it. 
    // This would do great with a mix of evolution/mutation
    public void BackPropagate(float[] inputs, float[] expected)//backpropogation;
    {
        float[] output = FeedForward(inputs);//runs feed forward to ensure neurons are populated correctly

        //cost = 0;
        //for (int i = 0; i < output.Length; i++) cost += (float)Math.Pow(output[i] - expected[i], 2);//calculated cost of network
        //cost = cost / 2;//this value is not used in calculions, rather used to identify the performance of the network

        float[][] gamma;


        List<float[]> gammaList = new List<float[]>();
        for (int i = 0; i < layers.Length; i++)
        {
            gammaList.Add(new float[layers[i]]);
        }
        gamma = gammaList.ToArray();//gamma initialization

        int layer = layers.Length - 2;
        for (int i = 0; i < output.Length; i++) gamma[layers.Length-1][i] = (output[i] - expected[i]) * activateDer(output[i],layer);//Gamma calculation
        for (int i = 0; i < neurons[layers.Length - 1].Length; i++)//calculates the w' and b' for the last layer in the network
        {
            biases[layers.Length - 1][i] -= gamma[layers.Length - 1][i] * learningRate;
            for (int j = 0; j < neurons[layers.Length - 2].Length; j++)
            {
                
                weights[layers.Length - 2][i][j] -= gamma[layers.Length - 1][i] * neurons[layers.Length-2][j] * learningRate;//*learning 
            }
        }

        for (int i = layers.Length - 2; i > 0; i--)//runs on all hidden layers
        {
            layer = i - 1;
            for (int j = 0; j < neurons[i].Length; j++)//outputs
            {
                gamma[i][j] = 0;
                for (int k = 0; k < gamma[i+1].Length; k++)
                {
                    gamma[i][j] = gamma[i + 1][k] * weights[i][k][j];
                }
                gamma[i][j] *= activateDer(neurons[i][j],layer);//calculate gamma
            }
            for (int j = 0; j < neurons[i].Length; j++)//itterate over outputs of layer
            {
                biases[i][j] -= gamma[i][j] * learningRate;//modify biases of network
                for (int k = 0; k < neurons[i-1].Length; k++)//itterate over inputs to layer
                {
                    weights[i - 1][j][k] -= gamma[i][j] * neurons[i-1][k] * learningRate;//modify weights of network
                }
            }
        }
    }

    //Genetic implementations down onwards until save.

    //Mutate is there to level up the network based on opinions of the user. 
    //high: ???
    //val: ???
    public Task Mutate(int high, float val)//used as a simple mutation function for any genetic implementations.
    {
        for (int i = 0; i < biases.Length; i++)
        {
            for (int j = 0; j < biases[i].Length; j++)
            {
                biases[i][j] = (NextFloat(0f, high) <= 2) ? biases[i][j] += NextFloat(-val, val) : biases[i][j];
            }
        }

        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    weights[i][j][k] = (NextFloat(0f, high) <= 2) ? weights[i][j][k] += NextFloat(-val, val) : weights[i][j][k];
                }
            }
        }

        return Task.CompletedTask;
    }

    //Just a function to get a random float between min and max
    static float NextFloat(float min, float max)
    {
        var random = new System.Random();
        double val = (random.NextDouble() * (max - min) + min);
        return (float)val;
    }

    public int CompareTo(NeuralNetwork other) //Comparing For Genetic implementations. Used for sorting based on the fitness of the network
    {
        if (other == null) return 1;

        if (fitness > other.fitness)
            return 1;
        else if (fitness < other.fitness)
            return -1;
        else
            return 0;
    }

    public NeuralNetwork copy(NeuralNetwork nn) //For creatinga deep copy, to ensure arrays are serialzed.
    {
        for (int i = 0; i < biases.Length; i++)
        {
            for (int j = 0; j < biases[i].Length; j++)
            {
                nn.biases[i][j] = biases[i][j];
            }
        }
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    nn.weights[i][j][k] = weights[i][j][k];
                }
            }
        }
        return nn;
    }

    //save and load functions
    public void Load(string path)//this loads the biases and weights from within a file into the neural network.
    {
        TextReader tr = new StreamReader(path);
        int NumberOfLines = (int)new FileInfo(path).Length;
        string[] ListLines = new string[NumberOfLines];
        int index = 1;
        for (int i = 1; i < NumberOfLines; i++)
        {
            ListLines[i] = tr.ReadLine();
        }
        tr.Close();
        if (new FileInfo(path).Length > 0)
        {
            for (int i = 0; i < biases.Length; i++)
            {
                for (int j = 0; j < biases[i].Length; j++)
                {
                    biases[i][j] = float.Parse(ListLines[index]);
                    index++;
                }
            }

            for (int i = 0; i < weights.Length; i++)
            {
                for (int j = 0; j < weights[i].Length; j++)
                {
                    for (int k = 0; k < weights[i][j].Length; k++)
                    {
                        weights[i][j][k] = float.Parse(ListLines[index]); ;
                        index++;
                    }
                }
            }
        }
    }
    public void Save(string path)//this is used for saving the biases and weights within the network to a file.
    {
        File.Create(path).Close();
        StreamWriter writer = new StreamWriter(path, true);

        for (int i = 0; i < biases.Length; i++)
        {
            for (int j = 0; j < biases[i].Length; j++)
            {
                writer.WriteLine(biases[i][j]);
            }
        }

        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    writer.WriteLine(weights[i][j][k]);
                }
            }
        }
        writer.Close();
    }
}
