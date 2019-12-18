using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleMove : MonoBehaviour
{
    public float speed = 3;
    public float height = 0f;

    private Vector3 startPos;
    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
        speed = Random.Range(1f, 3f);
        height = Random.Range(0.2f, 1.0f);
        transform.localScale = new Vector3(transform.localScale.x, height, transform.localScale.z);
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.x < -9)
        {
            Instantiate(this.gameObject, startPos, this.transform.rotation);
            Destroy(this.gameObject);

        }

        transform.position -= Vector3.right * speed * Time.deltaTime;
    }
}
