using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphHelp : MonoBehaviour {


	#region Elements
	[Header("Design Elements")]
	public Vector3 backgroundColor = Vector3.zero;
	public Vector3 axisColor = Vector3.one;
	public Vector3 gridColor = Vector3.one;
	public Vector3 indicatedColor = Vector3.zero;
	[Header("Axis")]
	[Range(2,100)]
	public int valueDetailX;
	[Range(2,100)]
	public int valueDetailY;

	//[Header("Graphs")]
	[HideInInspector]
	public List<Graph> graphs = new List<Graph>();
	[HideInInspector]
	public Dictionary<string, int> graphID = new Dictionary<string, int> ();
	//[HideInInspector]
	public float TMaxX=0,TMinX;
	//[HideInInspector]
	public float TMaxY=0,TMinY;
	[HideInInspector]
	public Vector2 indicatedPoint;

	[HideInInspector]
	public float backUpX, backUpY;

	[HideInInspector]
	public string printString;

	#endregion

	public void PrintStr(string s) {
		printString = s;
	}

	public float derivative (float x) {
		return activate (x) * (1 - activate (x));
	}

	public float activate (float x) {
		return 1 / (1 + Mathf.Exp (-x));
	}

	public void AddGraph (string name, Color col) {
		//print ("Graph Added : " + name + " of Index : " + graphs.Count + " (Color : " + col.ToString());
		if (graphID.ContainsKey (name))
			return;
		graphs.Add (new Graph (name, col));
		graphID.Add (name, graphs.Count - 1);
	}

	public void Plot (float x, float y, int graph) {
		if (graphs [graph].points.Count == 0) {
			//TMaxX = x;
			TMinX = x;
			TMinY = y;
			//TMaxY = y;
		}
		if (x > TMaxX) {
			TMaxX = x;
			backUpX = TMaxX;
		}
		if (y > TMaxY) {
			TMaxY = y;
			backUpY = TMaxY;
		}

		if (x < TMinX)
			TMinX = x;
		if (y < TMinY)
			TMinY = y;

		graphs[graph].points.Add (new Vector2 (x, y));
	}

	public void Plot (float x, float y, string graphKey) {
		if (graphID.ContainsKey (graphKey)) {
			int graph = graphID [graphKey];
			if (graphs [graph].points.Count == 0) {
				//TMaxX = x;
				TMinX = x;
				TMinY = y;
				//TMaxY = y;
			}
			if (x > TMaxX) {
				TMaxX = x;
				backUpX = TMaxX;
			}
			if (y > TMaxY) {
				TMaxY = y;
				backUpY = TMaxY;
			}

			if (x < TMinX)
				TMinX = x;
			if (y < TMinY)
				TMinY = y;

			graphs [graph].points.Add (new Vector2 (x, y));
			return;
		}
		print ("Nonexistant graph");
	}

	public void IndicatePoint (float x, float y) {
		indicatedPoint = new Vector2 (x, y);
	}

	public void ClearPoints(int graph) {
		ResetMaxMin ();
		graphs[graph].points.Clear ();
	}

	public void ClearAll () {
		ResetMaxMin ();
		graphs.Clear ();
		graphID.Clear ();
	}

	public void ResetMaxMin() {
		TMaxX = 0;
		TMinX = 0;
		TMaxY = 0;
		TMinY = 0;
	}

	public void RemoveGraph (int graph) {
		graphs.RemoveAt (graph);
		//graphID.Remove ();
	}

	public void ClearAllPoints () {
		ResetMaxMin ();
		for (int i = 0; i < graphs.Count; i++) {
			graphs [i].points.Clear ();
		}
	}

	public void RemoveGraph (string key) {
		graphs.Remove (graphs [graphID [key]]);
		//graphID.Remove (key);
	}

	public void PassResetBatch (string graph, params Vector2[] points) {
		graphs [graphID [graph]].points.Clear ();
		for (int i = 0; i < points.Length; i++) {
			graphs [graphID [graph]].points.Add (points [i]);
		}
	}

}

[System.Serializable]
public class Graph {
	public string name;
	public List<Vector2> points = new List<Vector2>();
	public Color col = Color.white;

	public Graph (string _name, Color _col) {
		name = _name;
		col = _col;
	}

}
