using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{

    public Circuit circuit;
    Drive ds;
    public float steeringSensitivity = 0.01f;
    public float brakingSensitivity = 3.0f;
    Vector3 target;
    Vector3 nextTarget;
    int currentWP = 0;
    float totalDistanceToTarget;

    // Start is called before the first frame update
    void Start()
    {
        ds = this.GetComponent<Drive>();
        target = circuit.waypoints[currentWP].transform.position;
        nextTarget = circuit.waypoints[currentWP + 1].transform.position;
        totalDistanceToTarget = Vector3.Distance(target, ds.rb.gameObject.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 localTarget = ds.rb.gameObject.transform.InverseTransformPoint(target);
        Vector3 nextLocalTarget = ds.rb.gameObject.transform.InverseTransformPoint(nextTarget);

        float distanceToTarget = Vector3.Distance(target, ds.rb.gameObject.transform.position);

        float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
        float nextTargetAngle = Mathf.Atan2(nextLocalTarget.x, nextLocalTarget.z) * Mathf.Rad2Deg;

        float steer = Mathf.Clamp(targetAngle * steeringSensitivity, -1, 1) * Mathf.Sign(ds.currentSpeed);

        float distanceFactor = distanceToTarget / totalDistanceToTarget;
        float speedFactor = ds.currentSpeed / ds.maxSpeed;

        float accel = 1f;
        float brake = Mathf.Lerp((-1 - Mathf.Abs(nextTargetAngle)) * brakingSensitivity, 1 + speedFactor, 1 - distanceFactor);

        Debug.Log("Brake: " + brake + " Accel: " + accel + " Speed: " + ds.rb.velocity.magnitude);

        // if(distanceToTarget < 5) { brake = 0.7f; accel = 0.3f; }

        ds.Go(accel, steer, brake);

        if(distanceToTarget < 4) // threshold, make larger if car starts to circle waypoint
        {
            currentWP++;
            if(currentWP >= circuit.waypoints.Length)
            {
                currentWP = 0;
            }
            target = circuit.waypoints[currentWP].transform.position;
            nextTarget = circuit.waypoints[currentWP + 1].transform.position;
            totalDistanceToTarget = Vector3.Distance(target, ds.rb.gameObject.transform.position);
        }

        ds.CheckForSkid();
        ds.CalculateEngineSound();
    }
}
