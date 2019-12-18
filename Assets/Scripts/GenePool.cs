using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class GenePool : MonoBehaviour
{

    [Header("Genetic Variables")]
    public int population;
    public float mutChance, strucChance = 20;
    //public float mutationRate = 0.02f;
    public float speciationThreshold = 2;
    public float speciesFitnessThreshold = 200;

    [Header("Variables")]
    public List<NNet> combinedPopulation = new List<NNet>();
    public List<Transform> carts = new List<Transform>();
    public List<RandomWindGenerator> cartScripts = new List<RandomWindGenerator>();
    public List<Species> species = new List<Species>();
    [Space(20)]
    public Transform cart;
    

    [Space(20)]
    public NEATManager manager;

    public int generationNumber = -1;
    private int iterator = 0;

    private Vector3 initialPosition, initialRotation;

    private void Awake()
    {
        initialPosition = cart.position;
        initialRotation = cart.eulerAngles;
        Populate();
    }
    public void Populate()
    {
        generationNumber++;
        if (generationNumber == 0)
        {
            for (int i = 0; i < population; i++)
            {
                Transform t = (Transform)Transform.Instantiate<Transform>(cart, cart.position, cart.rotation);
                carts.Add(t);
                combinedPopulation.Add(t.GetComponentInChildren<RandomWindGenerator>().network);
                cartScripts.Add(t.GetComponentInChildren<RandomWindGenerator>());
            }

        }
        else
        {
            for (int i = 0; i < population; i++)
            {
                //reset all the networks to the changed networks
                cartScripts[i].Reset(combinedPopulation[i].net);
            }
        }
    }

    public void RecordDeath(Genotype g, float fitness)
    {

        iterator++;
        if (iterator >= population)
        {
            iterator = 0;
            //Next Generation
            CueNextGeneration();
            return;
        }

    }

    public void CueNextGeneration()
    {
        //Speciate
        Speciate();

        CrossOver();
        //Crossover & Mutate based on Fitness
        //Re-Populate (Reset all the carts) + Assign new Genotypes to the NNEts
        
        Populate();
    }

    public void CrossOver()
    {

        for (int i = 0; i < species.Count; i++)
        {
            float avgFitness = 0f;
            for (int f = 0; f < species[i].scripts.Count; f++)
            {
                avgFitness += species[i].scripts[f].currentFitness;
            }

            avgFitness /= species[i].scripts.Count;

            //Local gene pool where you get ranked based on fitness
            List<Genotype> localGenePool = new List<Genotype>();
            //Keep the top 20%
            if (species[i].scripts.Count > 2 )
            {
                species[i].scripts.Sort(delegate (RandomWindGenerator x, RandomWindGenerator y)
                {
                    return x.CompareTo(y);
                });

                int TopTwentyPercent = Mathf.RoundToInt(species[i].scripts.Count * 0.5f);
                if (TopTwentyPercent == 0 || TopTwentyPercent < 0 || TopTwentyPercent > localGenePool.Count - 1)
                    TopTwentyPercent = Mathf.RoundToInt(species[i].scripts.Count*0.5f);

                //IMPORTANT : Top 20 percent not being used here hahh
                for (int c = 0; c < TopTwentyPercent; c++)
                {
                    int times = Mathf.RoundToInt(species[i].scripts[c].currentFitness *100);

                    for (int t = 0; t < times; t++)
                    {
                        localGenePool.Add(species[i].scripts[c].network.net);
                    }
                }

                //Keep the bottom one
                localGenePool.Add(species[i].scripts[species[i].scripts.Count - 1].network.net);
                CrossoverSpecies(localGenePool, species[i]);
            }
            else
            {
                for (int l = 0; l < species[i].scripts.Count; l++)
                {
                    MutateNetwork(100, strucChance, species[i].scripts[l].network.net);
                    species[i].scripts[l].network.net = species[i].scripts[l].network.net;
                }
            }
            

            //Randomly crossover and see what happens (or you will have to normalise the fitnesses to get a ratio)
        }
    }

    public void CrossoverSpecies (List<Genotype> localgenePool, Species s)
    {
        if (localgenePool == null || localgenePool.Count <= 1)
            return;

        List<Genotype> newbatch = new List<Genotype>();
        for (int i = 0; i < s.population.Count; i++)
        {
            //Crossing over 
            int a = UnityEngine.Random.Range(0, localgenePool.Count);
            int b = UnityEngine.Random.Range(0, localgenePool.Count);
            //print("Crossing over A " + a + " B : " + b);
            if (a != b)
                newbatch.Add(CrossOver(localgenePool[a], localgenePool[b]));
            else
                newbatch.Add(localgenePool[a]);
        }

        //Replacing Stage
        for (int i = 0; i < s.scripts.Count; i++)
        {
            MutateNetwork(mutChance, strucChance, newbatch[i]);
            s.scripts[i].network.net = newbatch[i];
        }
    }


    public void Speciate()
    {
        species.Clear();   
        for (int c = 0; c < combinedPopulation.Count; c++) {
            
            bool foundSpecies = false;
            for (int i = 0; i < species.Count; i++)
            {
                if (BelongsToSpecies(species[i],combinedPopulation[c].net))
                {
                    species[i].scripts.Add(cartScripts[c]);
                    species[i].population.Add(combinedPopulation[c].net);
                    foundSpecies = true;
                    break;
                }
            }

            if (foundSpecies == false)
            {
                //New species has been created
                Species s = new Species();
                s.speciesName = "S " + UnityEngine.Random.Range(0, 100);
                s.speciesColor = UnityEngine.Random.ColorHSV();

                s.population.Add(combinedPopulation[c].net);
                s.scripts.Add(cartScripts[c]);
                species.Add(s);
            }

        }

        
    }

    public bool BelongsToSpecies (Species s, Genotype a)
    {
        float sScore = manager.SpeciationScore(a, s.population[0]);

        if (sScore <= speciationThreshold || float.IsNaN(sScore))
        {
            
            return true;
        }
        else return false;
    }

    #region Network Code

    public Genotype InitialiseNetwork(int inputs, int outputs)
    {

        Genotype genotype = new Genotype();

        for (int i = 0; i < inputs; i++)
        {
            Node n = new Node(1f, NodeType.Input, 0, i);
            genotype.nodes.Add(n);
            genotype.uniqueNodeIdentifier.Add(n.uniqueID, n);
        }

        for (int i = 0; i < outputs; i++)
        {
            Node n3 = new Node(0f, NodeType.Output, 100000, i + inputs);
            genotype.nodes.Add(n3);
            genotype.uniqueNodeIdentifier.Add(n3.uniqueID, n3);
        }

 
        genotype.Redistribute();

        return genotype;
    }

    public Genotype CrossOver(Genotype a, Genotype b)
    {

        Genotype newNet = InitialiseNetwork(4, 1);

        //Align Innovation Numbers
        Dictionary<int, NodeConnection> aConnections = new Dictionary<int, NodeConnection>();
        Dictionary<int, NodeConnection> bConnections = new Dictionary<int, NodeConnection>();

        for (int i = 0; i < a.connections.Count; i++)
        {
            if (!aConnections.ContainsKey(a.connections[i].innovationNumber))
                aConnections.Add(a.connections[i].innovationNumber, a.connections[i]);
            else
                Debug.LogWarning("You're loosing gnomes");
        }

        for (int c = 0; c < b.connections.Count; c++)
        {
            if (!bConnections.ContainsKey(b.connections[c].innovationNumber))
                bConnections.Add(b.connections[c].innovationNumber, b.connections[c]);
            else
                Debug.LogWarning("You're loosing gnomes");
        }

        int max = aConnections.Count;
        if (aConnections.Count < bConnections.Count)
            max = bConnections.Count;

        //Going through each innovation number and matching
        for (int i = 0; i < max; i++)
        {
            if (aConnections.ContainsKey(i) && bConnections.ContainsKey(i))
            {
                float chance = UnityEngine.Random.value;
                if (chance < 0.5f)
                {
                    NodeConnection aC = aConnections[i];
                    NodeConnection newConnection = new NodeConnection(null, null);

                    Node a1 = null;
                    if (newNet.uniqueNodeIdentifier.ContainsKey(aC.con1.uniqueID))
                        a1 = newNet.uniqueNodeIdentifier[aC.con1.uniqueID];

                    Node a2 = null;
                    if (newNet.uniqueNodeIdentifier.ContainsKey(aC.con2.uniqueID))
                        a2 = newNet.uniqueNodeIdentifier[aC.con2.uniqueID];

                    if (!newNet.nodes.Contains(aC.con1) && a1 == null)
                    {
                        newNet.nodes.Add(aC.con1);
                        newNet.hidden.Add(aC.con1);
                        newNet.AssignLayer(aC.con1);
                    }

                    if (!a.nodes.Contains(aC.con2) && a2 == null)
                    {
                        newNet.nodes.Add(aC.con2);
                        newNet.hidden.Add(aC.con2);
                        newNet.AssignLayer(aC.con2);
                    }

                    newConnection.con1 = aC.con1;
                    newConnection.con2 = aC.con2;

                    if (a1 != null) newConnection.con1 = a1;
                    if (a2 != null) newConnection.con2 = a2;

                    newConnection.weight = aC.weight;
                    newConnection.enabled = aC.enabled;
                    newConnection.layerNumber = aC.layerNumber;



                    newConnection.innovationNumber = aC.innovationNumber;
                    newConnection.replacedBy = aC.replacedBy;

                    if (newNet.layerDictionary.ContainsKey(aC.con2.layerNumber))
                    {
                        newNet.connections.Add(newConnection);
                        newNet.AssignLayer(newConnection);

                        newNet.layerDictionary[aC.con2.layerNumber].connectionsComingIn.Add(newConnection);
                    }
                }
                else
                {
                    NodeConnection aC = bConnections[i];
                    NodeConnection newConnection = new NodeConnection(null, null);

                    Node a1 = null;
                    if (newNet.uniqueNodeIdentifier.ContainsKey(aC.con1.uniqueID))
                        a1 = newNet.uniqueNodeIdentifier[aC.con1.uniqueID];

                    Node a2 = null;
                    if (newNet.uniqueNodeIdentifier.ContainsKey(aC.con2.uniqueID))
                        a2 = newNet.uniqueNodeIdentifier[aC.con2.uniqueID];

                    if (!newNet.nodes.Contains(aC.con1) && a1 == null)
                    {
                        newNet.nodes.Add(aC.con1);
                        newNet.hidden.Add(aC.con1);
                        newNet.AssignLayer(aC.con1);
                    }

                    if (!a.nodes.Contains(aC.con2) && a2 == null)
                    {
                        newNet.nodes.Add(aC.con2);
                        newNet.hidden.Add(aC.con2);
                        newNet.AssignLayer(aC.con2);
                    }

                    newConnection.con1 = aC.con1;
                    newConnection.con2 = aC.con2;

                    if (a1 != null) newConnection.con1 = a1;
                    if (a2 != null) newConnection.con2 = a2;

                    newConnection.weight = aC.weight;
                    newConnection.enabled = aC.enabled;
                    newConnection.layerNumber = aC.layerNumber;
                    newConnection.innovationNumber = aC.innovationNumber;
                    newConnection.replacedBy = aC.replacedBy;

                    if (newNet.layerDictionary.ContainsKey(aC.con2.layerNumber))
                    {
                        newNet.connections.Add(newConnection);
                        newNet.AssignLayer(newConnection);

                        newNet.layerDictionary[aC.con2.layerNumber].connectionsComingIn.Add(newConnection);
                    }
                }

                continue;
            }

            if (aConnections.ContainsKey(i))
            {
                NodeConnection aC = aConnections[i];
                NodeConnection newConnection = new NodeConnection(null, null);

                Node a1 = null;
                if (newNet.uniqueNodeIdentifier.ContainsKey(aC.con1.uniqueID))
                    a1 = newNet.uniqueNodeIdentifier[aC.con1.uniqueID];

                Node a2 = null;
                if (newNet.uniqueNodeIdentifier.ContainsKey(aC.con2.uniqueID))
                    a2 = newNet.uniqueNodeIdentifier[aC.con2.uniqueID];

                if (!newNet.nodes.Contains(aC.con1) && a1 == null)
                {
                    newNet.nodes.Add(aC.con1);
                    newNet.hidden.Add(aC.con1);
                    newNet.AssignLayer(aC.con1);
                }

                if (!a.nodes.Contains(aC.con2) && a2 == null)
                {
                    newNet.nodes.Add(aC.con2);
                    newNet.hidden.Add(aC.con2);
                    newNet.AssignLayer(aC.con2);
                }

                newConnection.con1 = aC.con1;
                newConnection.con2 = aC.con2;

                if (a1 != null) newConnection.con1 = a1;
                if (a2 != null) newConnection.con2 = a2;

                newConnection.weight = aC.weight;
                newConnection.enabled = aC.enabled;
                newConnection.layerNumber = aC.layerNumber;
                newConnection.innovationNumber = aC.innovationNumber;
                newConnection.replacedBy = aC.replacedBy;

                if (newNet.layerDictionary.ContainsKey(aC.con2.layerNumber))
                {
                    newNet.connections.Add(newConnection);
                    newNet.AssignLayer(newConnection);

                    newNet.layerDictionary[aC.con2.layerNumber].connectionsComingIn.Add(newConnection);
                }
                continue;
            }

            if (bConnections.ContainsKey(i))
            {
                NodeConnection aC = bConnections[i];
                NodeConnection newConnection = new NodeConnection(null, null);

                Node a1 = null;
                if (newNet.uniqueNodeIdentifier.ContainsKey(aC.con1.uniqueID))
                    a1 = newNet.uniqueNodeIdentifier[aC.con1.uniqueID];

                Node a2 = null;
                if (newNet.uniqueNodeIdentifier.ContainsKey(aC.con2.uniqueID))
                    a2 = newNet.uniqueNodeIdentifier[aC.con2.uniqueID];

                if (!newNet.nodes.Contains(aC.con1) && a1 == null)
                {
                    newNet.nodes.Add(aC.con1);
                    newNet.hidden.Add(aC.con1);
                    newNet.AssignLayer(aC.con1);
                }

                if (!a.nodes.Contains(aC.con2) && a2 == null)
                {
                    newNet.nodes.Add(aC.con2);
                    newNet.hidden.Add(aC.con2);
                    newNet.AssignLayer(aC.con2);
                }

                newConnection.con1 = aC.con1;
                newConnection.con2 = aC.con2;

                if (a1 != null) newConnection.con1 = a1;
                if (a2 != null) newConnection.con2 = a2;

                newConnection.weight = aC.weight;
                newConnection.enabled = aC.enabled;
                newConnection.layerNumber = aC.layerNumber;
                newConnection.innovationNumber = aC.innovationNumber;
                newConnection.replacedBy = aC.replacedBy;

                if (newNet.layerDictionary.ContainsKey(aC.con2.layerNumber))
                {
                    newNet.connections.Add(newConnection);
                    newNet.AssignLayer(newConnection);

                    newNet.layerDictionary[aC.con2.layerNumber].connectionsComingIn.Add(newConnection);
                }
                continue;
            }

            //Neither exists

        }

        //newNet.Redistribute();
        return newNet;
    }
    #endregion


    #region Copied Mutation Code

    public void MutateNetwork(float mutationChance, float structualChangeChance, Genotype net)
    {

        net.Redistribute();
        int structuralChange = UnityEngine.Random.Range(1, 101);

        int r = UnityEngine.Random.Range(1, 101);
        if (r < mutationChance)
        {
            //Is the network going to be mutated structurally
            if (structuralChange <= structualChangeChance)
            {

                //If so, is the connection going to be mutated or is there going to be a new ndoe
                int selection = UnityEngine.Random.RandomRange(1, 3);

                //New node mutation
                if (selection == 1)
                {
                    if (net.connections.Count <= 0)
                    {
                        //Debug.Log("No connections exist.");
                        return;
                    }

                    int randCon = UnityEngine.Random.Range(0, net.connections.Count);
                    NewNode(net.connections[randCon], false, net);

                }

                //New connection mutation
                else if (selection == 2)
                {
                    //Trying to find a good 2 nodes
                    for (int i = 0; i < 200; i++)
                    {
                        int randNodA = UnityEngine.Random.Range(0, net.nodes.Count);
                        int randNodB = UnityEngine.Random.Range(0, net.nodes.Count);

                        if (randNodA == randNodB)
                            continue;

                        if (net.nodes[randNodA].type == NodeType.Input && net.nodes[randNodB].type == NodeType.Input)
                            continue;

                        if (net.nodes[randNodA].type == NodeType.Output && net.nodes[randNodB].type == NodeType.Output)
                            continue;

                        //IMPORTANT : Connections are allowed between layers

                        //if (nodes[randNodA].layerNumber == nodes[randNodB].layerNumber)
                        //  continue;


                        if (net.nodes[randNodA].layerNumber < net.nodes[randNodB].layerNumber)
                            NewConnection(net.nodes[randNodA], net.nodes[randNodB], UnityEngine.Random.Range(-1.00f, 1.00f), true, false, net);
                        else
                            NewConnection(net.nodes[randNodB], net.nodes[randNodA], UnityEngine.Random.Range(-1.00f, 1.00f), true, false, net);

                        break;
                    }

                }

                //Failsafe
                else
                {
                    Debug.LogError("More or less options than possible");
                }

            }
            else
            {
                int selection = UnityEngine.Random.RandomRange(1, 3);

                if (selection == 1)
                {
                    int randCon = UnityEngine.Random.Range(0, net.connections.Count);

                    if (net.connections.Count > randCon && net.connections[randCon] != null)
                    {
                        //Can be between -1f -> 1f or 0f to 1f (we need left & right)
                        net.connections[randCon].weight = UnityEngine.Random.Range(-1.00f, 1.00f);
                    }
                }
                else
                {
                    int randCon = UnityEngine.Random.Range(0, net.connections.Count);

                    if (net.connections.Count > randCon && net.connections[randCon] != null)
                        net.connections[randCon].enabled = !net.connections[randCon].enabled;
                }
                //Weight shift/mutate 
                //Enable/Disable switch random connection
            }

        }



    }
    //Creating a new connection between two nodes, contains error checking and edge cases + increments the global innovation number
    public NodeConnection NewConnection(Node connectFrom, Node connectTo, float connectionWeight, bool enabled, bool debug, Genotype net)
    {
        //Increment global innovation number correctly
        for (int i = 0; i < net.connections.Count; i++) { if (net.connections[i].innovationNumber > net.globalInnovationNumber) { net.globalInnovationNumber = net.connections[i].innovationNumber + 1; } }
        /* Remember that connection weights are randomly assigned during creation */

        if ((connectFrom.type == NodeType.Input && connectTo.type == NodeType.Input) || (connectFrom.type == NodeType.Output && connectTo.type == NodeType.Output)) { Debug.LogError("Can't connect two of the same type"); return null; }
        if (connectFrom == connectTo) { Debug.LogError("Same nodes cannot be connected. "); return null; }

        if (connectFrom.connectsTo.Contains(connectTo)) { /*Debug.LogError("A connection already exists between these two nodes. ");*/ return null; }
        if (connectTo.connectsTo.Contains(connectFrom)) { Debug.LogError("Cannot have bi-directional connections between nodes. "); return null; }

        if (debug) { Debug.Log("<b>New Connection : </b> " + connectFrom.type + " ( " + connectFrom.innovationNumber + " )" + " --> " + connectTo.type + " ( " + connectTo.innovationNumber + " )" + "<i><color=red> Innovation Number " + (net.globalInnovationNumber + 1) + "</color></i>" + " <b> weight : " + connectionWeight + " </b>"); }

        NodeConnection con = new NodeConnection(connectFrom, connectTo);
        connectFrom.connectsTo.Add(connectTo);
        con.layerNumber = (connectFrom.layerNumber);

        con.enabled = enabled;

        //Important: Innovation number is assigned then incremented && Innovation number = same if mutation = same
        NodeConnection foundConnection;
        /*if (manager.ConnectionExists(con, out foundConnection))
        {
            con.innovationNumber = foundConnection.innovationNumber;
            manager.globalGenerationInnovationNumber = con.innovationNumber + 1;
        }
        else
        {
            con.innovationNumber = manager.globalGenerationInnovationNumber;
            manager.globalGenerationInnovationNumber++;
        }*/

        //Global Innov. Number is different but for inputs and stuff it should be the same
        con.innovationNumber = net.globalInnovationNumber;
        net.globalInnovationNumber++;



        con.weight = connectionWeight;



        net.AssignLayer(con);
        net.connections.Add(con);

        net.layerDictionary[connectTo.layerNumber].connectionsComingIn.Add(con);
        manager.globalConnectionList.Add(con);
        return con;
    }

    //This is based on ( https://towardsdatascience.com/neat-an-awesome-approach-to-neuroevolution-3eca5cc7930f )'s article on how to create new nodes/connections
    public (Node, NodeConnection) NewNode(NodeConnection alongConnection, bool debug, Genotype g)
    {
        if (alongConnection == null) { Debug.LogError("This connection is invalid"); return (null, null); }

        //Creating new node and assigning innovation number
        Node newNode = new Node(0f, NodeType.Hidden, UnityEngine.Random.Range(5, 1000000));

        //Keep track using a unique ID (Can be overlaps)
        g.uniqueNodeIdentifier.Add(newNode.uniqueID, newNode);

        //newNode.innovationNumber = globalInnovationNumber;

        //Apparently the innovation number for 'new' nodes are not tracked
        //globalInnovationNumber++;

        //Creating a new layer or assigining node to an existing layer
        newNode.layerNumber = (alongConnection.con1.layerNumber + alongConnection.con2.layerNumber) / 2;

        //Management in the network
        g.nodes.Add(newNode);
        g.hidden.Add(newNode);
        g.AssignLayer(newNode);

        //Ensure the prior connection is disabled
        alongConnection.enabled = false;
        alongConnection.replacedBy = newNode;

        //Using the new connection method as it will detect any issues w/ the connections
        NodeConnection aRef = NewConnection(alongConnection.con1, newNode, alongConnection.weight, true, debug, g);
        NewConnection(newNode, alongConnection.con2, 1, true, debug, g);

        //Debugging success
        if (debug)
        {
            Debug.Log("<b>New Node : </b>" + newNode.type.ToString() + " ( " + newNode.innovationNumber + " ) " + " at <color=blue>" + newNode.layerNumber + " </color> ");
            Debug.Log("<i> In Between : " + alongConnection.con1.type + " ( " + alongConnection.con1.layerNumber + " ) " + " and " + alongConnection.con2.type + " ( " + alongConnection.con2.layerNumber + " ) " + " </i>");
        }

        return (newNode, aRef);

    }

    //Ensuring that the connection doesn't exist within the system
    public bool ConnectionExists(NodeConnection a, Genotype g)
    {

        for (int i = 0; i < g.connections.Count; i++)
        {

            if (g.connections[i] == a)
                continue;

            if (g.connections[i].SameConnection(a))
            {
                return true;
            }

        }

        return false;

    }

    public bool ConnectionExists(Node b, Node c, Genotype g)
    {

        NodeConnection a = new NodeConnection(b, c);
        for (int i = 0; i < g.connections.Count; i++)
        {

            if (g.connections[i] == a)
                continue;

            if (g.connections[i].SameConnection(a))
            {
                return true;
            }

        }

        return false;

    }

    #endregion
}

[Serializable]
public class Species
{

    public string speciesName;
    public Color speciesColor;

    public List<Genotype> population = new List<Genotype>();
    public List<RandomWindGenerator> scripts = new List<RandomWindGenerator>();
    
}