using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneVisualise : MonoBehaviour {

	public List<Joint> joints = new List<Joint>();

	private void OnDrawGizmos() {
		if (joints != null && joints.Count > 0) {
			Gizmos.color = Color.cyan;
			for (int i = 0; i < joints.Count; i++) {
				if (i != joints.Count - 1) 
					Gizmos.DrawLine (joints [i].pos, joints [i + 1].pos);
			}
		}
	}

}
