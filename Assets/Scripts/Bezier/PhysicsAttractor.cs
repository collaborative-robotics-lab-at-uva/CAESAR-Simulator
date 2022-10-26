using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PhysicsAttractor : MonoBehaviour
{


    public Rigidbody attractedRb;
    [Range(0,100)]
    public float strength;
    public Vector3 forcedirection;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void  FixedUpdate()
    {
        forcedirection =  this.transform.position - attractedRb.transform.position;
        attractedRb.velocity = strength*forcedirection;
        //attractedRb.AddForce(strength *forcedirection);
    }

   
}
