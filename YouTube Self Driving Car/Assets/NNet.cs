using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MathNet.Numerics.LinearAlgebra;

using System;

public class NNet : MonoBehaviour
{
    
    #region  Variables

    public Matrix<float> inputLayer = Matrix<float>.Build.Dense(1,3);
    
    public List<Matrix<float>> hiddenLayers = new List<Matrix<float>>();

    public Matrix<float> outputLayer = Matrix<float>.Build.Dense(1,2);

    public List<Matrix<float>> weights = new List<Matrix<float>>();

    public List<float> biases = new List<float>();

    public float fitness;

    #endregion

    //No start or Update function here...

    public void Initialise (int hiddenLayerCount, int hiddenNeuronCount) {

        inputLayer.Clear();
        hiddenLayers.Clear();
        outputLayer.Clear();
        weights.Clear();
        biases.Clear();

        for (int i = 0; i < hiddenLayerCount+1; i++) {

            Matrix<float> f = Matrix<float>.Build.Dense(1,hiddenLayerCount);
            hiddenLayers.Add(f);

            biases.Add(UnityEngine.Random.Range(-1f,1f));

            //This is the connection between Input and First hidden layer
            if (i == 0) {
                Matrix<float> inputToH1 = Matrix<float>.Build.Dense(3,hiddenNeuronCount);
                weights.Add(inputToH1);
                continue;
            }

            Matrix<float> HiddenToHidden = Matrix<float>.Build.Dense(hiddenNeuronCount,hiddenNeuronCount);
            weights.Add(HiddenToHidden);

        }

        Matrix<float> OutputWeights = Matrix<float>.Build.Dense(hiddenNeuronCount,2);
        weights.Add(OutputWeights);
        biases.Add(UnityEngine.Random.Range(-1f,1f));

        //Initialised weights but we need to randomise between -1 and 1
        RandomiseWeights();


    }

    //This whole segment is to copy the data of this Network to another object without referencing the instance

    public NNet InitialiseCopy (int hiddenLayerCount, int hiddenNeuronCount) {
        NNet n = new NNet();
        List<Matrix<float>> newWeights = new List<Matrix<float>>();

        for (int i = 0; i < weights.Count; i++) {

            Matrix<float> newDim = Matrix<float>.Build.Dense(weights[i].RowCount,weights[i].ColumnCount);

            for (int x = 0; x < newDim.RowCount; x++) {
                for (int y = 0; y < newDim.ColumnCount; y++) {

                    newDim[x,y] = weights[i][x,y];

                }
            }

            newWeights.Add(newDim);

        }

        List<float> newBises = new List<float>();
        for (int i = 0; i < biases.Count; i++) {
            newBises.Add(biases[i]);
        }

        n.weights = newWeights;
        n.biases = newBises;

        n.InitialiseHidden(hiddenLayerCount,hiddenNeuronCount);

        return n;

    }

    public void InitialiseHidden(int hiddenLayerCount, int hiddenNeuronCount) {
        inputLayer.Clear();
        hiddenLayers.Clear();
        outputLayer.Clear();

        for (int i = 0; i < hiddenLayerCount+1; i++) {
            Matrix<float> f = Matrix<float>.Build.Dense(1,hiddenNeuronCount);
            hiddenLayers.Add(f);
        }

    }

    public void RandomiseWeights() {

        for (int i = 0; i < weights.Count; i++) {

            for (int x = 0; x < weights[i].RowCount; x++) {

                for (int y = 0; y < weights[i].ColumnCount; y++) {

                    weights[i][x,y] = UnityEngine.Random.Range(-1f,1f);

                }

            }

        }

    }


    public (float,float) RunNetwork (float a,float b, float c) {

        inputLayer[0,0] = a;
        inputLayer[0,1] = b;
        inputLayer[0,2] = c;

        inputLayer = inputLayer.PointwiseTanh();

        hiddenLayers[0] = ((inputLayer*weights[0])+biases[0]).PointwiseTanh();

        for (int i = 1; i < hiddenLayers.Count; i++) {
            hiddenLayers[i] = ((hiddenLayers[i-1]*weights[i])+biases[i]).PointwiseTanh();
        }

        outputLayer = ((hiddenLayers[hiddenLayers.Count-1]*weights[weights.Count-1])+biases[biases.Count-1]).PointwiseTanh();

        //Sigmoid for acceleration & Tanh for steering
        return (Sigmoid(outputLayer[0,0]), (float)Math.Tanh(outputLayer[0,1]));
    }

    private float Sigmoid (float s) {
        return (1/(1+Mathf.Exp(-s)));
    }

}
