using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class RandomWindGenerator : MonoBehaviour, IComparable<RandomWindGenerator>
{
    public float currentFitness;

    [Header("Fitness Calculations")]
    public float straightWeight;
    public float timeWeight;

    [Header("ForceProperties")]
    public Vector3 localForcePoint;
    public float maxMagnitudeForce = 2;
    public Vector3 currentForce;
    public float lerpSpeed = 0f;
    public float windChangeTimer = 2;
    
    public float maxCartSpeed;

    [Header("Boundaries")]
    public Vector3 boundingBox;
    public Vector3 origin;

    public Vector3 windDirection;

    [Header("System State")]
    public Transform cart;

    public float velocity;
    public float posX;
    public float angle;
    public float angularVel;

    Vector3 initialCartPos, initialCartRotation, initialPolePosition, initialPoleRot;

    private Rigidbody rBody;
    private Rigidbody cartBody;

    public NNet network;

    //0 is false and 1 is true
    public int CompareTo(RandomWindGenerator w)
    {
        //if (obj == null) return 1;

        RandomWindGenerator otherTemperature = w;
        if (otherTemperature != null)
        {
            if (currentFitness > otherTemperature.currentFitness) { return 1; }
            else if (currentFitness == otherTemperature.currentFitness) return 0;
            else return -1;
        }
        else
            throw new ArgumentException("Object is not a Temperature");
    }

    private void Start()
    {
        initialCartPos = cart.position;
        initialCartRotation = cart.eulerAngles;
        initialPolePosition = transform.position;
        initialPoleRot = transform.eulerAngles;

        network.InitialiseNetwork(4, 1);

        rBody = transform.GetComponent<Rigidbody>();
        cartBody = cart.GetComponent<Rigidbody>();
        int lR = UnityEngine.Random.Range(0, 2);
        if (lR == 0)
            windDirection.x = Vector3.right.x * UnityEngine.Random.Range(1, maxMagnitudeForce);
        else if (lR == 1)
            windDirection.x = -Vector3.right.x * UnityEngine.Random.Range(1, maxMagnitudeForce);


        rBody.AddForceAtPosition(windDirection * 10, transform.InverseTransformPoint(localForcePoint), ForceMode.Force);

    }

    public void Reset(Genotype gene)
    {
        cart.position = initialCartPos;
        cart.eulerAngles = initialCartRotation;
        transform.position = initialPolePosition;
        transform.eulerAngles = initialPoleRot;
        timer = 0f;
        currentFitness = 0f;
        velocity = 0f;
        posX = 0f;
        angle = 0f;
        angularVel = 0f;
        rBody.velocity = Vector3.zero;
        cartBody.velocity = Vector3.zero;
        dead = false;
        network.net = gene;

        int lR = UnityEngine.Random.Range(0, 2);
        if (lR == 0)
            windDirection.x = Vector3.right.x * UnityEngine.Random.Range(0.1f, maxMagnitudeForce);
        else if (lR == 1)
            windDirection.x = -Vector3.right.x * UnityEngine.Random.Range(0.1f, maxMagnitudeForce);

        
        rBody.AddForceAtPosition(windDirection * maxMagnitudeForce, transform.InverseTransformPoint(localForcePoint), ForceMode.Force);
        //Restart the wind

        StartCoroutine("windChange");
    }

    private IEnumerator windChange()
    {
        yield return new WaitForSeconds(windChangeTimer);
        rBody.AddForceAtPosition(windDirection * maxMagnitudeForce, transform.InverseTransformPoint(localForcePoint), ForceMode.Force);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(origin, boundingBox);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.TransformPoint(localForcePoint), 0.2f);
    }

    bool dead = false;
    float timer = 0f;
    private void Update()
    {
        if (!dead)
        {
            if (currentFitness >= 5000)
            {
                if (dead != true)
                {
                    dead = true;
                    GameObject.FindObjectOfType<GenePool>().RecordDeath(network.net, currentFitness);
                    StopCoroutine("windChange");
                    return;
                }
            }
            if (Mathf.Abs(angularVel) <= 0.001f)
            {
                timer += Time.deltaTime;
            }
            else
            {
                timer = 0f;
            }

            if (timer >= 7)
            {
                currentFitness = 0f;
                timer = 0f;
                if (dead != true)
                {
                    dead = true;
                    GameObject.FindObjectOfType<GenePool>().RecordDeath(network.net, currentFitness);
                    StopCoroutine("windChange");
                    return;
                }
            }
            //currentForce = Vector3.Lerp(currentForce, windDirection, lerpSpeed * Time.deltaTime);

            angularVel = rBody.angularVelocity.magnitude;
            angle = Vector3.Angle(Vector3.right, transform.up);
            posX = cartBody.position.x;
            velocity = cartBody.velocity.magnitude;

            float filteredAngle = angle;
            if (filteredAngle > 90) filteredAngle = 90 - filteredAngle;
            
            float filteredX = posX;
            if (filteredX > origin.x)
                filteredX = posX - origin.x;
            else
                filteredX = origin.x - posX;

            List<float> inputs = new List<float>();
            inputs.Add(angularVel/2f);
            inputs.Add(filteredAngle/90);
            inputs.Add(posX/(origin.x+boundingBox.x/2));
            inputs.Add(velocity/maxCartSpeed);

            float output = 0f;
            //Assumes the network has been initialised
            output = network.RunNetwork(inputs)[0].value;

            Move(output);

            currentFitness += Time.deltaTime * timeWeight;
            float absWeight = 0f;
            if (angle > 90)
            {
                absWeight = angle - 90;
            }

            float score = (90 - absWeight) / 90;

            if (!(Mathf.Abs(angularVel)<=0.001f))
            currentFitness += score * straightWeight;
        }
    }

    private void Move (float hox)
    {
        if (hox != 0)
        {
            float strength = Mathf.Clamp(hox * maxCartSpeed * Time.deltaTime,0.03f*Time.deltaTime,maxCartSpeed);
            cartBody.position += Vector3.right * strength;
        }

        if (cartBody.position.x > origin.x + boundingBox.x / 2 || cartBody.position.x < origin.x - boundingBox.x / 2)
        {
            if (dead != true)
            {
              
                dead = true;
                GameObject.FindObjectOfType<GenePool>().RecordDeath(network.net, currentFitness);
                StopCoroutine("windChange");
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Ground")
        {
            if (dead != true)
            {
                dead = true;
                GameObject.FindObjectOfType<GenePool>().RecordDeath(network.net, currentFitness);
                StopCoroutine("windChange");
            }
        }
    }

}
