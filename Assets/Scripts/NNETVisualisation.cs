using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
public class NNETVisualisation : MonoBehaviour
{
    public GenePool manager;
    public NNet net;
    public RectTransform trans;

    public Transform max;
    public Transform min;

    public Transform prefabSprite;
    public Transform lineRenderer;

    [Header("Visual Properties")]
    public float spacing;
    public float layerSpacing;
    
    private void Start()
    {
        UpdateNetwork();
    }

    List<Transform> allCreated = new List<Transform>();

    Dictionary<Node, Transform> nodeDictionary = new Dictionary<Node, Transform>();
    Dictionary<NodeConnection, Transform> nodeConnectionDictionary = new Dictionary<NodeConnection, Transform>();


    private void LateUpdate()
    {

        if (manager != null && manager.cartScripts != null && manager.cartScripts.Count > 0)
        {
            RandomWindGenerator gen = manager.cartScripts[0];
            for (int i = 0; i < manager.cartScripts.Count; i++)
            {
                if (manager.cartScripts[i].currentFitness > gen.currentFitness)
                {
                    gen = manager.cartScripts[i];
                }
            }

            net = gen.network;
            UpdateNetwork();
        }
        else { //Debug.LogError("Something went wrong here"); 
        }



    }

    public void UpdateNetwork()
    {

        float maxX = max.position.x;
        float minX = min.position.x;
        float maxY = max.position.y;
        float minY = min.position.y;
        float maxNodeY = minY;
        for (int i = 0; i < allCreated.Count; i++) { GameObject.Destroy(allCreated[i].gameObject); }
        allCreated.Clear();

        nodeDictionary.Clear();
        nodeConnectionDictionary.Clear();

        int divisions = net.net.layers.Count;
        float divisionWidth = (maxX + minX) / divisions;

        for (int i = 0; i < net.net.layers.Count; i++)
        {
            int nodeDivisions = net.net.layers[i].nodes.Count;
            float heightDivision = spacing;

            float layerX = minX + divisionWidth * i;

            for (int n = 0; n < net.net.layers[i].nodes.Count; n++)
            {
                float nodeY = minY - heightDivision * n;
                
                /*print(nodeY + " n " + n);

                float nodeY = (minY + maxY) * (n / net.net.layers[i].nodes.Count);
                float subbed = minY - maxY;
                float perc = subbed * ((float)n / (float)net.net.layers[i].nodes.Count);
                float nodeY = perc + maxY;*/

                Vector3 position = new Vector3(layerX, nodeY, 0);

                if (!nodeDictionary.ContainsKey(net.net.layers[i].nodes[n]))
                {
                    Transform t = Instantiate(prefabSprite, position, Quaternion.identity, trans) as Transform;
                    nodeDictionary.Add(net.net.layers[i].nodes[n], t);
                    allCreated.Add(t);
                }
                else
                {

                    //nodeDictionary[net.net.layers[i].nodes[n]].position = position;
                }

            }

        }



        for (int i = 0; i < net.net.layers.Count; i++)
        {
            for (int c = 0; c < net.net.layers[i].connections.Count; c++)
            {
                
                Transform t = null;
                if (!nodeConnectionDictionary.ContainsKey(net.net.layers[i].connections[c]))
                {
                    t = (Transform)Instantiate(lineRenderer, nodeDictionary[net.net.layers[i].connections[c].con1].position, Quaternion.identity, trans) as Transform;
                    nodeConnectionDictionary.Add(net.net.layers[i].connections[c], t);
                    allCreated.Add(t);
                }
                else
                {
                    //t = nodeConnectionDictionary[net.net.layers[i].connections[c]];
                }


                t.GetComponent<UILineRenderer>().Points[0] = t.InverseTransformPoint(nodeDictionary[net.net.layers[i].connections[c].con1].position);
                t.GetComponent<UILineRenderer>().Points[1] = t.InverseTransformPoint(nodeDictionary[net.net.layers[i].connections[c].con2].position);

                if (net.net.layers[i].connections[c].enabled)
                {
                    t.GetComponent<UILineRenderer>().color = Random.ColorHSV();
                }
                else
                {

                    t.GetComponent<UILineRenderer>().color = Color.red;

                    if (net.net.layers[i].connections[c].replacedBy != null)
                    {
                        Vector3 avgMid = new Vector3((t.GetComponent<UILineRenderer>().Points[0].x + t.GetComponent<UILineRenderer>().Points[1].x) / 2, (t.GetComponent<UILineRenderer>().Points[0].y + t.GetComponent<UILineRenderer>().Points[1].y) / 2, 0);
                        avgMid = t.TransformPoint(avgMid);
                        if (nodeDictionary.ContainsKey(net.net.layers[i].connections[c].replacedBy))
                        nodeDictionary[net.net.layers[i].connections[c].replacedBy].position = avgMid;
                    }

                }

                float perc = net.net.layers[i].connections[c].weight;
                float width = 0.5f + perc * 1.5f;

                t.GetComponent<UILineRenderer>().LineThickness = width;
            }
        }

        for (int i = 0; i < net.net.layers.Count; i++)
        {
            for (int c = 0; c < net.net.layers[i].connections.Count; c++)
            {
                Transform t = nodeConnectionDictionary[net.net.layers[i].connections[c]];
                t.GetComponent<UILineRenderer>().Points[0] = t.InverseTransformPoint(nodeDictionary[net.net.layers[i].connections[c].con1].position);
                t.GetComponent<UILineRenderer>().Points[1] = t.InverseTransformPoint(nodeDictionary[net.net.layers[i].connections[c].con2].position);

            }

        }

    }
    

}
