using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FABRIK : MonoBehaviour {

    public ConstrainedIK constrainedIK;
	public Transform startPoint;
	public float minError;
    public float movespeed;
    public float distanceModifier;
	public List<Joint> joints = new List<Joint>();
	public int elbowJoint;

    public Transform rotateMesh;
    public Vector3 offsetMesh;

    [HideInInspector]
    public List<Segment> segments = new List<Segment> ();

	private bool initCompleted = false;
	private float combinedLength;

	private Vector3 rootPoint;

	private void Awake() {
		for (int i = 0; i < joints.Count; i++) {
			rootPoint = joints [0].pos;
			if (i != joints.Count - 1) {
				combinedLength += Vector3.Distance (joints [i].pos, joints [i + 1].pos);
				if (joints[i].mesh == null)
					segments.Add(new Segment(joints[i], joints[i+1], Vector3.Distance (joints [i].pos, joints [i + 1].pos)));
				else
					segments.Add(new Segment(joints[i], joints[i+1], Vector3.Distance (joints [i].pos, joints [i + 1].pos),joints[i].mesh));
			}
		}
        combinedLength += distanceModifier;
		initCompleted = true;
	}

	float curError = 0;


    private void Update() {
        SolveIK();
    }

    public void SolveIK() {
        if (initCompleted) {
            if (startPoint != null)
                rootPoint = startPoint.position;
            if (joints [joints.Count - 1].mesh != null && rotateMesh == null) {
                Transform finalMesh = joints [joints.Count - 1].mesh;
                finalMesh.eulerAngles = transform.eulerAngles + joints [joints.Count - 1].offsetRot;
            }
            else if (rotateMesh != null)
                rotateMesh.eulerAngles = transform.eulerAngles + offsetMesh;
            if (curError > minError) {

                if (!StraightSolve ()) {
                    usedStraightEffector = false;
                    ForwardSolve ();
                    BackwardSolve ();
                    UpdatePositions ();
                }
                else
                    usedStraightEffector = true;


            } 

            curError = Vector3.Distance (joints [joints.Count - 1].pos, transform.position);

            constrainedIK.ConstrainIK();

        }
    }


    private void UpdatePositions() {

        for (int i = 0; i < segments.Count; i++) {
            if (segments [i].mesh != null) {
                Vector3 cross = Vector3.Cross (joints [elbowJoint].effectors [0].effector.position - joints [elbowJoint].pos, joints [elbowJoint + 1].pos - joints [elbowJoint].pos);
                Debug.DrawRay (segments [i].jointA.pos, cross, Color.magenta, 0.2f);

                segments [i].mesh.LookAt (segments [i].jointB.pos, cross);
                segments [i].mesh.Rotate (segments [i].jointA.offsetRot);


            }
        }

        for (int i = 0; i < joints.Count; i++) {
            joints [i].joint.position = Vector3.Lerp(joints[i].joint.position,joints [i].tempPos,movespeed);
		}
	}

    private void RotFix() {
        for (int i = 0; i < segments.Count; i++) {
            if (segments [i].mesh != null) {
                Vector3 cross = Vector3.Cross (joints [elbowJoint].effectors [0].effector.position - joints [elbowJoint].pos, joints [elbowJoint + 1].pos - joints [elbowJoint].pos);
                Debug.DrawRay (segments [i].jointA.pos, cross, Color.magenta, 0.2f);

                segments [i].mesh.LookAt (segments [i].jointB.pos, cross);
                segments [i].mesh.Rotate (segments [i].jointA.offsetRot);


            }
        }
    }

	public void BackwardSolve() {
		for (int i = 0; i < segments.Count; i++) {
			if (i == 0) {
				segments [i].jointA.tempPos = rootPoint;
				segments [i].jointB.tempPos = segments [i].jointA.tempPos + (segments [i].jointB.tempPos - segments [i].jointA.tempPos).normalized * segments [i].length;
                continue;
			}

			segments [i].jointB.tempPos = segments [i].jointA.tempPos + (segments [i].jointB.tempPos - segments [i].jointA.tempPos).normalized * segments [i].length;

		}
	}

	public void ForwardSolve() {
		for (int i = segments.Count - 1; i > -1; i--) {
			if (i == segments.Count - 1) {
				segments [i].jointB.tempPos = transform.position;
				segments [i].jointA.tempPos = transform.position + (segments [i].JApos - transform.position).normalized * segments [i].length;
				JointEffector (segments [i], true);

				/*if (segments [i].mesh != null) {
					Vector3 cross = Vector3.Cross (joints [elbowJoint].effectors [0].effector.position - joints [elbowJoint].pos, joints [elbowJoint + 1].pos - joints [elbowJoint].pos);
					Debug.DrawRay (segments [i].jointA.pos, cross, Color.magenta, 0.2f);

					segments [i].mesh.LookAt (segments [i].jointB.tempPos, cross);
					segments [i].mesh.Rotate (segments [i].jointA.offsetRot);
				}*/
				continue;
			}
				
			segments [i].jointA.tempPos = segments[i].jointB.tempPos + (segments [i].JApos - segments[i].jointB.tempPos).normalized * segments [i].length;
			JointEffector (segments [i], false);
            /*if (segments [i].mesh != null) {
				Vector3 cross2 = Vector3.Cross (joints [elbowJoint].effectors [0].effector.position - joints [elbowJoint].pos, joints [elbowJoint + 1].pos - joints [elbowJoint].pos);
				Debug.DrawRay (segments [i].jointA.pos, cross2, Color.magenta, 0.2f);

				segments [i].mesh.LookAt (segments [i].jointB.tempPos, cross2);
				segments [i].mesh.Rotate (segments [i].jointA.offsetRot);
			}*/
		}


	}

	private void JointEffector (Segment s, bool AandB) {
		if (AandB) {
			Joint a = s.jointA;
			Vector3 finalDir = Vector3.zero;
			for (int e = 0; e < a.effectors.Count; e++) {
				finalDir += (a.effectors [e].effector.position - a.pos).normalized*a.effectors[e].weight;
			}
			//finalDir = finalDir.normalized * s.length;
			a.tempPos = a.tempPos + finalDir;
			Debug.DrawRay (a.pos, finalDir, Color.green, 0.2f);
		}

		Joint b = s.jointB;
		Vector3 finalDirB = Vector3.zero;
		for (int e = 0; e < b.effectors.Count; e++) {
			finalDirB += (b.effectors [e].effector.position - b.pos).normalized*b.effectors[e].weight;
		}
		//finalDirB = finalDirB.normalized * s.length;
		b.tempPos = b.tempPos + finalDirB;
		Debug.DrawRay (b.pos, finalDirB, Color.green, 0.2f);

	}

	private void JointStraightEffector (Segment s, bool AandB) {
		if (AandB) {
			Joint a = s.jointA;
			Vector3 finalDir = Vector3.zero;
			for (int e = 0; e < a.effectors.Count; e++) {
				finalDir += (a.effectors [e].effector.position - a.pos).normalized*a.effectors[e].weight;
			}
			//finalDir = finalDir.normalized * s.length;
			a.tempPos = a.pos + finalDir;
			Debug.DrawRay (a.pos, finalDir, Color.green, 0.2f);
		}

		Joint b = s.jointB;
		Vector3 finalDirB = Vector3.zero;
		for (int e = 0; e < b.effectors.Count; e++) {
			finalDirB += (b.effectors [e].effector.position - b.pos).normalized*b.effectors[e].weight;
		}
		//finalDirB = finalDirB.normalized * s.length;
		b.tempPos = b.pos + finalDirB;
		Debug.DrawRay (b.pos, finalDirB, Color.green, 0.2f);

	}
//    private bool firstPlay = true;
    [HideInInspector]
    public bool usedStraightEffector;
	private bool StraightSolve() {

		if (Vector3.Distance (transform.position, rootPoint) > combinedLength) {

			Vector3 projectedVector = (transform.position - rootPoint).normalized;
			float addedDist = 0;
			
			for (int i = 0; i < segments.Count; i++) {

				float pushFloat = segments [i].length + addedDist;
				Vector3 pointB = rootPoint + projectedVector * pushFloat;

                segments [i].jB.position = Vector3.Lerp(segments[i].jB.position,pointB,movespeed*0.5f);
                JointStraightEffector(segments[i],true);
				addedDist += segments [i].length;
			
            
			}

			return true;
		}

		return false;
	}

	private void OnDrawGizmos() {
		if (joints != null && joints.Count > 0) {
			Gizmos.color = Color.cyan;
			for (int i = 0; i < joints.Count; i++) {
				if (i != joints.Count - 1) 
					Gizmos.DrawLine (joints [i].pos, joints [i + 1].pos);

				if (joints [i].effectors.Count > 0) {
					for (int c = 0; c < joints[i].effectors.Count; c++) {
						Gizmos.color = Color.yellow;
						Gizmos.DrawLine (joints [i].pos, joints [i].effectors [c].effector.position);
					}
				}

			}
		}
	}


    private Vector3 ConstrainRotation(int joint, int segment, Vector3 calc)
	{
        Joint j = joints[joint];
        Segment s = segments[segment];
        if (joint == 0 || joint == joints.Count-1)
            return j.tempPos;


        Joint p = joints[joint-1];
        Joint n = joints[joint+1];

        Vector3 coneDir = (n.pos-j.pos);

        float scalar = Vector3.Dot(calc,coneDir);
        Vector3 proj  = scalar*coneDir.normalized;

        Vector3 adjust = calc-proj;

        Vector3 rightV = Vector3.Cross(coneDir,new Vector3(0,1,0));
        Vector3 upV = Vector3.Cross(rightV,coneDir);

        if (scalar < 0)
            proj = -proj;

        Debug.DrawRay(j.pos, upV, Color.red);
        Debug.DrawRay(j.pos, rightV,Color.magenta,0.2f);


        float xAspect = Vector3.Dot(adjust,rightV);
        float yAspect = Vector3.Dot(adjust, upV);


        float left = -(proj.magnitude*Mathf.Tan(70));
        float right = (proj.magnitude*Mathf.Tan(70));
        float up = (proj.magnitude*Mathf.Tan(70));
        float down = -(proj.magnitude*Mathf.Tan(70));


        int xBound = 0;
        int yBound = 0;

        if (xAspect >= 0 && xAspect >= right || xAspect >= left)
            xBound = 1;
        if (yAspect >= 0 && yAspect >= up || yAspect >= down)
            yBound = 1;

        Vector3 f = calc;

        float ellipse = (xAspect*xAspect)/(xBound*xBound) + (yAspect*yAspect)/(yBound*yBound);
        bool inbounds = false;

        if (ellipse <= 1 && scalar >= 0)
            inbounds = true;

        if (!inbounds) {

            float ang = Mathf.Atan2(yAspect,xAspect);

            float x = xBound*Mathf.Cos(ang);
            float y = yBound*Mathf.Sin(ang);

            f = (proj+rightV*x+upV*y).normalized*calc.magnitude;

        }


        return f;
	}

}

[System.Serializable]
public class Joint {


	[Header ("Effectors")]
	public List<Effector> effectors = new List<Effector>();

	[Header("Joint")]
	public Transform joint;
	public Vector3 pos {
		get {
			return joint.position;
		}
	}

	[Header("Mesh")]
	public Transform mesh;
	public Vector3 offsetRot;

	[HideInInspector]
	public Vector3 tempPos;
}

[System.Serializable]
public class Segment {
	public Joint jointA;
	public float length;
	public Joint jointB;
	public Transform mesh;
	public Segment (Joint _jointA, Joint _jointB, float len) {
		jointA = _jointA;
		jointB = _jointB;
		length = len;

	}
	public Segment (Joint _jointA, Joint _jointB, float len, Transform _mesh) {
		jointA = _jointA;
		jointB = _jointB;
		length = len;
		mesh = _mesh;

	}

	public Vector3 allocatedTempDir = Vector3.zero;

	public Transform jA {
		get {
			return jointA.joint;
		}
	}

	public Transform jB {
		get {
			return jointB.joint;
		}
	}

	public Vector3 JApos {
		get {
			return jA.position;
		}
	}

	public Vector3 JBpos {
		get {
			return jB.position;
		}
	}

	public Vector3 prevDir;

}

[System.Serializable]
public class Effector {
	public Transform effector;
	[Range(0f,1f)]
	public float weight;
}