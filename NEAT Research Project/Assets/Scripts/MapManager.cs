using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public Vector3 areaOfMap;
    public Vector3 offset;

    public Transform obstacleElement;


    //Transform 
    private void Update() {
        
    }

    private void OnDrawGizmos() {
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position+offset,areaOfMap/2);

    }
}
