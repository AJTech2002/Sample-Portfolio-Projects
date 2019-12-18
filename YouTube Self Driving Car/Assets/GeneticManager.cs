using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MathNet.Numerics.LinearAlgebra;

public class GeneticManager : MonoBehaviour
{
    [Header("References")]
    public CarController carController;

    [Header("Public Controls")]
    public int initialPopulation = 85;
    [Range(0.0f,1.0f)]
    public float mutationRate = 0.05f;

    [Header("Crossover Controls")]
    public int bestAgentSelection = 8;
    public int worstAgentSelection = 3;
    public int numberToCrossover = 39;

    private List<int> genePool = new List<int>();
    private int naturallySelected;
    private NNet[] population;

    [Header("Public View")]
    public int currentGeneration;
    public int currentGenome;


    private void Start() {
        CreatePopulation();
    }

    private void CreatePopulation() {
        population = new NNet[initialPopulation];
        FillPopulationWithRandomValues(population,0);
        ResetToCurrentGenome();
    }

    //Set currentGenome to the Car Controller
    private void ResetToCurrentGenome() {
        carController.ResetWithNetwork(population[currentGenome]);
    }

    private void FillPopulationWithRandomValues(NNet[] newPopulation, int startIndex) {

        //Will start at index at array and fill everything from that point to the end with random networks
        while (startIndex < initialPopulation) {
            newPopulation[startIndex] = new NNet();
            newPopulation[startIndex].Initialise(carController.LAYERS,carController.NEURONS);
            startIndex++;
        }

    }


    //Sorry just a spelling mistake here
    public void Death(float fitness, NNet network) {
        if (currentGenome < population.Length-1) {
            population[currentGenome].fitness = fitness;
            currentGenome+=1;
            ResetToCurrentGenome();
        }
        else {
           
            RePopulate();


        }
    }


//------------------------------------


    private void RePopulate() {
        
        //Resets
        genePool.Clear();
        currentGeneration ++;
        naturallySelected = 0;

        //The population is now sorted by fitness
        SortPopulation();

        //Have preserved the top networks & populated the gene pool
        NNet[] newPopulation = PickBestPopulation();

        //Crossover Time! 
        CrossOver(newPopulation);

        //Mutation Time!
        Mutation(newPopulation);

        //--- Now at this point some of the population would have been filled up say 50 out of the 85 initial population .. to keep the number of 85 we spawn 35 random networks ..
        FillPopulationWithRandomValues(newPopulation,naturallySelected);

        population = newPopulation;

        currentGenome = 0;
        ResetToCurrentGenome();

        //FINALLY!

    }

    private void Mutation (NNet[] newPopulation) {

        //We only mutate the networks that we actually have in the population so far 
        for (int i = 0; i < naturallySelected; i++) {

            for (int c = 0; c < newPopulation[i].weights.Count; c++) {

                //Weight mutation
                if (Random.Range(0.0f, 1.0f) < mutationRate) {

                    //We need to mutate a matrix now
                    newPopulation[i].weights[c] = Mutate(newPopulation[i].weights[c]);
                
                }

            }

            //Mutation for biases
            if (Random.Range(0.0f, 1.0f) < mutationRate) {

                int b = Random.Range(0,newPopulation[i].biases.Count);
                newPopulation[i].biases[b] = Mathf.Clamp(newPopulation[i].biases[b] + Random.Range(-1f,1f),-1f,1f);

            }

        }

    }

    private Matrix<float> Mutate (Matrix<float> a) {

        //Just a hard-coded value to see how many points within the network will get randomised
        int randomPoints = Random.Range(1,(a.RowCount*a.ColumnCount)/7);

        Matrix<float> C = a;
        
        //The number of points that will be mutated
        for (int i = 0; i < randomPoints+1; i++) {

            int randColumn = Random.Range(0,C.ColumnCount);
            int randRow = Random.Range(0,C.RowCount);

            //Slight nudge up or down is a mutation enough
            C[randRow,randColumn] = Mathf.Clamp(C[randRow,randColumn]+Random.Range(-1f,1f),-1f,1f);

        }

        return C;

    }

    private void CrossOver(NNet[] newPopulation) {

        //For every crossover 2 children are created
        for (int i = 0; i < numberToCrossover; i += 2) {

            //Defaults
            int AIndex = i;
            int BIndex = i+1;

            if (genePool.Count >= 1 ){

                for (int l = 0; l < 100; l++) {
                    //Here we are getting the index of two random members of the gene pool
                    AIndex = genePool[Random.Range(0,genePool.Count)];
                    BIndex = genePool[Random.Range(0,genePool.Count)];

                    //Can't crossover two of the same members, so check if they are not the same .. then leave loop
                    if (AIndex != BIndex) {
                        break;
                    }

                    //Else continue to randomly pick until different members are chosen.

                }

            }

            //Create 2 new untouched neural networks

            NNet Child1 = new NNet();
            NNet Child2 = new NNet();

            Child1.Initialise(carController.LAYERS, carController.NEURONS);
            Child2.Initialise(carController.LAYERS,carController.NEURONS);

            //First weights crossover
            for (int w = 0; w < Child1.weights.Count; w++) {

                //Randomly pick from which member the weight is taken 
                //This is technically a work around because instead of crossing over individual weights, we are crossing over entire weight layers...
                if (Random.Range(0.0f,1.0f) < 0.5f) {
                    Child1.weights[w] = (population[AIndex].weights[w]);
                    Child2.weights[w] = (population[BIndex].weights[w]);
                }
                else {
                    Child1.weights[w] = (population[BIndex].weights[w]);
                    Child2.weights[w] = (population[AIndex].weights[w]);
                }

            }

            //Now for biases
             for (int b = 0; b < Child1.biases.Count; b++) {

                 if (Random.Range(0.0f, 1.0f) < 0.5f) {
                    Child1.biases[b] = (population[AIndex].biases[b]);
                    Child2.biases[b] = (population[BIndex].biases[b]);
                 }
                 else {
                    Child1.biases[b] = (population[BIndex].biases[b]);
                    Child2.biases[b] = (population[AIndex].biases[b]);
                 }

             }

            //Since we preserved the best networks, the naturallySelected was incremented to say 8... so now we will start adding children from the index of 8, so the best networks stay there.

             newPopulation[naturallySelected] = Child1;
             naturallySelected++;

             newPopulation[naturallySelected] = Child2;
             naturallySelected++;

            //NaturallySelected is incremented twice because there are two children

        }

    }

    private NNet[] PickBestPopulation() {

        NNet[] newPopulation = new NNet[initialPopulation];
        for (int i = 0; i < bestAgentSelection; i++) {
            
            //We need a copy of the neural network because if we simply reference, then when the network obj changes so will the reference
            //The top networks are passed onto the next generation unharmed
            newPopulation[naturallySelected] = population[i].InitialiseCopy(carController.LAYERS,carController.NEURONS);


            //The networks are also added to the gene pool based on their fitness
            int f = Mathf.RoundToInt(population[i].fitness*10);
            for (int c = 0; c < f; c++) {
                //We are adding the index of the network in the population to the gene pool
                genePool.Add(i);
            }

            newPopulation[naturallySelected].fitness = 0;

            //Increment this so that the crossover doesn't override this
            naturallySelected++;

        }


        //This is to add some variety to the genePool
        for (int i = 0; i < worstAgentSelection; i++) {

            int last = population.Length-1;
            last -= i;

            int f = Mathf.RoundToInt(population[last].fitness*10);
            for (int c = 0; c < f; c++) {
                genePool.Add(last);
            }

        }

        return newPopulation;

    }

    //Simple Bubble Sort
    private void SortPopulation() {
        for (int i = 0; i < population.Length; i++) {
            for (int j = i; j < population.Length; j++) {
                if (population[i].fitness < population[j].fitness) {
                    NNet temp = population[i];
                    population[i] = population[j];
                    population[j] = temp;
                }
            }
        }

    }

}
