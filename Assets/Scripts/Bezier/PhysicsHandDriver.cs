using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsHandDriver : MonoBehaviour
{
    public Transform trackedTransform;
    public Rigidbody rb;
    public Transform forearm;

    public Vector3 neutralrotation;

    public float positionStrength = 15;

  


    // Start is called before the first frame update
    void Start()
    {
        //rb = this.GetComponent<Rigidbody>();
       
    }
    void SyncTargetRotation(){ //Sync target rotation with forearm rotation
        this.transform.rotation =Quaternion.Euler(forearm.localRotation.eulerAngles);
    }

    void FixedUpdate(){
        //var vel = (trackedTransform.position - rb.position).normalized * positionStrength * Vector3.Distance(trackedTransform.position, rb.position);
        //rb.velocity = vel;
        
    }

    // Update is called once per frame
    void Update()
    {
        //SyncTargetRotation();
    }
}
