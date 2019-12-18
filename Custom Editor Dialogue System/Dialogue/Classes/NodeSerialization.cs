using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SDict {
	
	public List<string> keys = new List<string>();
	public List<float> values = new List<float>();
	public bool locked = false;

	public void Add (string key, float value) {
		if (!keys.Contains (key)) {
			keys.Add (key);
			values.Add (value);
		} else {
			SetValue (key, value);
		}
	}

	public float GetValue (string key) {
		return val (key);
	}

	public void ClearValues () {
		keys.Clear ();
		values.Clear ();
	}

	public void RemoveValue (string key) {
		int i = valueIndex (key);
		values.RemoveAt (i);
		keys.RemoveAt (i);
	}

	public void SetValue (string key, float value) {
		if (keys.Contains (key)) {
			values [valueIndex (key)] = value;
		}
	}

	private int valueIndex (string key) {
		for (int i = 0; i < keys.Count; i++) {
			if (keys [i] == key)
				return i;
		}

		Debug.LogError ("Key could not be found : " + key);
		return 0;

	}

	private float val (string key) {
		float v = 0;
		bool foundKey = false;
		for (int i = 0; i < keys.Count; i++) {
			if (keys[i] == key) {
				v = values [i];
				foundKey = true;
			}
		}

		if (!foundKey)
			Debug.LogError ("Key was not found : " + key);

		return v;
	}

}


[System.Serializable]
public class ConnectionDict {

	public List<int> keys = new List<int>();
	public List<DNode> values = new List<DNode>();
	public bool locked = false;

	public void Add (int key, DNode value) {
		if (!keys.Contains (key)) {
			keys.Add (key);
			values.Add (value);
		} else {
			SetValue (key, value);
		}
	}

	public DNode GetValue (int key) {
		return val (key);
	}

	public void ClearValues () {
		keys.Clear ();
		values.Clear ();
	}

	public void RemoveValue (int key) {
		int i = valueIndex (key);
		values.RemoveAt (i);
		keys.RemoveAt (i);
	}

	public void SetValue (int key, DNode value) {
		if (keys.Contains (key)) {
			values [valueIndex (key)] = value;
		}
	}

	private int valueIndex (int key) {
		for (int i = 0; i < keys.Count; i++) {
			if (keys [i] == key)
				return i;
		}

		Debug.LogError ("Key could not be found : " + key);
		return 0;

	}

	private DNode val (int key) {
		DNode v = new DNode();
		bool foundKey = false;
		for (int i = 0; i < keys.Count; i++) {
			if (keys[i] == key) {
				v = values [i];
				foundKey = true;
			}
		}

		if (!foundKey)
			Debug.LogError ("Key was not found : " + key);

		return v;
	}

}

	
[System.Serializable]
public class CChecker {
	public string propertyName="";
	public float value=0;
	public enum propertyCheck {LowerThan, GreaterThan, EqualTo, GreaterOrEqual, LowerOrEqual, NA};
	public propertyCheck checkType=propertyCheck.EqualTo;
	[System.NonSerialized]
	private SDict dict = new SDict ();

	public bool available (string s) {
		if (dict.keys.Contains (s)) {
			return true;
		} else
			return false;
	}

	public bool passCheck(SDict newD) {
		dict = newD;
		if (available(propertyName)) {
			if (checkType == propertyCheck.GreaterThan) {
				if (dict.GetValue (propertyName) > value)
					return true;
			} else if (checkType == propertyCheck.EqualTo) {
				if (dict.GetValue (propertyName) == value)
					return true;

			} else if (checkType == propertyCheck.GreaterOrEqual) {
				if (dict.GetValue (propertyName) >= value)
					return true;

			} else if (checkType == propertyCheck.LowerOrEqual) {
				if (dict.GetValue (propertyName) <= value)
					return true;

			} else if (checkType == propertyCheck.LowerThan) {
				if (dict.GetValue (propertyName) < value)
					return true;

			} else if (checkType == propertyCheck.NA) {
				return true;
			}
			return false;
		}
		return false;
	}

	public CChecker (string _propName) {
		propertyName = _propName;
	}

	public CChecker() {

	}

}

[System.Serializable]
public class CEffector {

	public string s;
	public float f;

	public enum EffectorType {Add,Minus,Multiply,Divide,Equalize,NA};
	public EffectorType type = EffectorType.Add;

	public void ActOnEffect (SDict dict) {
		if (dict.keys.Contains (s)) {
			if (type == EffectorType.Add)
				dict.SetValue (s, dict.GetValue (s) + f);
			if (type == EffectorType.Divide)
				dict.SetValue (s, dict.GetValue (s) / f);
			if (type == EffectorType.Minus)
				dict.SetValue (s, dict.GetValue (s) - f);
			if (type == EffectorType.Multiply)
				dict.SetValue (s,dict.GetValue (s) * f);
			if (type == EffectorType.Equalize)
				dict.SetValue (s, f);
		} 
	}

	public void SetValues (string key, float value, EffectorType _type) {
		s = key;
		f = value;
		type = _type;
	}

	public CEffector (string _s) {
		s = _s;
	}

	public CEffector () {}



}