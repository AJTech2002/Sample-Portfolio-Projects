using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    //Store Beginning Positions
    private Vector3 startP;
    private Vector3 startE;

    private NNet network;

    //Accleration & Turning Values
    [Range(-1,1)]
    public float a,t;

    [Header("Network Options")]
    public int LAYERS = 1;
    public int NEURONS = 10;

    [Header("Fitness")]
    public float overallFitness;
    public float distanceMultiplier = 1.5f;
    public float avgSpeedMultiplier = 0.2f;
    public float sensorMultipler = 0.2f;

    private float timeSinceStart;
    private float totalDistanceTravelled;
    private float avgSpeed;
    private Vector3 lastPosition;

    private float aSensor,bSensor,cSensor;

    //------------------------------------

    private void Awake() {
        network = GetComponent<NNet>();
        startP = transform.position; startE = transform.eulerAngles;
    }

    //To reset the car fully
    public void Reset() {

        timeSinceStart = 0f;
        totalDistanceTravelled = 0f;
        avgSpeed = 0f;
        lastPosition = startP;
        overallFitness = 0f;
        transform.position = startP;
        transform.eulerAngles = startE;
    }

    public void ResetWithNetwork(NNet network) {
        this.network = network;
        Reset();
    }

    //Reset the network
    public void Death() {
        GameObject.FindObjectOfType<GeneticManager>().Death(overallFitness,network);
    }

    //If crashes into the walls 
    private void OnCollisionEnter(Collision collision) {
        Death();
    }

    //----------------------------------------------

    private void FixedUpdate() {

        //First we get the inputs
        InputSensors();

        //Used to calculate distance
        lastPosition = transform.position;

        //Now network controlls a & t
        (a,t) = network.RunNetwork(aSensor,bSensor,cSensor);

        MoveCar(a,t);

        //Fitness Calculation
        timeSinceStart += Time.deltaTime;

        CalculateFitness();
        a = 0;
        t = 0;

    }

    private void CalculateFitness() {

        totalDistanceTravelled += Vector3.Distance(transform.position,lastPosition);
        avgSpeed = totalDistanceTravelled/timeSinceStart;

        overallFitness = (totalDistanceTravelled*distanceMultiplier)+(avgSpeed*avgSpeedMultiplier)+(((aSensor+bSensor+cSensor)/3)*sensorMultipler);

        //Check for really bad network
        if (timeSinceStart>20 && overallFitness < 40)
            Death();

        //Awesome
        if (overallFitness >= 1000) {
            print("Great!");
            Death();
        }
            
    }

    private Vector3 inp;

    private void MoveCar(float v, float h) {
        //Allows for smooth acceleration changes
        inp = Vector3.Lerp(Vector3.zero,new Vector3(0,0,v*11.4f),0.02f);
        inp = transform.TransformDirection(inp);
        inp = Vector3.ClampMagnitude(inp, 100);

        transform.position += inp;

        //If 0.02 is higher, it allows the car to do full revolutions which means it allows it to rotate in place without moving
        transform.eulerAngles += new Vector3(0,Mathf.Lerp(0,h*90,0.02f),0);

    }

    private void InputSensors() {
        Vector3 a = (transform.forward);
        Vector3 b = (transform.forward+transform.right);
        Vector3 c = (transform.forward-transform.right);

        Ray r = new Ray(transform.position,a);
        RaycastHit hit;

        if (Physics.Raycast(r, out hit)) {

            //Dividing by 20 allows us to stay within a range of 0-1...
            aSensor = hit.distance/20;
            Debug.DrawLine(r.origin,hit.point,Color.red);

        }

        r.direction = b;
        if (Physics.Raycast(r, out hit)) {
            bSensor = hit.distance/20;
            Debug.DrawLine(r.origin,hit.point,Color.red);
        }

        r.direction = c;
        if (Physics.Raycast(r, out hit)) {
            cSensor = hit.distance/20;
            Debug.DrawLine(r.origin,hit.point,Color.red);
        }

    }
    
}
