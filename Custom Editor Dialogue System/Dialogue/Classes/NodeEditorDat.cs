using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NodeEditorDat {

    //List of the nodes
	public List<DNode> nodes = new List<DNode>();
	public string savePath = "";

	public int connectionIDCount = 0;

	public ConnectionDict connections;
	public SDict propertyDict;

	public NodeEditorDat (string _path, List<DNode> _nodes, int _connectionIDCount, ConnectionDict dict, SDict propertyDictionary)  {
		savePath = _path;
		nodes = _nodes;
		connectionIDCount = _connectionIDCount;
		connections = dict;
		propertyDict = propertyDictionary;
	}

}
