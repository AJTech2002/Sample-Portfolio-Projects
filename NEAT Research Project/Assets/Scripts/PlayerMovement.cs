using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Transform detector;
    [Header("Network Outputs")]
    public float jumpMultiplier = 2f;

    [Header("Movement Code")]
    public bool grounded;
    public float groundDistance = 2f;
    public Rigidbody rBody;

    public LayerMask groundMask;

    Vector3 initialPos;
    Vector3 initialRot;

    private void Jump(float networkVal)
    {
        //Network needs to have enough energy to jump over the obstacle
        if (energyStart < 5f )
            return;

        Ray ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, groundMask ))
        {

            if (hit.distance <= groundDistance)
            {
                grounded = true;
                rBody.AddForce(Vector3.up * jumpMultiplier * networkVal, ForceMode.Impulse);
                energyStart -= 5f;
            }


        }
    }

    [Header("Network Modifiers")]
    public NNet network;
    public float fitness;
    public float energyStart = 10f;

    [Header("Network Inputs")]
    public float distanceToObstacle;
    public float obstacleSpeed;
    public float obstacleHeight;

    [Header("Normalization Variables")]
    public float maxDistance;

    public bool playing = true;
    private void Update()
    {
        if (playing)
        {
            if (fitness >= 500)
            {
                ResetWithGenePool();
                //You reached a good point, end it's life

            }
            energyStart += Time.deltaTime * 2f;
            fitness += Time.deltaTime;
            Ray ray = new Ray(detector.transform.position, Vector3.right);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Debug.DrawLine(ray.origin, hit.point, Color.green, 0.2f);
                distanceToObstacle = hit.distance;
                if (distanceToObstacle > maxDistance)
                    maxDistance = distanceToObstacle;

                distanceToObstacle = distanceToObstacle / maxDistance;

                ObstacleMove m = hit.transform.root.GetComponent<ObstacleMove>();
                obstacleSpeed = m.speed / 3;
                obstacleHeight = m.height / 1.6f;
            }

            List<float> inps = new List<float>();
            inps.Add(distanceToObstacle); inps.Add(obstacleSpeed); inps.Add(obstacleHeight);

        }

    }

    private void OnCollisionEnter(Collision collision)
    {
     
        if (collision.transform.CompareTag("Obstacle"))
        {
            ResetWithGenePool();
            
        }
    }

    private void ResetWithGenePool()
    {
        transform.position = initialPos;
        transform.eulerAngles = initialRot;

        playing = false;
        float finalFitness = fitness + energyStart;
        ResetDead();
    }

    public void ResetDead()
    {
        fitness = 0f;

    }

    private void Awake()
    {
        initialPos = transform.position;
        initialRot = transform.eulerAngles;
    }

}
