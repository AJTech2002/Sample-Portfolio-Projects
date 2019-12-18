using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Grapher : EditorWindow {

	Texture2D tex;
	GraphHelp a;

	Vector2 origin;
	Vector2 maxYPoint;
	Vector2 maxXPoint;

	bool grid = false;

	[MenuItem("ANN/Grapher")]
	static void Init() {
		EditorWindow g = (Grapher)EditorWindow.GetWindow (typeof(Grapher)) as Grapher;
		g.Show ();
	}

	void OnGUI() {
		if (a == null)
			a = GameObject.FindObjectOfType<GraphHelp> ();
		Repaint ();

		//Methods
	
		GraphDesign();
		Axis ();
		Options();
		Zoom ();
	}
	float originOffset=0;
	bool indicate = false;
	bool names = false;
	bool advanced=false;
	private void Options() {
		GUILayout.BeginHorizontal ();
		if (GUILayout.Button ("Grid"))
			grid = !grid;
		if (GUILayout.Button ("Indicate")) {
			indicate = !indicate;
		}
		if (GUILayout.Button ("Display Names"))
			names = !names;
		if (GUILayout.Button ("Clear Graph"))
			a.ClearAll ();
		if (GUILayout.Button ("Advanced Options" ))
			advanced = !advanced;
		GUILayout.EndHorizontal ();

		GUILayout.BeginHorizontal ();

		if (grid) {
			GUILayout.Label ("Grid Detail (X,Y) : ");
			a.valueDetailX = Mathf.RoundToInt(GUILayout.HorizontalSlider (a.valueDetailX, 2, 100));
			a.valueDetailY = Mathf.RoundToInt(GUILayout.HorizontalSlider (a.valueDetailY, 2, 100));
		}

		if (indicate) {
			GUILayout.Label ("Indicated Point (X , Y ) : ");
			a.indicatedPoint.x = GUILayout.HorizontalSlider (a.indicatedPoint.x, a.TMinX, a.TMaxX+originOffset);
			a.indicatedPoint.y = GUILayout.HorizontalSlider (a.indicatedPoint.y, a.TMinY, a.TMaxY);

		}

		if (advanced) {
			GUILayout.Label ("Zoom (X, Y) : ");
			a.TMaxX = GUILayout.HorizontalSlider (a.TMaxX, 0, a.backUpX+a.backUpX*0.5f);
			a.TMaxY = GUILayout.HorizontalSlider (a.TMaxY, a.backUpY, a.backUpX);
		}
			

		GUILayout.Label ("Move (X) : ");
		originOffset = GUILayout.HorizontalSlider (originOffset, -farthestPoint, 200);
		if (GUILayout.Button ("Reset")) {
			a.TMaxX = a.backUpX;
			a.TMaxY = a.backUpY;
			originOffset = 0;
		}
		GUILayout.EndHorizontal ();

	}

	private void Axis() {
		DrawAxis();
		DrawAxisLabels ();
		if (a.graphs != null || a.graphs.Count > 0)
		PlotPoints ();
	}

	private void Zoom() {
		
	}
	float farthestPoint = 0;
	private void PlotPoints() {

		if (indicate) {
			Vector2 point = a.indicatedPoint;
			float xRelPrev = Mathf.Abs(a.TMinX - point.x);
			float yRelPrev = Mathf.Abs(a.TMinY - point.y);
			float screenPosXPrev = spaceBetweenX.x / (a.TMaxX - a.TMinX)*xRelPrev;
			float screenPosYPrev = spaceBetweenY.y / (a.TMaxY - a.TMinY)*yRelPrev;
			float endXPrev = origin.x + screenPosXPrev + originOffset;
			if (endXPrev >= farthestPoint)
				farthestPoint = endXPrev;
			float endYPrev = origin.y + screenPosYPrev;

			Handles.BeginGUI ();
			Handles.color = new Color (a.indicatedColor.x, a.indicatedColor.y, a.indicatedColor.z);
			Handles.DrawAAPolyLine (3, new Vector2 (endXPrev, origin.y), new Vector2 (endXPrev, endYPrev));
			Handles.DrawAAPolyLine (3, new Vector2 (origin.x, endYPrev), new Vector2 (endXPrev, endYPrev));
			Handles.EndGUI ();

			GUI.Label (r (endXPrev + 10, endYPrev + 10, 150, 30), "(" + a.indicatedPoint.x.ToString () + "," + a.indicatedPoint.y.ToString() + ")");

		}

		for (int i = 0; i < a.graphs.Count; i++) {
			List<Vector2> points = a.graphs [i].points;
			if (points.Count > 0) {
				if (names)
					GUI.Label (r (end (points [points.Count - 1]).x - 200, end (points [points.Count - 1]).y + 10, 200, 30), "(" + a.graphs [i].name + ")" + " [" + i.ToString () + "]" + "\n" + "(X : " + points [points.Count - 1].x.ToString() + " Y : " + points [points.Count - 1].y.ToString() + ")");
				for (int p = 0; p < points.Count; p++) {
					if (p == 0)
						continue;
					else {
						float difX = (a.TMaxX - a.TMinX);
						float difY = (a.TMaxY - a.TMinY);
						float xRelPrev = Mathf.Abs (a.TMinX - points [p - 1].x);
						float yRelPrev = Mathf.Abs (a.TMinY - points [p - 1].y);
						float screenPosXPrev = spaceBetweenX.x / (a.TMaxX - a.TMinX) * xRelPrev;
						float screenPosYPrev = spaceBetweenY.y / (a.TMaxY - a.TMinY) * yRelPrev;
						float endXPrev = origin.x + screenPosXPrev + originOffset;
						if (endXPrev >= farthestPoint)
							farthestPoint = endXPrev;
						float endYPrev = origin.y + screenPosYPrev;
						//Debug.Log ("X : " + screenPosXPrev + " Y : " + screenPosYPrev);

						float xRel = Mathf.Abs (a.TMinX - points [p].x);
						float yRel = Mathf.Abs (a.TMinY - points [p].y);
						float screenPosX = spaceBetweenX.x / (a.TMaxX - a.TMinX) * xRel;
						float screenPosY = spaceBetweenY.y / (a.TMaxY - a.TMinY) * yRel;
						float endX = origin.x + screenPosX + originOffset;
						if (endX >= farthestPoint)
							farthestPoint = endXPrev;
						float endY = origin.y + screenPosY;
						Handles.BeginGUI ();
						Handles.color = a.graphs [i].col;
						Handles.DrawAAPolyLine (3, new Vector2 (endXPrev, endYPrev), new Vector2(endX, endY));
						Handles.EndGUI ();

					}
				}
			}
		}
			
	

	}

	public Vector2 end (Vector2 point) {
		float xRel = Mathf.Abs(a.TMinX - point.x);
		float yRel = Mathf.Abs(a.TMinY - point.y);
		float screenPosX = spaceBetweenX.x / (a.TMaxX - a.TMinX) * xRel;
		float screenPosY = spaceBetweenY.y / (a.TMaxY - a.TMinY) * yRel;
		float endX = origin.x + screenPosX + originOffset;
		if (endX >= farthestPoint)
			farthestPoint = endX;
		float endY = origin.y + screenPosY;
		return new Vector2 (endX, endY);
	}

	Vector2 spaceBetweenX;
	Vector2 spaceBetweenY;
	private void DrawAxisLabels() {
		//XLABLES
		spaceBetweenX = new Vector2 (maxXPoint.x, maxXPoint.y + 5) - new Vector2 (origin.x, origin.y + 5);
		maxXPoint = new Vector2 (maxXPoint.x, maxXPoint.y + 5);
		maxYPoint = new Vector2 (maxYPoint.x - 50, maxYPoint.y);
		int valDet = a.valueDetailX;
		float labelSpacingX = spaceBetweenX.x / valDet;
		for (int x = 0; x < valDet+1; x++) {
			Vector2 o = new Vector2 (origin.x, origin.y + 5);
			//float dif = Mathf.Abs(a.TMaxX - a.TMinX;
			GUI.Label (r (o.x+(labelSpacingX*x), o.y, 50, 20), (a.TMinX+(a.TMaxX/valDet)*x).ToString());
			if (grid) {
				Handles.BeginGUI ();
				Handles.color = new Color (a.gridColor.x, a.gridColor.y, a.gridColor.z);
				Handles.DrawLine (new Vector2 (o.x+(labelSpacingX*x), o.y), new Vector2 (o.x+(labelSpacingX*x), maxYPoint.y));
				Handles.EndGUI ();
			}
		}

		GUI.Label (r (0, position.height - 20, position.width, 30), a.printString);

		//GUI.Label (r (origin.x-50, origin.y, 50, 20), a.TMinY.ToString());
		//GUI.Label (r (maxYPoint.x-50, maxYPoint.y, 50, 20), a.TMaxY.ToString());
		spaceBetweenY = new Vector2 (maxYPoint.x-50, maxYPoint.y) - new Vector2 (origin.x-50, origin.y);
		int valDetY = a.valueDetailY;
		float labelSpacingY = spaceBetweenY.y / valDetY;
		for (int y = 0; y < valDetY+1; y++) {
			Vector2 o = new Vector2 (origin.x-50, origin.y);
			GUI.Label (r (o.x, o.y+(labelSpacingY*y), 50, 20), (a.TMinY+(a.TMaxY/valDetY)*y).ToString());
			if (grid) {
				Handles.BeginGUI ();
				Handles.color = new Color (a.gridColor.x, a.gridColor.y, a.gridColor.z);
				Handles.DrawLine (new Vector2 (o.x, o.y + (labelSpacingY * y)), new Vector2 (maxXPoint.x, o.y + (labelSpacingY * y)));
				Handles.EndGUI ();
			}
		}

	}

	private void DrawAxis() {
		Handles.BeginGUI ();
		Handles.color = new Color (a.axisColor.x, a.axisColor.y, a.axisColor.z);
		Handles.DrawAAPolyLine (4, new Vector2 (100, 50), new Vector2 (100, position.height - 50));
		Handles.DrawAAPolyLine (4, new Vector2 (100, position.height-50), new Vector2 (position.width-50, position.height - 50));
		origin = new Vector2 (100, position.height - 50);
		maxYPoint = new Vector2 (100, 50);
		maxXPoint = new Vector2 (position.width - 50, position.height - 50);
		Handles.DrawLine (new Vector2 (100, position.height-50), new Vector2 (position.width-50, position.height - 50));
		Handles.EndGUI ();
	}

	private void GraphDesign() {
		DrawBackgroundColor ();
	}

	private void DrawBackgroundColor() {
		tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
		tex.SetPixel(0, 0, new Color(a.backgroundColor.x, a.backgroundColor.y, a.backgroundColor.z));
		tex.Apply();
		GUI.DrawTexture(new Rect(0, 0, maxSize.x, maxSize.y), tex, ScaleMode.StretchToFill);
	}

	Rect r (float x, float y, float xS, float yS) {
		return new Rect (new Vector2 (x, y), new Vector2 (xS, yS));
	}

	Rect o (Rect r, float xOff, float yOff, float xS, float yS) {
		return new Rect (new Vector2 (r.x+xOff, r.y+yOff), new Vector2 (xS, yS));
	}

}

