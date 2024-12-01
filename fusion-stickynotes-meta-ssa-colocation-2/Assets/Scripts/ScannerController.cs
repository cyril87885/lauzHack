using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScannerController : MonoBehaviour
{
    public Vector3 startPoint;
    public Vector3 endPoint;
    private float currPosition;
    public float bpm = 120f;
    private float speed;
    // Start is called before the first frame update
    void Start()
    {
        float totalTime = (19 * 60) / bpm;
        speed = (endPoint.x - startPoint.x) / totalTime;
        currPosition = startPoint.x;

        transform.localPosition = new Vector3(currPosition, transform.localPosition.y, transform.localPosition.z);
    }

    // Update is called once per frame
    void Update()
    {
        currPosition += speed * Time.deltaTime;
        currPosition = (currPosition > endPoint.x) ? startPoint.x : currPosition;

        transform.localPosition = new Vector3(currPosition, transform.localPosition.y, transform.localPosition.z);
    }
}
