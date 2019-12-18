using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Debugger : EditorWindow {

	DebuggerHelp h;
	Vector2 scrollPos;
	[MenuItem("ANN/Logger")]
	static void Init() {
		EditorWindow w = (Debugger)EditorWindow.GetWindow (typeof(Debugger));
		w.Show ();
	}

	void OnGUI() {
		if (h == null)
			h = GameObject.FindObjectOfType<DebuggerHelp> ();
		Repaint ();
		EditorUtility.SetDirty (h);

		DrawLog ();

	}
	int curInd = 0;
	private void DrawLog() {
		scrollPos = GUILayout.BeginScrollView (scrollPos, true, true);
		GUILayout.BeginHorizontal ();
		if ((curInd-1) >= 0 && (curInd-1) < h.prevLog.Count)
		if (GUILayout.Button ("<")) {
			curInd -= 1;
			h.log = h.prevLog [curInd];
		}
		if ((curInd+1) >= 0 && (curInd+1) < h.prevLog.Count)
		if (GUILayout.Button (">")) {
			curInd += 1;
			h.log = h.prevLog [curInd];
		}
		GUILayout.EndHorizontal ();
		GUILayout.Label (h.log);
		GUILayout.EndScrollView ();

	}

	Rect r (float x, float y, float xS, float yS) {
		return new Rect (new Vector2 (x, y), new Vector2 (xS, yS));
	}
		
}
