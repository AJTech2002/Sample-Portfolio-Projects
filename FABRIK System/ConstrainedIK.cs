using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstrainedIK : MonoBehaviour {

    [Header("Joints")]
    public List<CJoint> joints = new List<CJoint>();
    [Header("Segments")]
    public List<CSegment> segments = new List<CSegment>();
   
    public FABRIK main;

    public Transform startPoint;

    private Vector3 rootPoint;
    private float finalLength;
    private bool playing;

    private void Awake()
    {
        SetupSegments();
    }

    private void SetupSegments() {
        rootPoint = startPoint.position;
        for (int i = 0; i < joints.Count; i++) {

            joints[i].origPos = startPoint.InverseTransformPoint(joints[i].pos);

            if (i != joints.Count-1)
            {
                CSegment segment = new CSegment(joints[i],joints[i+1],Vector3.Distance(joints[i].pos,joints[i+1].pos));
                segments.Add(segment);
                finalLength += Vector3.Distance(joints[i].pos,joints[i+1].pos);
            }
            continue;
        }
        playing = true;
    }

    public void ConstrainIK() {
        rootPoint = startPoint.position;
        if (playing) {
            StretchModify();
        }
    }

    private void StretchModify() {

       // if (finalLength < Vector3.Distance(rootPoint, transform.position))
       // {

            Vector3 projectedVector = (transform.position - rootPoint).normalized;
            //float addedDist = 0;

            for (int i = 0; i < segments.Count; i++) {

               // float pushFloat = segments [i].length + addedDist;
               //// Vector3 pointB = rootPoint + projectedVector * pushFloat;

                //segments [i].a.joint.position = pointB;
               // addedDist += segments [i].length;

                if (segments[i].a.constraintRadius > 0) {
                if (!PointIsInSphere(segments[i].a.pos, startPoint.TransformPoint(segments[i].a.origPos), segments[i].a.constraintRadius)) {
                    segments[i].aTrans.position = closestPointOnBounds(segments[i].a.pos, startPoint.TransformPoint(segments[i].a.origPos), segments[i].a.constraintRadius);
                    }
                }

                if (segments[i].b.constraintRadius > 0) {
                if (!PointIsInSphere(segments[i].b.pos, startPoint.TransformPoint(segments[i].b.origPos), segments[i].b.constraintRadius)){
                    segments[i].bTrans.position = closestPointOnBounds(segments[i].b.pos, startPoint.TransformPoint(segments[i].b.origPos), segments[i].b.constraintRadius);
                       
                    }
                }


                AlignBones();

            }

          
       // }
//        return false;
    }


    private void AlignBones() {
        for (int i = segments.Count - 1; i > -1; i--) {
            Vector3 b = segments[i].b.pos;
            Vector3 a = segments[i].a.pos;

            Vector3 newAPos = b+((a-b).normalized*segments[i].length);

            segments[i].a.joint.position = newAPos;

        }
        BackwardSolve();
    }

    public void BackwardSolve() {
        for (int i = 0; i < segments.Count; i++) {
            if (i == 0) {
                segments [i].a.joint.position = rootPoint;
                segments [i].b.joint.position = segments [i].a.joint.position + (segments [i].b.joint.position - segments [i].a.joint.position).normalized * segments [i].length;
                continue;
            }

            segments [i].b.joint.position = segments [i].a.joint.position + (segments [i].b.joint.position - segments [i].a.joint.position).normalized * segments [i].length;

        }

        if (main.usedStraightEffector)
        {
            for (int i = 0; i < main.segments.Count; i++) {
                if (main.segments [i].mesh != null) {
                    Vector3 cross = Vector3.Cross (main.joints [main.elbowJoint].effectors [0].effector.position - main.joints [main.elbowJoint].pos, main.joints [main.elbowJoint + 1].pos - main.joints [main.elbowJoint].pos);
                    Debug.DrawRay (main.segments [i].jointA.pos, cross, Color.magenta, 0.2f);

                    main.segments [i].mesh.LookAt (main.segments [i].jointB.pos, cross);
                    main.segments [i].mesh.Rotate (main.segments [i].jointA.offsetRot);


                }
            }
        }
    }


    private void OnDrawGizmos()
    {
        if (joints != null && joints.Count > 0) {
            for (int i = 0; i < joints.Count; i++) {
                if (joints[i].joint != null) {
                    Gizmos.color = Color.yellow;
                    if (!playing)
                        Gizmos.DrawWireSphere(joints[i].pos, joints[i].constraintRadius);
                    else
                        Gizmos.DrawWireSphere(startPoint.TransformPoint(joints[i].origPos),joints[i].constraintRadius);

                    Gizmos.color = Color.green;
                    if (i != 0) {
                        Gizmos.DrawLine(joints[i].pos, joints[i-1].pos);
                    }

                    if (joints[i].effector != null) {
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawLine(joints[i].pos, joints[i].effector.position);
                    }

                }
            }
        }
    }

    private bool PointIsInSphere (Vector3 point, Vector3 originalPoint, float radius) {
        Vector3 vecDist = originalPoint-point;
        float fDistSq = Vector3.Dot(vecDist,vecDist);

        if (fDistSq < (radius*radius))
        {
            return true;
        }

        return false;

    }

    private Vector3 closestPointOnBounds(Vector3 point, Vector3 originalCenter, float radius) {
        float r = radius;
        Vector3 c = originalCenter;
        Vector3 dif = point-originalCenter;
        Vector3 returnPoint = c+(r/dif.magnitude)*dif*1f;
        return returnPoint;
    }

}

[System.Serializable]
public class CJoint {

    public Transform joint;
    public Transform mesh;
    public Transform effector;

    public float constraintRadius;

    [HideInInspector]
    public Vector3 origPos;


    public Vector3 pos {
        get {
            return joint.transform.position;
        }
    }


}

[System.Serializable]
public class CSegment {
    public CJoint a;
    public CJoint b;

    public float length;

    public CSegment (CJoint _a, CJoint _b, float len) {
        a = _a;
        b = _b;
        length = len;
    }

    public Transform aTrans {
        get {
            return a.joint;
        }

    }

    public Transform bTrans {
        get {
            return b.joint;
        }

    }

    public Transform mesh {
        get {
            return a.mesh;
        }

    }

}