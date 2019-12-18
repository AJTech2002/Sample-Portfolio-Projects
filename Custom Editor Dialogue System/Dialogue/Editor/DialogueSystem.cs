using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

public class DialogueSystem : EditorWindow {
   
    #region Setup

    [MenuItem("EditorPack/Dialogue System")]
    private static void Init() {
		EditorWindow w = EditorWindow.GetWindow(typeof(DialogueSystem),false,"Dialogue System");
        w.Show();
    }

    #endregion

    #region Private Variables

    private bool clickedThisFrame = false;
    private Vector2 clickVector = Vector2.zero;

    private bool draggingThisFrame = false;
    private Vector2 draggingVector = Vector2.zero;
    //Need to call dat = new NodeEditorDat();
    private NodeEditorDat dat;

    //Serialise this into ^^ After
	public List<DNode> nodes = new List<DNode>();
	private int connectionIDCount = 0;


    //Types of nodes
    public enum NodeType {
        Player,
        NPC,
        Start,
        Gate
    }

    #endregion

	#region Main
	private GUIStyle centerLabel = new GUIStyle();
	private GUIStyle wordWrap = new GUIStyle();
    private void OnGUI()
    {
		if (wordWrap.wordWrap == false)
			wordWrap.wordWrap = true;

		Rect labeler = o (heading, 10, 50, 400, 300);
		GUI.Label (labeler, "Shift + Left Click = Delete \n" +
			" Mouse 3 Drag = Pan \n" +
			" Shift + Mouse 3 on Input = Delete Connections \n" +
			" Shift + Mouse Down on Node = Delete Node \n" +
			" Alt + Mouse 3 = REFRESH \n" +
			" RIGHT MOUSE = Node Selection \n" +
			" Alt + Click = INSPECTOR \n" +
			" Top Left Click Node = FOLD TOGGLE \n" +
			" REFRESH to Re-Center \n" +
			" Shift + Spacebar x 2 = REFRESH \n" + 
			" Shift + Mouse 3 = FOLD NODES \n" + 
			" Shift + Mouse 3 + Drag Parent = DRAG FOLD \n " +
			" ALT + MOVE NODE = DRAG ALL NODES WITH IT", wordWrap);

		NodeHandler();
		OnBaseScreen();
		Inspector ();
		OnDetectClick();
		ScreenEventHandler();
        Repaint();


		if (connections == null )
			connections = new ConnectionDict ();

    }
	#endregion	

	#region Inspector System

	#region Vars
	private bool drawInspector = false;
	private Rect inspectorRect = new Rect();
	private bool clickedInspector;
	private Rect constantInspectorRect = new Rect();
	private bool propertiesTab = true;
	private Rect finalNodeScroll = new Rect ();
	private bool selectedNode;
	public DNode currentlySelected = new DNode();
	private Rect finalScroll = new Rect();
	public SDict properties = new SDict();
	#endregion

	private void Inspector() {
		if (properties == null)
			properties = new SDict ();


		if (clickedThisFrame) {
			if (constantInspectorRect.Contains (Event.current.mousePosition)) {
				clickedInspector = true;
			}
		} else {
			if (clickedInspector) {
				if (constantInspectorRect.Contains (Event.current.mousePosition)) {
					clickedInspector = true;
				} else
					clickedInspector = false;
			}
		}

		if (drawInspector) {
			inspectorRect = r (width - 250, 0 + heading.height, 250, height);
			constantInspectorRect = r (width - 250, 0 + heading.height, 250, height);
			DrawRectBox (inspectorRect, "");
		} else {
			inspectorRect = r (0, 0, 0, 0);
			constantInspectorRect = r (0, 0, 0, 0);
		}



		InspectorEvents ();

	}

	private void InspectorEvents() {
		if (propertiesTab) {
			inspectorRect.position = GUI.BeginScrollView (inspectorRect, finalScroll.position, finalScroll);
			GUI.Label (o (inspectorRect, 70, 30, 160, 30), "Properties");

			Rect tabSwitch = o (inspectorRect, 0, 0, inspectorRect.width, 20);
			if (GUI.Button (tabSwitch, "()")) {
				propertiesTab = !propertiesTab;
			}

			DisplayPropertiesGUI ();

		} else {
			inspectorRect.position = GUI.BeginScrollView (inspectorRect, finalNodeScroll.position, finalNodeScroll);
			//GUI.Label (o (inspectorRect, 70, 30, 160, 30), "Node Change");
			Rect tabSwitch = o (inspectorRect, 0, 0, inspectorRect.width, 20);
			if (GUI.Button (tabSwitch, "()")) {
				propertiesTab = !propertiesTab;
			}

			DisplayNodePropertiesGUI ();

		}
	}

	private bool effectors = false;
	private void DisplayNodePropertiesGUI() {
		Rect guiLabel = o (inspectorRect, 5, 25, inspectorRect.width - 10, 25);
		if (selectedNode) {
			EditorGUI.LabelField (guiLabel, currentlySelected.nodeType, centerLabel);	
			Rect chatBox = yo (guiLabel, 0, 20, inspectorRect.width - 10, 100);
			currentlySelected.chat = GUI.TextArea (chatBox, currentlySelected.chat);


			Rect addProps = yo (chatBox,0 , 20, inspectorRect.width - 10, 20);
			Rect clearProps = yo (addProps, 0, 5, inspectorRect.width - 10, 20);

			if (GUI.Button (addProps, "See Checkers")) {
				effectors = false;
			}

			if (GUI.Button (clearProps, "See Effectors")) {
				effectors = true;
			}

			if (effectors) {
				SeeEffectors (clearProps);
			} else {
				SeeCheckers (clearProps);
			}


		} else {
			EditorGUI.LabelField (guiLabel, "You need to select a node first...", centerLabel);
			GUI.EndScrollView ();
		}
			
	}


	private void SeeEffectors(Rect end) {
		Rect addProps = o (end, 0, 70, 125, 20);
		Rect clearProps = xo (addProps, 5, 0, 110, 20);

		if (GUI.Button (addProps, "Add Effectors")) {
			if (properties.keys.Count <= 0)
				currentlySelected.effectors.Add (new CEffector ());
			else
				currentlySelected.effectors.Add (new CEffector (properties.keys [properties.keys.Count - 1]));
		}
		if (GUI.Button (clearProps, "Clear Effectors")) {
			currentlySelected.effectors.Clear ();
		}

		Vector2 pos = yo (addProps, 0, 20, 0, 0).position;

		for (int i = 0; i < currentlySelected.effectors.Count; i++) {
			Rect myRect = r (pos.x, pos.y + (75 * i) + 5 * i, inspectorRect.width - 25, 75);
			GUI.Box (myRect, "");

			Rect val1 = o (myRect, 5, 20, 100, 20);
			Rect checkType = xo (val1, 5, 0, 70, 20);;
			Rect val2 = xo (checkType, 5, 0, 30, 20);

			currentlySelected.effectors[i].s = EditorGUI.TextField (val1, currentlySelected.effectors[i].s);
			currentlySelected.effectors[i].type = (CEffector.EffectorType)EditorGUI.EnumPopup (checkType, currentlySelected.effectors[i].type);
			currentlySelected.effectors[i].f = EditorGUI.FloatField (val2, currentlySelected.effectors[i].f);

			Rect removeRect = yo (val1, 0, 10, 100, 20);

			if (GUI.Button (removeRect, "Remove")) {
				currentlySelected.effectors.RemoveAt (i);
				continue;
			}

		}

		finalNodeScroll.height = 325 + (75 * currentlySelected.effectors.Count) + 5 * currentlySelected.effectors.Count + 75;
		finalNodeScroll.position = inspectorRect.position;
		GUI.EndScrollView ();
	}

	private void SeeCheckers(Rect end) {
		Rect addProps = o (end, 0, 70, 125, 20);
		Rect clearProps = xo (addProps, 5, 0, 95, 20);

		if (GUI.Button (addProps, "Add Checker")) {
			if (properties.keys.Count <= 0)
				currentlySelected.checkers.Add (new CChecker ());
			else
				currentlySelected.checkers.Add (new CChecker (properties.keys [properties.keys.Count - 1]));
		}
		if (GUI.Button (clearProps, "Clear Checkers")) {
			currentlySelected.checkers.Clear ();
		}


		Vector2 pos = yo (addProps, 0, 20, 0, 0).position;

		//Draw properties
		for (int i = 0; i < currentlySelected.checkers.Count; i++) {
			Rect myRect = r (pos.x, pos.y + (75 * i) + 5 * i, inspectorRect.width - 25, 75);
			GUI.Box (myRect, "");

			Rect val1 = o (myRect, 5, 20, 100, 20);
			Rect checkType = xo (val1, 5, 0, 70, 20);;
			Rect val2 = xo (checkType, 5, 0, 30, 20);

			currentlySelected.checkers[i].propertyName = EditorGUI.TextField (val1, currentlySelected.checkers[i].propertyName);
			currentlySelected.checkers[i].checkType = (CChecker.propertyCheck)EditorGUI.EnumPopup (checkType, currentlySelected.checkers[i].checkType);
			currentlySelected.checkers[i].value = EditorGUI.FloatField (val2, currentlySelected.checkers[i].value);

			Rect removeRect = yo (val1, 0, 10, 100, 20);

			if (GUI.Button (removeRect, "Remove")) {
				currentlySelected.checkers.RemoveAt (i);
				continue;
			}

		}
	
		finalNodeScroll.height = 325 + (75 * currentlySelected.checkers.Count) + 5 * currentlySelected.checkers.Count + 75;
		finalNodeScroll.position = inspectorRect.position;
		GUI.EndScrollView ();

	}


	private void DisplayPropertiesGUI() {
		Rect addProps = o (inspectorRect, 10, 70, inspectorRect.width - 30, 20);
		Rect clearProps = yo (addProps, 0, 0, inspectorRect.width - 30, 20);

		if (GUI.Button (addProps, "Add Property")) {
			properties.Add (properties.keys.Count.ToString(), 0);
//			Debug.Log ("GETTING CLIKED?");
		}

		if (GUI.Button (clearProps, "Clear Property")) {
			properties.ClearValues ();
		}

		Vector2 pos = yo (clearProps, 0, 20, 0, 0).position;

		//Draw properties
		for (int i = 0; i < properties.keys.Count; i++) {
			Rect myRect = r (pos.x, pos.y+(75*i) + 5 * i, inspectorRect.width - 30, 75);
			GUI.Box (myRect, "");

			Rect propertyName = o (myRect, 5, 5, myRect.width - 10, 20);
			Rect floatValue = yo (propertyName , 0, 5, myRect.width - 10, 20);
			Rect boolValue = yo (floatValue , 0, 5, 20, 20);
			Rect destroy = xo (boolValue, 30, 0, 100, 20);

			//string s = properties.keys[i]; 
			properties.locked = EditorGUI.Toggle (boolValue, properties.locked);
			if (properties.locked == false) {
				properties.keys [i] = GUI.TextField (propertyName, properties.keys [i]);
				//float v = properties.values[i];
				properties.values [i] = EditorGUI.FloatField (floatValue, properties.values [i]);

				if (GUI.Button (destroy, "Remove")) {
					properties.RemoveValue (properties.keys [i]);
				}

			} else {
				GUI.Label (propertyName, "KEY : " + properties.keys [i]);
				GUI.Label (floatValue, "VAL : " + properties.values [i]);
			}

			//properties.SetValue (s, v);

		}


		finalScroll.height = 130 + (75 * properties.keys.Count) + 5 * properties.keys.Count + 75;
		finalScroll.position = inspectorRect.position;
		GUI.EndScrollView ();
	}

	#endregion

	#region Save and Loading

	private void Download (string path) {

		nodes.Clear ();
		connections.ClearValues ();
		properties.ClearValues ();

		string json = File.ReadAllText (Application.dataPath + "/" + path);
		NodeEditorDat dat = JsonUtility.FromJson<NodeEditorDat> (json);

		nodes = dat.nodes;
		connectionIDCount = dat.connectionIDCount;
		connections = dat.connections;
		properties = dat.propertyDict;


	}

	private void Upload (string path) {

		NodeEditorDat dat = new NodeEditorDat (path, nodes, connectionIDCount, connections, properties);
		File.WriteAllText (Application.dataPath + "/" + path, JsonUtility.ToJson(dat));


	}

	#endregion

	#region Connection System

	public ConnectionDict connections = new ConnectionDict();


	public DNode currentlyDraggingConnection;
	public bool currentlyDraggingCon = false;

	private void ConnectionHandling(int i) {
		if (currentlyDraggingCon) {
			Event e = Event.current;
			Vector2 nC = currentlyDraggingConnection.outp.center;
			float hC = currentlyDraggingConnection.pos.height;
			Handles.DrawBezier (nC, e.mousePosition, nC + Vector2.up * hC, e.mousePosition + Vector2.down * hC, currentlyDraggingConnection.tagColor, null, 5f);


		}


		for (int c = 0; c < nodes [i].displayConnections.Count; c++) {

			if (nodes.Contains (nodes [i].displayConnections[c])) {

				if (nodes [i].displayConnections[c] != null || nodes[i].displayConnections[c].folded) {
					Vector2 nC3 = nodes [i].outp.center;
					float hC3 = nodes [i].pos.height / 2;

					Vector2 nC4 = nodes [i].displayConnections [c].inp.center;
					float hC4 = nodes [i].displayConnections [c].pos.height / 2;

					Handles.DrawBezier (nC3, nC4, nC3 + Vector2.up * hC3, nC4 + Vector2.down * hC4, nodes [i].tagColor, null, 4f);
				} else if (nodes[i].displayConnections[c].folded != false) {
					nodes [i].displayConnections.RemoveAt (c);
					nodes [i].connectedIDS.RemoveAt (c);
				}

			} else {
				nodes [i].displayConnections.RemoveAt (c);
				nodes [i].connectedIDS.RemoveAt (c);
			}
		}
			
	}

	private void ConnectionDrag (int i) {
		Event e = Event.current;
		if (!currentlyDraggingCon && e.button == 0 && e.type == EventType.MouseDrag) {
			if (nodes [i].outp.Contains (e.mousePosition)) {
				currentlyDraggingConnection = nodes [i];
				currentlyDraggingCon = true;
			}
		}




		if (e.button == 0 && e.type == EventType.MouseUp) {
			for (int n = 0; n < nodes.Count; n++) {
				if (nodes [n].inp.Contains (e.mousePosition)) {
					if (nodes [n] == currentlyDraggingConnection) {
						Debug.LogError ("Don't connect with self!");
						continue;
					} else {
						if (!currentlyDraggingConnection.connectedIDS.Contains (nodes [n].nodeID)) {
							currentlyDraggingConnection.connectedIDS.Add (nodes [n].nodeID);
							currentlyDraggingConnection.ConnectionOccurred (connections);
							nodes [n].tagColor = currentlyDraggingConnection.tagColor;
						} 
					}
				}
			}

			currentlyDraggingCon = false;

		}


	}

	#endregion

    #region Base Screen

    private void OnBaseScreen() {
        DrawHeadingBar();
    }

    #region Private Heading Vars
    private string fileStringPath;
    #endregion
	private Rect heading;
    private void DrawHeadingBar() {

        #region Draw Base Heading

        DrawRectBox(r(0,0,width,30),"",out heading);

        //Draw Options
        Rect filePath = o(heading,5,5,100,20);
        fileStringPath = GUI.TextField(filePath,fileStringPath);

        //Download / Upload
        Rect download = xo(filePath,10,0,80,20);
        Rect upload = xo(download,10,0,80,20);

        if (GUI.Button(download,"Download")) {
			Download(fileStringPath);
        }

        if (GUI.Button(upload,"Upload")) {
			Upload(fileStringPath);
        }

        #endregion

        #region Draw Nodes

        //Draw Node Options
        Rect startNode = xo(upload,50,0,80,20);
        Rect gateNode = xo(startNode,10,0,80,20);
        Rect NPCNode = xo(gateNode,10,0,80,20);
        Rect PlayerNode = xo(NPCNode,10,0,80,20);

        Rect ClearNodes = xo(PlayerNode,30,0,80,20);
		Rect refresh = xo (ClearNodes, 30,0,80,20);

		Rect _drawInspector = r(width-120,5,100,20);


        if (GUI.Button(startNode,"Start")) {
			connectionIDCount ++;
			nodes.Add(new DNode("Start",new Vector2(width-400, 70) - offset,connectionIDCount));
			connections.Add (connectionIDCount, nodes[nodes.Count - 1]);
        }

        if (GUI.Button(gateNode,"Gate")) {
			connectionIDCount ++;
			nodes.Add(new DNode("Gate",new Vector2(width-400, 70) - offset,connectionIDCount));
			connections.Add (connectionIDCount, nodes[nodes.Count - 1]);
        }

        if (GUI.Button(NPCNode,"NPC")) {
			connectionIDCount ++;
			nodes.Add(new DNode("NPC",new Vector2(width-400, 70) - offset,connectionIDCount));
			connections.Add (connectionIDCount, nodes[nodes.Count - 1]);
        }

        if (GUI.Button(PlayerNode,"Player")) {
			connectionIDCount ++;
			nodes.Add(new DNode("Player",new Vector2(width-400, 70) - offset,connectionIDCount));
			connections.Add (connectionIDCount, nodes[nodes.Count - 1]);
        }

		if (GUI.Button(ClearNodes,"Clear")) {
			connectionIDCount = 0;
			connections.ClearValues();
			properties.ClearValues();
            nodes.Clear();
        }

		if (GUI.Button(_drawInspector,"[]")) {
			drawInspector = !drawInspector;
		}

		if (GUI.Button(refresh, "REFRESH")) {
			
			for (int c = 0; c < nodes.Count; c++) {
				connections.SetValue(nodes[c].nodeID, nodes[c]);
			}

			for (int i = 0; i < nodes.Count; i++) {
				nodes [i].ConnectionOccurred (connections);
			}
			//ww22
			offset = Vector2.zero;
		}

        #endregion

    }

    #endregion

    #region Node Handling

	bool didClickThisEventTime = false;

    private void NodeHandler() {
        MainNodeLoop();
    }


    private void MainNodeLoop() {
		didClickThisEventTime = false;
        for (int i = 0; i < nodes.Count; i++) {
			
            if (nodes[i] != null) {
				
				nodes[i].DrawNode(offset);

				if (!nodes [i].folded) {

					if (!nodes [i].foldedChildren)
						ConnectionHandling (i);
			
					CheckTag (i);

					if (!clickedInspector) {
					
						if (!dragginNode && !nodes[i].foldedChildren)
							ConnectionDrag (i);

						if (!currentlyDraggingCon)
							NodeDragging (i);
					
					}

				}

            }


        }
    }

	#region Tagging

	private void CheckTag (int i) {
		Vector2 tagPos = nodes [i].pos.position;
		Vector2 tS = nodes [i].pos.size;
		Vector2 p = nodes[i].colorTag.position;
		bool outOfScreen = false;
		bool yAffected = false;
		bool xAffected = false;
		if (drawInspector) {
			if (tagPos.x > width - 250) {
				p.x = width - 250 - 20;
				outOfScreen = true;
				xAffected = true;
				//Debug.Log ("GTHANK X");
			}
		} else {

			if (tagPos.x > width) {
				p.x = width - 20;
				outOfScreen = true;
				xAffected = true;
			}

		}

		if (tagPos.x+tS.x < 0) {
			p.x = 0;
			outOfScreen = true;
			xAffected = true;
			//Debug.Log (":LTHANK X");
		}

		if (tagPos.y > height-20) {
			p.y = height - 40;
			outOfScreen = true;
			yAffected = true;
		}

		if (tagPos.y+tS.y < heading.position.y + heading.height) {
			p.y = heading.position.y + heading.height;
			outOfScreen = true;
			yAffected = true;
			//Debug.Log ("LTHANK Y");
		}
			
		Vector3 tempPos = nodes [i].colorTag.position;

		if (xAffected && !yAffected) {
			tempPos.x = p.x;
			tempPos.y = nodes [i].pos.y;
		}

		if (yAffected && !xAffected) {
			tempPos.x = nodes[i].pos.x;
			tempPos.y = p.y;
		}

		if (yAffected && xAffected)
			tempPos = p;

		if (drawInspector)
			tempPos.x = Mathf.Clamp (tempPos.x, 0, width-250);
		else
			tempPos.x = Mathf.Clamp (tempPos.x, 0, width);
		tempPos.y = Mathf.Clamp (tempPos.y, heading.position.y + heading.height, height-20);

		nodes [i].colorTag.position = tempPos;

		nodes [i].colorTag.size = new Vector2 (20, 20);

		nodes [i].outOfScreen = outOfScreen;
	}

	#endregion

    #region Dragging

    private bool dragginNode = false;
    private DNode gettingDragged = new DNode();
    private Vector2 dragPointStart;
    private bool brokeOutOfZone = false;
//	private Vector2 fullOffset = Vector2.zero;
    private void NodeDragging(int i) {

		#region Dragging

		if (Event.current.button == 0 && Event.current.type == EventType.MouseDrag && nodes[i].pos.Contains(Event.current.mousePosition) && dragginNode == false) {
			dragginNode = true;
			gettingDragged = nodes[i];
			brokeOutOfZone = false;
			dragPointStart = Event.current.mousePosition;

			if (Event.current.alt) {
				gettingDragged.SetOffsets(gettingDragged);
			}

		}

		if (draggingThisFrame && dragginNode && gettingDragged.canDrag && brokeOutOfZone || draggingThisFrame && dragginNode && gettingDragged.canDrag && Vector2.Distance(dragPointStart,Event.current.mousePosition)>7) {
			gettingDragged.realPos.position = Vector2.Lerp(gettingDragged.pos.position,(draggingVector-gettingDragged.pos.size/2)-offset,5);
			gettingDragged.gettingDragged = true;
			brokeOutOfZone = true;

			if (Event.current.alt) {
				gettingDragged.ConnectionRealPosInc(gettingDragged,true);
			}

		}



		if (dropped) {

	
			dragginNode = false;
			brokeOutOfZone = true;
			gettingDragged.gettingDragged = false;
			didClickThisEventTime = false;
			//fullOffset = Vector2.zero;
		}

		#endregion

		#region Inspector 

		if (clickedThisFrame &&  !constantInspectorRect.Contains(Event.current.mousePosition) && Event.current.alt == true) {
			if (clickedRect (nodes [i].pos)) {
				propertiesTab = false;
				currentlySelected = nodes[i];
				drawInspector = true;
				selectedNode = true;
				didClickThisEventTime = true;
			} else {
				if (!didClickThisEventTime) {
					drawInspector = false;
					selectedNode = false;
				}
			}
		}

		#endregion

		#region Deleting and Folding

		//Deleting
		if (Event.current.button == 0 && Event.current.type == EventType.MouseDown && nodes[i].pos.Contains(Event.current.mousePosition) && dragginNode == false && Event.current.shift == true) {



			connections.RemoveValue(nodes[i].nodeID);
			nodes.RemoveAt (i);
		}

		if (Event.current.button == 2 && Event.current.type == EventType.MouseDown && Event.current.shift == true) {
			if (nodes [i].inp.Contains (Event.current.mousePosition)) {
				for (int x = 0; x < nodes.Count; x++) {
					if (nodes[x].connectedIDS.Contains(nodes[i].nodeID)) {
						nodes[x].displayConnections.Remove (nodes[i]);
						nodes[x].connectedIDS.Remove (nodes[i].nodeID);
					}
				}
			}
		}

		if (Event.current.button == 2 && Event.current.type == EventType.MouseDown && Event.current.shift == true) {
			if (nodes [i].outp.Contains (Event.current.mousePosition)) {

				if (nodes[i].foldedChildren)
					nodes[i].MainUnFoldChild();
				else if(!nodes[i].foldedChildren) 
					nodes[i].MainFoldChild(nodes[i].pos.position);
			}
		}


		#endregion


    }

    #endregion

    #endregion

    #region Screen handling
    NodeType selection;
    private bool enumUP = false;
    private Vector2 selPoint;
	private bool panning = false;

	private Vector2 offset = Vector2.zero;
	private Vector2 startDrag = Vector2.zero;
	private Vector2 moveOffset = Vector2.zero;
    private void ScreenEventHandler() {
        Event e = Event.current;

		if (e.button == 1 && e.type == EventType.MouseDown && !e.alt) {
            enumUP = true;
            selPoint = e.mousePosition;
        }
        else if (e.button == 0 && e.type == EventType.MouseDown) {
            if (!r(selPoint.x-50,selPoint.y,100,70).Contains(e.mousePosition))
            enumUP = false;

        }

		if (e.button == 2 && e.type == EventType.MouseDown && e.alt) {
			this.maximized = !this.maximized;
			for (int c = 0; c < nodes.Count; c++) {
				connections.SetValue(nodes[c].nodeID, nodes[c]);
			}

			for (int i = 0; i < nodes.Count; i++) {
				nodes [i].ConnectionOccurred (connections);
			}
		}

		if (e.alt && e.button == 1 && e.type == EventType.MouseDown) {
			drawInspector = !drawInspector;
			propertiesTab = true;
		}

        if (enumUP) {
            Rect enumPopup = r(selPoint.x-50,selPoint.y,100,70);
            GUI.changed = false;
            selection = (NodeType)EditorGUI.EnumPopup(enumPopup, selection);
            if (GUI.changed) {
                //Selected an option
                selPoint.x -= 50;
				connectionIDCount++;
				if (selection == NodeType.Gate) {
					nodes.Add (new DNode ("Gate", selPoint-offset, connectionIDCount));
					connections.Add (connectionIDCount, nodes[nodes.Count - 1]);
				}
				if (selection == NodeType.NPC) {
					nodes.Add (new DNode ("NPC", selPoint-offset, connectionIDCount));
					connections.Add (connectionIDCount, nodes[nodes.Count - 1]);
				}
				if (selection == NodeType.Player) {
					nodes.Add (new DNode ("Player", selPoint-offset, connectionIDCount));
					connections.Add (connectionIDCount, nodes[nodes.Count - 1]);
				}
				if (selection == NodeType.Start) {
					nodes.Add (new DNode ("Start", selPoint-offset, connectionIDCount));
					connections.Add (connectionIDCount, nodes[nodes.Count - 1]);
				}

                enumUP = false;


            }
        }


		///PANNINGIGNIGNIG

		if (e.button == 2 && e.type == EventType.MouseDown && !Event.current.alt && !Event.current.shift) {
			startDrag = e.mousePosition;
		}

		if (e.button == 2 && e.type == EventType.MouseDrag && !Event.current.shift) {
			moveOffset = (e.mousePosition - startDrag) * 0.1f;
			moveOffset = Vector3.ClampMagnitude (moveOffset, 3f);
			panning = true;
		}

		if (panning) {
			offset += moveOffset;
		}


		if (e.button == 2 && e.type == EventType.MouseUp) {
			moveOffset = Vector2.zero;
			startDrag = Vector2.zero;
		}


    }
    #endregion

    #region Helpers

    private bool dropped;
   // private Vector2 dropVect;

    Rect r (float x, float y, float xS, float yS) {
        return new Rect (new Vector2 (x, y), new Vector2 (xS, yS));
    }

    Rect r (float x, float y, float xS, float yS, out Rect rectOut) {
        rectOut = new Rect (new Vector2 (x, y), new Vector2 (xS, yS));
        return new Rect (new Vector2 (x, y), new Vector2 (xS, yS));
    }

    Rect o (Rect r, float xOff, float yOff, float xS, float yS) {
        return new Rect (new Vector2 (r.x+xOff, r.y+yOff), new Vector2 (xS, yS));
    }

    Rect xo (Rect r, float xOff, float yOff, float xS, float yS) {
        return new Rect (new Vector2 (r.x+xOff+r.size.x, r.y+yOff), new Vector2 (xS, yS));
    }

    Rect yo (Rect r, float xOff, float yOff, float xS, float yS) {
        return new Rect (new Vector2 (r.x+xOff, r.y+yOff+r.size.y), new Vector2 (xS, yS));
    }

    Rect o (Rect r, float xOff, float yOff, float xS, float yS, out Rect rectOut) {
        rectOut = new Rect (new Vector2 (r.x+xOff, r.y+yOff), new Vector2 (xS, yS));
        return new Rect (new Vector2 (r.x+xOff, r.y+yOff), new Vector2 (xS, yS));
    }

    private void DrawRectBox (Rect rect, string text, out Rect r) {
        GUI.Box(rect,text);
        r = rect;
    }

    private void DrawRectBox (Rect rect, string text) {
        GUI.Box(rect,text);
    }

    private float width {
        get {
            return Screen.width;
        }
    }

    private float height {
        get {
            return Screen.height;
        }
    }

    private void OnDetectClick() {
        Event e = Event.current;

        if (e.button == 0 && e.type == EventType.MouseDown) {
            clickedThisFrame = true;
            clickVector = e.mousePosition;
        }
        else
        {
            clickedThisFrame = false;
        }

        if (e.button == 0 && e.type == EventType.MouseDrag) {
            draggingThisFrame = true;
            draggingVector = e.mousePosition;
        }
        else
            draggingThisFrame = false;

        if (e.button == 0 && e.type == EventType.MouseUp) {
            dropped = true;
           // dropVect = e.mousePosition;
        }
        else
            dropped = false;

    }

    private bool clickedRect (Rect r) {
        if (clickedThisFrame) {
            if (r.Contains(clickVector))
                return true;
        }

        return false;
    }

    private bool draggedRect (Rect r) {
        if (draggingThisFrame) {
            if (r.Contains(draggingVector))
                return true;
        }

        return false;
    }

    #endregion

}
