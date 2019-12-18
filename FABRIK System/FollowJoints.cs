using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowJoints : MonoBehaviour {

	[Header("Joints")]
	public Transform jointA;
	public Transform jointB;

	public Vector3 eulerOffset;


	private void Awake() {
		eulerOffset = transform.eulerAngles - (Quaternion.FromToRotation (jointA.forward, (jointB.position - jointA.position).normalized).eulerAngles);
	}

	private void LateUpdate() {
		
		transform.rotation = Quaternion.LookRotation (jointB.position-transform.position);

	}


}
