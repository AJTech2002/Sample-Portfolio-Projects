/*

Referenced from : https://www.cs.ucf.edu/~kstanley/neat.html

*/

using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;

public class NNet : MonoBehaviour
{
    public string name;
    public float fitness;

    public NNet (string n, float initFitness)
    {
        name = n;
        fitness = initFitness;
    }

    public NNet() { }

    public bool mutateShow;
    public NNet crossWith;

    public int mutationChance;
    public int structuralChangeChance;

    public NNETVisualisation visualisation;

    private NEATManager mgr;
    public NEATManager manager
    {
        get
        {
            if (mgr == null)
                mgr = GameObject.FindObjectOfType<NEATManager>();
            return mgr;
        }
    }

    //Number of iterations till a successful match
    public int mutationTries = 100;
    
   
    public Genotype net;
    public GraphHelp help;

    public List<Node> nodes
    {
        get
        {
            return net.nodes;
        }
    }

    public List<NodeConnection> connections
    {
        get
        {
            return net.connections;
        }
    }

   

    public Genotype CrossOver (Genotype a, Genotype b)
    {

        Genotype newNet = InitialiseNetwork(0,0);

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
                float chance = Random.value;
                if (chance < 0.5f)
                {
                    NodeConnection aC = aConnections[i];
                    NodeConnection newConnection = new NodeConnection(null,null);

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

                    if (!net.nodes.Contains(aC.con2) && a2 == null)
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

                    newNet.connections.Add(newConnection);
                    newNet.AssignLayer(newConnection);
                    newNet.layerDictionary[aC.con2.layerNumber].connectionsComingIn.Add(newConnection);
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

                    if (!net.nodes.Contains(aC.con2) && a2 == null)
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

                    newNet.connections.Add(newConnection);
                    newNet.AssignLayer(newConnection);
                    newNet.layerDictionary[aC.con2.layerNumber].connectionsComingIn.Add(newConnection);
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

                if (!net.nodes.Contains(aC.con2) && a2 == null)
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

                newNet.connections.Add(newConnection);
                newNet.AssignLayer(newConnection);
                newNet.layerDictionary[aC.con2.layerNumber].connectionsComingIn.Add(newConnection);
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

                if (!net.nodes.Contains(aC.con2) && a2 == null)
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

                newNet.connections.Add(newConnection);
                newNet.AssignLayer(newConnection);
                newNet.layerDictionary[aC.con2.layerNumber].connectionsComingIn.Add(newConnection);
                continue;   
            }

            //Neither exists

        }

        //newNet.Redistribute();
        return newNet;
    }


    float t = 0;
    private void Plot (int upto)
    {

        //help.ResetMaxMin();
        help.ClearAllPoints();


        //help.Plot(0, 0, 0);
        for (int i = 0; i < upto; i++)
        {

            net.inputs[0].value = (Random.Range(0.00f,1.00f));
            net.inputs[1].value = (Random.Range(0.00f, 1.00f));
            net.inputs[2].value = (Random.Range(0.00f, 1.00f));

            for (int c = 0; c < net.layers.Count; c++)
            {
                
                if (c == 0)
                    continue;

                net.layers[c].FeedForward(net.layers[c - 1]);
            }

            help.Plot(i, net.outputs[0].value, 0);


            //print (net.outputs[0].value + " , " + net.outputs[1].value);

        }

    }

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
            Node n3 = new Node(0f, NodeType.Output, 100000, i+inputs);
            genotype.nodes.Add(n3);
            genotype.uniqueNodeIdentifier.Add(n3.uniqueID, n3);
        }

        genotype.Redistribute();


        //Assings to local var.
        net = genotype;
        return genotype;
    }

    
    public int globalInnovationNumber
    {
        get
        {
            return net.globalInnovationNumber;
        }
        set
        {
            net.globalInnovationNumber = value;
        }
    }
    
    
    

    float t2 = 0;
    public List<Node> RunNetwork (List<float> inputs)
    {

        t2 += 1;
        
        for (int i = 0; i < inputs.Count; i++)
        {
            net.inputs[i].value = inputs[i];
        }

        for (int c = 0; c < net.layers.Count; c++)
        {

            if (c == 0)
                continue;

            net.layers[c].FeedForward(net.layers[c - 1]);
        }

        return (net.outputs);

    }
    public void MutateNetwork(float mutationChance, float structualChangeChance)
    {

        net.Redistribute();
        int structuralChange = Random.Range(1, 101);

        int r = Random.Range(1, 101);
        if (r < mutationChance)
        {
            //Is the network going to be mutated structurally
            if (structuralChange <= structuralChangeChance)
            {

                //If so, is the connection going to be mutated or is there going to be a new ndoe
                int selection = Random.RandomRange(1, 3);

                //New node mutation
                if (selection == 1)
                {
                    if (net.connections.Count <= 0)
                    {
                        //Debug.Log("No connections exist.");
                        return;
                    }

                    int randCon = Random.Range(0, net.connections.Count);
                    NewNode(net.connections[randCon], false);

                }

                //New connection mutation
                else if (selection == 2)
                {
                    //Trying to find a good 2 nodes
                    for (int i = 0; i < 200; i++)
                    {
                        int randNodA = Random.Range(0, net.nodes.Count);
                        int randNodB = Random.Range(0, net.nodes.Count);

                        if (randNodA == randNodB)
                            continue;

                        if (nodes[randNodA].type == NodeType.Input && nodes[randNodB].type == NodeType.Input)
                            continue;

                        if (nodes[randNodA].type == NodeType.Output && nodes[randNodB].type == NodeType.Output)
                            continue;

                        //IMPORTANT : Connections are allowed between layers

                        //if (nodes[randNodA].layerNumber == nodes[randNodB].layerNumber)
                        //  continue;


                        if (nodes[randNodA].layerNumber < nodes[randNodB].layerNumber)
                            NewConnection(nodes[randNodA], nodes[randNodB], Random.Range(-1.00f, 1.00f), true, false);
                        else
                            NewConnection(nodes[randNodB], nodes[randNodA], Random.Range(-1.00f, 1.00f), true, false);

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
                int selection = Random.RandomRange(1, 3);

                if (selection == 1)
                {
                    int randCon = Random.Range(0, net.connections.Count);

                    if (net.connections.Count > randCon && net.connections[randCon] != null)
                    {
                        //Can be between -1f -> 1f or 0f to 1f (we need left & right)
                        net.connections[randCon].weight = Random.Range(-1.00f, 1.00f);
                    }
                }
                else
                {
                    int randCon = Random.Range(0, net.connections.Count);

                    if (net.connections.Count > randCon && net.connections[randCon] != null)
                        net.connections[randCon].enabled = !net.connections[randCon].enabled;
                }
                //Weight shift/mutate 
                //Enable/Disable switch random connection
            }

        }



    }
    //Creating a new connection between two nodes, contains error checking and edge cases + increments the global innovation number
    public NodeConnection NewConnection(Node connectFrom, Node connectTo, float connectionWeight, bool enabled, bool debug)
    {
        //Increment global innovation number correctly
        for (int i = 0; i < connections.Count; i++) { if (connections[i].innovationNumber > net.globalInnovationNumber) { net.globalInnovationNumber = connections[i].innovationNumber + 1; } }
        /* Remember that connection weights are randomly assigned during creation */

        if ((connectFrom.type == NodeType.Input && connectTo.type == NodeType.Input) || (connectFrom.type == NodeType.Output && connectTo.type == NodeType.Output)) { Debug.LogError("Can't connect two of the same type"); return null; }
        if (connectFrom == connectTo) { Debug.LogError("Same nodes cannot be connected. "); return null; }

        if (connectFrom.connectsTo.Contains(connectTo)) { /*Debug.LogError("A connection already exists between these two nodes. ");*/ return null; }
        if (connectTo.connectsTo.Contains(connectFrom)) { Debug.LogError("Cannot have bi-directional connections between nodes. "); return null; }

        if (debug) { Debug.Log("<b>New Connection : </b> " + connectFrom.type + " ( " + connectFrom.innovationNumber + " )" + " --> " + connectTo.type + " ( " + connectTo.innovationNumber + " )" + "<i><color=red> Innovation Number " + (globalInnovationNumber + 1) + "</color></i>" + " <b> weight : " + connectionWeight + " </b>"); }

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
    public (Node,NodeConnection) NewNode(NodeConnection alongConnection, bool debug)
    {
        if (alongConnection == null) { Debug.LogError("This connection is invalid"); return (null,null); }

        //Creating new node and assigning innovation number
        Node newNode = new Node(0f, NodeType.Hidden, Random.Range(5,1000000));

        //Keep track using a unique ID (Can be overlaps)
        net.uniqueNodeIdentifier.Add(newNode.uniqueID, newNode);

        //newNode.innovationNumber = globalInnovationNumber;

        //Apparently the innovation number for 'new' nodes are not tracked
        //globalInnovationNumber++;

        //Creating a new layer or assigining node to an existing layer
        newNode.layerNumber = (alongConnection.con1.layerNumber + alongConnection.con2.layerNumber) / 2;

        //Management in the network
        net.nodes.Add(newNode);
        net.hidden.Add(newNode);
        net.AssignLayer(newNode);
       
        //Ensure the prior connection is disabled
        alongConnection.enabled = false;
        alongConnection.replacedBy = newNode;

        //Using the new connection method as it will detect any issues w/ the connections
        NodeConnection aRef = NewConnection(alongConnection.con1, newNode, alongConnection.weight, true, debug);
        NewConnection(newNode, alongConnection.con2, 1, true, debug);

        //Debugging success
        if (debug)
        {
            Debug.Log("<b>New Node : </b>" + newNode.type.ToString() + " ( " + newNode.innovationNumber + " ) " + " at <color=blue>" + newNode.layerNumber + " </color> ");
            Debug.Log("<i> In Between : " + alongConnection.con1.type + " ( " + alongConnection.con1.layerNumber + " ) " + " and " + alongConnection.con2.type + " ( " + alongConnection.con2.layerNumber + " ) " + " </i>");
        }

        return (newNode,aRef);

    }

    //Ensuring that the connection doesn't exist within the system
    public bool ConnectionExists(NodeConnection a)
    {

        for (int i = 0; i < connections.Count; i++)
        {

            if (connections[i] == a)
                continue;

            if (connections[i].SameConnection(a))
            {
                return true;
            }

        }

        return false;

    }

    public bool ConnectionExists(Node b, Node c)
    {

        NodeConnection a = new NodeConnection(b, c);
        for (int i = 0; i < connections.Count; i++)
        {

            if (connections[i] == a)
                continue;

            if (connections[i].SameConnection(a))
            {
                return true;
            }

        }

        return false;

    }


}

//Genotype is genetic representation of a creature
[System.Serializable]
public class Genotype
{

    public List<Node> nodes = new List<Node>();
    public List<NodeConnection> connections = new List<NodeConnection>();

    public List<Node> inputs = new List<Node>();
    public List<Node> outputs = new List<Node>();
    public List<Node> hidden = new List<Node>();

    public List<Layer> layers = new List<Layer>();
    public Dictionary<float, Layer> layerDictionary = new Dictionary<float, Layer>();
    public Dictionary<int, Node> uniqueNodeIdentifier = new Dictionary<int, Node>();

    public int globalInnovationNumber = 0;

    public void Redistribute()
    {
        inputs.Clear();
        outputs.Clear();
        hidden.Clear();

        for (int i = 0; i < nodes.Count; i++)
        {

            if (nodes[i].type == NodeType.Input)
                inputs.Add(nodes[i]);

            if (nodes[i].type == NodeType.Hidden)
                hidden.Add(nodes[i]);

            if (nodes[i].type == NodeType.Output)
                outputs.Add(nodes[i]);

            AssignLayer(nodes[i]);
        }



    }

    //This creates new layers based on nodes
    public void AssignLayer (Node n)
    {

        if (layerDictionary.ContainsKey(n.layerNumber))
        {
            if (!layerDictionary[n.layerNumber].nodes.Contains(n))
            layerDictionary[n.layerNumber].nodes.Add(n);
            
        }
        else
        {
            Layer l = new Layer();
            l.layerID = n.layerNumber;
            l.nodes.Add(n);

            layers.Add(l);
            layerDictionary.Add(l.layerID, l);
            
        }

        SortLayers();
    }

    public void AssignLayer (NodeConnection c)
    {
        if (layerDictionary.ContainsKey(c.layerNumber))
        {
            if (!layerDictionary[c.layerNumber].connections.Contains(c))
                layerDictionary[c.layerNumber].connections.Add(c);
        }
        else
        {
            Layer l = new Layer();
            l.layerID = c.layerNumber;
            l.connections.Add(c);

            layers.Add(l);
            layerDictionary.Add(l.layerID, l);
        }

        SortLayers();
    }

    private void SortLayers()
    {
        //Sort the layers based on layer iD
        for (int j = 0; j <= layers.Count - 2; j++)
        {
            for (int i = 0; i <= layers.Count - 2; i++)
            {
                if (layers[i].layerID > layers[i + 1].layerID)
                {
                    Swap<Layer>(layers, i, i + 1);
                }
            }
        }

    }

    public static void Swap<T>(IList<T> list, int indexA, int indexB)
    {
        T tmp = list[indexA];
        list[indexA] = list[indexB];
        list[indexB] = tmp;
    }


}

public struct NodeValuePair
{
    Node n;
    bool connected;
    float weight;
}

[System.Serializable]
public class Layer
{

    public float layerID;
    //This is the nodes within that layer
    public List<Node> nodes = new List<Node>();
    public List<NodeConnection> connectionsComingIn = new List<NodeConnection>();

    //This is the list of connections going OUT of the layer into the next one

    private List<NodeConnection> cons = new List<NodeConnection>();

    public List<NodeConnection> connections
    {
        get
        {
            return cons;
        }
        
        set
        {
            cons = value;

        }
    }
  

    public Matrix<float> GenerateLayerMatrix ()
    {
        Matrix<float> newMatrix = Matrix<float>.Build.Dense(1, nodes.Count);

        for (int x = 0; x < newMatrix.ColumnCount; x++)
        {
            newMatrix[0, x] = nodes[x].value;
        }

        //Debug.Log("<b> Layer  ( " + layerID + " ) </b> : " + newMatrix.ToString());

        return newMatrix;
    }

    public Matrix<float> GenerateWeightMatrix(Layer nextLayer)
    {
        int columns = nodes.Count;
        int rows = nextLayer.nodes.Count;

        Matrix<float> newMatrix = Matrix<float>.Build.Dense(rows, columns);

        for (int i = 0; i < newMatrix.RowCount; i++)
        {

            for (int c = 0; c < newMatrix.ColumnCount; c++)
            {

                Node nextLayerNode = nextLayer.nodes[i];
                Node currentLayerNode = nodes[c];

                bool connectionExists = false;
                NodeConnection foundConnection = null;
                for (int x = 0; x < connections.Count; x++) {
                    if (connections[x].con2 == nextLayerNode && connections[x].con1 == currentLayerNode) {

                        //Debug.Log("Found connection between : " + connections[x].con1.layerNumber + "INNOV : " + connections[x].con1.innovationNumber + " and " + connections[x].con2.layerNumber + "INNOV : " + connections[x].con2.innovationNumber +  " with weight " + connections[x].weight + " enabled? : " + connections[x].enabled);
                        connectionExists = true;
                        foundConnection = connections[x];
                    }
                }

                if (connectionExists && foundConnection.enabled) newMatrix[i, c] = foundConnection.weight;
                else newMatrix[i, c] = 0f;

            }

        }

        ////Debug.Log("<b> Weight : </b> " + newMatrix.ToString());

        return newMatrix;
    }

    public Matrix<float> GenerateRealConnectionMatrix()
    {
        Matrix<float> newMatrix;

        if (connectionsComingIn.Count <= 0)
        {
            newMatrix = Matrix<float>.Build.Dense(1, 1);
            newMatrix[0, 0] = 0f;
        }
        else
        {
            newMatrix = Matrix<float>.Build.Dense(1, connectionsComingIn.Count); 
        }

        for (int x = 0; x < newMatrix.ColumnCount; x++)
        {
            if (newMatrix.RowCount > 0 && newMatrix.ColumnCount > 0 && connectionsComingIn.Count > 0)
                newMatrix[0, x] = connectionsComingIn[x].con1.value;
        }

        //Debug.Log("<b> Layer  ( " + layerID + " ) </b> : " + newMatrix.ToString());

        return newMatrix;
    }

    public Matrix<float> GenerateRealWeightMatrix()
    {
        int columns = nodes.Count;
        int rows = connectionsComingIn.Count;
        Matrix<float> newMatrix;

        if (columns > 0 && rows > 0)
         newMatrix = Matrix<float>.Build.Dense(rows, columns);
        else
        {
            columns = Mathf.Clamp(columns, 1, 100000);
            rows = Mathf.Clamp(rows, 1, 100000);
            newMatrix = Matrix<float>.Build.Dense(rows, columns);
        }

        for (int i = 0; i < newMatrix.RowCount; i++)
        {

            for (int c = 0; c < newMatrix.ColumnCount; c++)
            {
                if (i > connectionsComingIn.Count - 1)
                    continue;

                Node currentNode = nodes[c];
                NodeConnection targettingConnection = connectionsComingIn[i];

                if (targettingConnection.enabled)
                {
                    if (targettingConnection.con2 == currentNode)
                    {
                        newMatrix[i, c] = targettingConnection.weight;
                    }
                }
                else
                {
                    newMatrix[i, c] = 0f;
                }

            }

        }

        ////Debug.Log("<b> Weight : </b> " + newMatrix.ToString());

        return newMatrix;
    }

    /*
     * Node nextLayerNode = nextLayer.nodes[i];
                Node currentLayerNode = nodes[c];

                bool connectionExists = false;
                NodeConnection foundConnection = null;
                for (int x = 0; x < connections.Count; x++)
                {
                    if (connections[x].con2 == nextLayerNode && connections[x].con1 == currentLayerNode)
                    {

                        //Debug.Log("Found connection between : " + connections[x].con1.layerNumber + "INNOV : " + connections[x].con1.innovationNumber + " and " + connections[x].con2.layerNumber + "INNOV : " + connections[x].con2.innovationNumber +  " with weight " + connections[x].weight + " enabled? : " + connections[x].enabled);
                        connectionExists = true;
                        foundConnection = connections[x];
                    }
                }

                if (connectionExists && foundConnection.enabled) newMatrix[i, c] = foundConnection.weight;
                else newMatrix[i, c] = 0f;
                */

    public void FeedForward(Layer priorLayer, bool sigmoid)
    {
        Matrix<float> priorLayerNodes = null;
        if (priorLayer.layerID == 0)
            priorLayerNodes = SigmoidActivateMatrix(GenerateRealConnectionMatrix());
        else
            priorLayerNodes = GenerateRealConnectionMatrix();
       
        Matrix<float> priorLayerWeights = GenerateRealWeightMatrix();

        //TODO: This shouldn't actually be a sigmoid activation and should be a TAN-H Activation
        Matrix<float> finalValues = SigmoidActivateMatrix(priorLayerNodes * priorLayerWeights);
        //Debug.Log("<b> Layer (" + priorLayer.layerID + ") </b> : [[" + priorLayerNodes.ToMatrixString() + "]] <color=red>*</color> [[" + priorLayerWeights.ToMatrixString() + "]] <color=red> == </color> [[" + finalValues.ToMatrixString()+"]]");
        if (finalValues.ColumnCount == nodes.Count)
        {

            for (int i = 0; i < finalValues.ColumnCount; i++)
            {

                nodes[i].value = finalValues[0, i];

            }

        }
        else { Debug.LogError("Feedforward Errors "); }

    }
    public void FeedForward(Layer priorLayer)
    {
        Matrix<float> priorLayerNodes = null;
        if (priorLayer.layerID == 0)
            priorLayerNodes = GenerateRealConnectionMatrix().PointwiseTanh();
        else
            priorLayerNodes = GenerateRealConnectionMatrix();

        Matrix<float> priorLayerWeights = GenerateRealWeightMatrix();

        //TODO: This shouldn't actually be a sigmoid activation and should be a TAN-H Activation
        Matrix<float> finalValues = (priorLayerNodes * priorLayerWeights).PointwiseTanh();
        //Debug.Log("<b> Layer (" + priorLayer.layerID + ") </b> : [[" + priorLayerNodes.ToMatrixString() + "]] <color=red>*</color> [[" + priorLayerWeights.ToMatrixString() + "]] <color=red> == </color> [[" + finalValues.ToMatrixString()+"]]");
        if (finalValues.ColumnCount == nodes.Count)
        {

            for (int i = 0; i < finalValues.ColumnCount; i++)
            {

                nodes[i].value = finalValues[0, i];

            }

        }
        else { Debug.LogError("Feedforward Errors "); }

    }

    //Only if you want positive values
    public Matrix<float> SigmoidActivateMatrix (Matrix<float> inp)
    {
        Matrix<float> rand = inp;
        for (int x = 0; x < inp.RowCount; x++)
        {
            for (int y = 0; y < inp.ColumnCount; y++)
            {
                rand[x, y] = SigmoidActivate(rand[x, y]);
            }
        }
        return rand;
    }

    public float SigmoidActivate (float f)
    {
        return (1 / (1 + Mathf.Exp(-f)));
    }


}

//These are the genes or the nodes within the network
 
public class Node
{
    public int uniqueID;
    
    public string nodeName;
    public float value;
    public int innovationNumber;
    public NodeType type;

    public float layerNumber;

    //This is a forward connection list -- what does this node connect to in the future
    [HideInInspector]
    public List<Node> connectsTo = new List<Node>();
    public ActivationType activationType;


    public Node(float val, NodeType nodeType, int uniqueID)
    {
        value = val;
        type = nodeType;
        this.uniqueID = uniqueID;
    }

    public Node(float val, NodeType nodeType, int layerNum, int uniqueID)
    {
        value = val;
        type = nodeType;
        layerNumber = layerNum;
        this.uniqueID = uniqueID;
    }


    public Node(string name, float val, NodeType nodeType, int uniqueID)
    {
        value = val;
        nodeName = name;
        type = nodeType;
        this.uniqueID = uniqueID;
    }

    public Node(string name, float val, NodeType nodeType, int layerNum, int uniqueID)
    {
        value = val;
        nodeName = name;
        type = nodeType;
        layerNumber = layerNum;
        this.uniqueID = uniqueID;
    }

}

[System.Serializable]
public enum NodeType { Input, Hidden, Output };

[System.Serializable]
public enum ActivationType {Sigmoid, RELU, LinearCutoff};

[System.Serializable]
public class NodeConnection
{

    //The two connections of the gene connection
    public Node con1;
    public Node con2;

    public float weight;
    public bool enabled;

    public int innovationNumber;
    public float layerNumber;

    public Node replacedBy;

    //Check if two connections are similar with their connections
    public bool SameConnection(NodeConnection obj)
    {

        if (obj.con1 == this.con1 && obj.con2 == this.con2)
            return true;

        if (obj.con2 == this.con1 && obj.con1 == this.con2)
            return true;

        if (obj.con1.uniqueID == this.con1.uniqueID && obj.con2.uniqueID == this.con2.uniqueID)
            return true;

        if (obj.con2.uniqueID == this.con1.uniqueID && obj.con1.uniqueID == this.con2.uniqueID)
            return true;

        if (obj.con1 == this.con1 && obj.con2.uniqueID == this.con2.uniqueID)
            return true;

        if (obj.con2 == this.con2 && obj.con1.uniqueID == this.con1.uniqueID)
            return true;

        if (obj.con1 == this.con2 && obj.con2.uniqueID == this.con1.uniqueID)
            return true;

        if (obj.con2 == this.con1 && obj.con1.uniqueID == this.con2.uniqueID)
            return true;

        return false;

    }

    public NodeConnection(Node a, Node b)
    {
        con1 = a;
        con2 = b;
        weight = 1f;
        enabled = true;
        innovationNumber = 0;
    }

}
