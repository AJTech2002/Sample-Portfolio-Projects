using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class DNode  {

    #region Public Data Vars
    //Type of node
    public string nodeType;

	public int nodeID;
    //Chat data
    public string chat;
	public Color tagColor = Color.red;

	[System.NonSerialized]
    private GUIStyle typeStyle = new GUIStyle();
	[System.NonSerialized]
	private GUIStyle nameStyle = new GUIStyle();


	public List<CEffector> effectors = new List<CEffector>();
	public List<CChecker> checkers = new List<CChecker>();



    public enum NodeState {
        Small,
        Medium,
        Full
    }

    public NodeState s;

	public List<int> connectedIDS = new List<int> ();
	[System.NonSerialized]
	public List<DNode> displayConnections = new List<DNode>();
    #endregion

    #region Public Visual Vars

    public Rect pos = new Rect(new Vector2(100,100),new Vector2(100,100));
	[System.NonSerialized]
	public Rect colorTag = new Rect ();
    private bool focused = false;

//	[System.NonSerialized]
    public bool canDrag {
        get {
            if (editingText)
                return false;

                return true;
        }
    }

	[System.NonSerialized]
    public bool gettingDragged;

    #endregion

    #region Base Methods

	public Rect realPos = new Rect(new Vector2(100,100),new Vector2(100,100));
	public bool foldedChildren = false;
	public void DrawNode (Vector2 offset) {


		if (!folded) {
			pos.position = realPos.position;
			pos.size = realPos.size;
			pos.position += offset;

			string type = nodeType;
			drawExtension = false;
			Tag ();
			if (type == "Start") {
				DrawStart ();
				DrawConnectionBoxes (false, true);
			}
			if (type == "Gate") {
				DrawGate ();
				DrawConnectionBoxes (true, true);
			}
			if (type == "Player" || type == "NPC") {
				DrawDef ();
				DrawConnectionBoxes (true, true);
			}
		} else {
			pos.size = Vector2.zero;
			outp.size = Vector2.zero;
			inp.size = Vector2.zero;
		}
    }

	public void MainFoldChild(Vector2 orig) {
		foldedChildren = true;
		FoldChildren (orig);
	}

	public void MainUnFoldChild() {
		UnfoldChildren ();
		ConnectionRealPosInc (this, false);
	}

	public void FoldChildren(Vector2 orig) {

		for (int i = 0; i < displayConnections.Count; i++) {
			displayConnections [i].offsetWParent = displayConnections [i].pos.position - orig;
			displayConnections [i].FoldChildren (orig);
		}

		if (!foldedChildren) {
			folded = true;
		}
	}

	public void UnfoldChildren() {
		for (int i = 0; i < displayConnections.Count; i++) {
			displayConnections [i].UnfoldChildren ();
		}
			
		folded = false;
		foldedChildren = false;
	}

    private void DrawStart() {

		DrawDef ();

    }

    private void DrawGate() {
		DrawDef ();
    }

    private void DrawDef() {

        if (nodeType == "Player") {
            if (typeStyle.normal.textColor != Color.blue) {
                typeStyle.normal.textColor = Color.blue;
                typeStyle.fontStyle = FontStyle.Bold;

                nameStyle.fontSize = 10;
                nameStyle.wordWrap = true;
            }
        }

        if (nodeType == "NPC") {
            if (typeStyle.normal.textColor != Color.red) {
                typeStyle.normal.textColor = Color.red;
                typeStyle.fontStyle = FontStyle.Bold;
                nameStyle.fontSize = 10;
                nameStyle.wordWrap = true;
            }
        }

		if (nodeType == "Start") {
			if (typeStyle.fontStyle != FontStyle.Bold) {
				//typeStyle.normal.textColor = Color.black;
				typeStyle.fontStyle = FontStyle.BoldAndItalic;
				//typeStyle.alignment = TextAnchor.MiddleCenter;
				nameStyle.fontSize = 10;
				nameStyle.wordWrap = true;
			}
		}

		if (nodeType == "Gate") {
			if (typeStyle.fontStyle != FontStyle.Bold) {
				//typeStyle.normal.textColor = Color.magenta;
				typeStyle.fontStyle = FontStyle.Bold;
				//.alignment = TextAnchor.MiddleCenter;
				nameStyle.fontSize = 10;
				nameStyle.wordWrap = true;
			}
		}
	


        DrawBaseLayout();
        DefTextBox();
        DrawDefExtension();
    }

    private float expectedHeight = 100;

    private void DrawBaseLayout() {


        Rect miniFoldBox = o(pos,0,0,10,10);
        if (GUI.Button(miniFoldBox,"")){
            if (topFolded) {
                pos.height = expectedHeight;
               // drawExtension = true;
                topFolded = false;
            }
            else {
               // pos.height = 200;
                //drawExtension = false;
                topFolded = true;
            }
        }



      
        DrawRectBox(pos,"");
    }

    #endregion

    #region DefaultNode

	public bool folded = false;

	[System.NonSerialized]
	private bool selectingColor = false;
	[System.NonSerialized]
	public bool outOfScreen = false;
	private void Tag() {
		if (!outOfScreen) {
			colorTag = o (pos, 0, -10, pos.width, 10);
			GUI.color = tagColor;
			GUI.Box (colorTag, "");
			GUI.color = Color.white;

			Event e = Event.current;
			if (e.button == 0 && e.type == EventType.MouseDown) {

				if (colorTag.Contains (e.mousePosition)) {
					selectingColor = true;
				} else {
					selectingColor = false;
				}

			}

			if (selectingColor) {
				tagColor = EditorGUI.ColorField (colorTag, tagColor);
			}
		} else {
			GUI.color = tagColor;
			GUI.Box (colorTag, "");
			GUI.color = Color.white;
		}
	
	}

    private bool editingText;
    public bool topFolded = false;
    private void DefTextBox() {
        Rect titleName = o(pos,5,5,pos.width-10,20);
       

        GUI.Label(titleName,nodeType,typeStyle);
       
		Rect chatBox = o(titleName,0,15,pos.width-10, pos.height-25);

	


        if (!topFolded) {
        //Rect chatBox = o(titleName,0,15,pos.width-10, pos.height-25);

        if (Event.current.button == 0 && Event.current.type == EventType.MouseDown)
        {
            if (!chatBox.Contains(Event.current.mousePosition)) {
            editingText = false;
            BlurOutFocus();
            return;
            }
            else {
                editingText = true;
            }
        }


        GUI.SetNextControlName("ChatBox");
			if (!gettingDragged)
				chat = GUI.TextArea (chatBox, chat);
			else {
				GUI.Label (chatBox, chat, nameStyle);
			}

     

        if (!focused) {
            GUI.FocusControl("ChatBox");
            focused = true;
        }

        }

        else {
            editingText = false;
            Rect smallDesc = yo(titleName,0,-5,pos.width-10,43);
			float h = nameStyle.CalcHeight(new GUIContent(chat),chatBox.width);
            h += 35;
			if (h <= 300) {
				pos.height = h;
				GUI.Label(smallDesc,chat,nameStyle);
			} else {
				pos.height = 305;
				smallDesc.height = 280;
				GUI.TextArea(smallDesc,chat + "......");

			}

        }

    }

    private void BlurOutFocus() {
        GUI.SetNextControlName("FOCUSOUT");
        GUI.Label(new Rect(-100, -100, 1, 1), "");
        GUI.FocusControl("FOCUSOUT");
    }

    private Rect extensionRect;
    private bool drawExtension=true;
    private void DrawDefExtension() {
        if (drawExtension) {
            extensionRect = yo(pos,0,0,pos.width,pos.height);
            GUI.Box(extensionRect,"Extension");
        }
    }

    #endregion

	#region DrawConnections 
	[System.NonSerialized]
	public Rect inp;
	[System.NonSerialized]
	public Rect outp;

	public Vector2 offsetWParent;

	private void DrawConnectionBoxes (bool input, bool output) {
		if (input) {

			inp = o (pos, -15, pos.height / 2 - 15, 15, 15);
			GUI.Box (inp,"");

		}

		if (output) {
			if (foldedChildren)
				GUI.color = Color.blue;
			outp = xo (pos, 0, pos.height / 2 - 15, 15, 15);
			GUI.Box (outp,"");

			GUI.color = Color.white;

		}

	}

	public void ConnectionOccurred(ConnectionDict dict) {
		displayConnections.Clear ();
		for (int i = 0; i < connectedIDS.Count; i++) {
			displayConnections.Add (dict.GetValue (connectedIDS [i]));
		}
	}

	public void SetOffsets(DNode parentNode) {
		for (int i = 0; i < displayConnections.Count; i++) {
			displayConnections [i].offsetWParent = displayConnections [i].pos.position - parentNode.pos.position;
			displayConnections [i].SetOffsets (parentNode);
		}
	}

	public void ConnectionRealPosInc (DNode parentNode, bool setOffsets) {
		for (int i = 0; i < displayConnections.Count; i++) {
			displayConnections [i].realPos.position = parentNode.pos.position + displayConnections [i].offsetWParent;
			displayConnections [i].ConnectionRealPosInc (parentNode,setOffsets);
		}
	}
		

	#endregion

    #region Constructors

	public DNode (string _type, int id) {
        nodeType = _type;
		nodeID = id;

		if (nodeType == "Start") {
			topFolded = true;
		}

		if (nodeType == "Gate") {
			topFolded = true;
		}
		colorTag = new Rect (Vector2.zero, new Vector2 (pos.width, 10));

    }

	public DNode (string _type, Vector2 _pos, int id) {
        nodeType = _type;
		realPos.position = _pos;
		nodeID = id;

		if (nodeType == "Start") {
			topFolded = true;
		}

		if (nodeType == "Gate") {
			topFolded = true;
		}
		colorTag = new Rect (Vector2.zero, new Vector2 (pos.width, 10));

    }

	public DNode (string _type, string _chatDat, Rect _pos, int id) {
        nodeType = _type;
        chat = _chatDat;
        pos = _pos;
		nodeID = id;

		if (nodeType == "Start") {
			topFolded = true;
		}

		if (nodeType == "Gate") {
			topFolded = true;
		}
		colorTag = new Rect (Vector2.zero, new Vector2 (pos.width, 10));
    }

    public DNode () {
        
    }

    #endregion

    #region Helpers

    #region Rects
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

    #endregion

    #region Events

    public bool leftClicked(out Vector3 pos) {
        if (Event.current.button == 0 && Event.current.type == EventType.MouseDown)
        {
            pos = Event.current.mousePosition;
            return true;
        }

        pos = Vector3.zero;
        return false;
    }

    public bool eventOccurred(out Vector3 pos, EventType type, int button) {
        if (Event.current.button == button && Event.current.type == type)
        {
            pos = Event.current.mousePosition;
            return true;
        }

        pos = Vector3.zero;
        return false;
    }

    public bool eventOccurred(EventType type, int button) {
        if (Event.current.button == button && Event.current.type == type)
        {
            return true;
        }

        return false;
    }

    #endregion

    #endregion

}
