using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebuggerHelp : MonoBehaviour {

	[HideInInspector]
	public string log;
	[HideInInspector]
	public int lines;

	void Awake() {
		Clear ();
	}

	public void Log(string l) {
		log += "---------------------------------------------------------------------------------- \n";
		log += l +"\n";
		log += "---------------------------------------------------------------------------------- \n";
		lines += 50;
	}

	[HideInInspector]
	public List<string> prevLog = new List<string> ();

	public void Clear() {
		prevLog.Add (log);
		log = "";
	}


}
