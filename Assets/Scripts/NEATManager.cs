using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NEATManager : MonoBehaviour
{
    //List of all networks in the current generation
    public List<NNet> nets = new List<NNet>();

    //The threshold before a new species is created
    [Header("Speciation Scores")]
    public float speciationThreshold;
    public float disjointWeight;
    public float excessWeight;
    public float weightDifWeight;

    public UnityEngine.UI.Text speciationText;

    public int globalGenerationInnovationNumber = 0;

    //Over Generations (Clear this every generation)
    public List<NodeConnection> globalConnectionList = new List<NodeConnection>();
    private List<NodeConnection> connections
    {
        get
        {
            return globalConnectionList;
        }
    }

    private void Update()
    {
        speciationText.text = SpeciationScore(nets[0].net, nets[1].net).ToString();
    }

    //Speciation is the ability to match networks that are similar, this will result in better crossovers as the paper mentions
    //All crossovers are not good crossovers
    public float SpeciationScore (Genotype a, Genotype b)
    {
        float disjointNodes = 0;
        float excessNodes = 0;

        //This is the average weight difference of matching genes
        float avgWeightDif = 0f;

        //Align Innovation Numbers
        Dictionary<int, NodeConnection> aConnections = new Dictionary<int, NodeConnection>();
        Dictionary<int, NodeConnection> bConnections = new Dictionary<int, NodeConnection>();

        for (int i = 0; i < a.connections.Count; i++)
        {
            if (!aConnections.ContainsKey(a.connections[i].innovationNumber))
                aConnections.Add(a.connections[i].innovationNumber, a.connections[i]);
            
            //Remember the way you're crossing over you are having innovation overlaps which is killing some genomes unncessarily

            //else
               // Debug.LogWarning("You're loosing gnomes");
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

        excessNodes = bConnections.Count - aConnections.Count;

        for (int i = 0; i < max; i++)
        {

            if (aConnections.ContainsKey(i) && bConnections.ContainsKey(i))
            {
                Node a1 = aConnections[i].con1;
                if (a.uniqueNodeIdentifier.ContainsKey(aConnections[i].con1.uniqueID))
                    a1 = a.uniqueNodeIdentifier[aConnections[i].con1.uniqueID];
                
                Node a2 = aConnections[i].con2;
                if (a.uniqueNodeIdentifier.ContainsKey(aConnections[i].con2.uniqueID))
                    a2 = a.uniqueNodeIdentifier[aConnections[i].con2.uniqueID];


                Node b1 = bConnections[i].con1;
                if (b.uniqueNodeIdentifier.ContainsKey(bConnections[i].con1.uniqueID))
                    b1 = b.uniqueNodeIdentifier[bConnections[i].con1.uniqueID];

                Node b2 = bConnections[i].con2;
                if (b.uniqueNodeIdentifier.ContainsKey(bConnections[i].con2.uniqueID))
                    b2 = b.uniqueNodeIdentifier[bConnections[i].con2.uniqueID];

                NodeConnection falseConA = new NodeConnection(a1, a2);
                NodeConnection falseConB = new NodeConnection(b1, b2);

                //print(falseConA.con1.uniqueID + " vs " + falseConB.con1.uniqueID);

                if (aConnections[i].SameConnection(bConnections[i]))
                {
                    float weightDif = Mathf.Abs(aConnections[i].weight-bConnections[i].weight);
                    avgWeightDif += weightDif;
                    
                }
                else if (falseConA.SameConnection(falseConB))
                {
                    float weightDif = Mathf.Abs(aConnections[i].weight - bConnections[i].weight);
                    avgWeightDif += weightDif;
                    
                }
                else
                {
                    disjointNodes++;
                }
            }
            else
            {
                disjointNodes++;
            }

        }

        disjointNodes /= max;
        excessNodes /= max;
        avgWeightDif /= max;

        disjointNodes *= disjointWeight;
        excessNodes *= excessWeight;
        avgWeightDif *= weightDifWeight;

        

        return (Mathf.Clamp(disjointNodes+excessNodes+avgWeightDif,0f,Mathf.Infinity));

    }

    public bool ConnectionExists(NodeConnection a, out NodeConnection val)
    {

        for (int i = 0; i < connections.Count; i++)
        {

            if (connections[i] == a)
                continue;

            if (connections[i].SameConnection(a))
            {
                val = a;
                return true;
            }

        }

        val = null;
        return false;

    }

    public bool ConnectionExists(Node b, Node c, out NodeConnection val)
    {

        NodeConnection a = new NodeConnection(b, c);
        for (int i = 0; i < connections.Count; i++)
        {

            if (connections[i] == a)
                continue;

            if (connections[i].SameConnection(a))
            {
                val = a;
                return true;
            }

        }

        val = null;
        return false;

    }
}
