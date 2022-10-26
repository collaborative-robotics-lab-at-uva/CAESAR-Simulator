using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class EyeTarget : MonoBehaviour
{
    [SerializeField] private Transform eye;
    [SerializeField] private LookAtConstraint eyeconstraint;
    // Start is called before the first frame update
    void Start()
    {
        eyeconstraint = this.GetComponent<LookAtConstraint>();
        
    }
 
    void LookAt()
    {
        Debug.Log("Eye Tracking Enabled");
        eyeconstraint.weight = Mathf.LerpUnclamped(0f, 1f, Tween.GetEasedValue(1, Tween.Easing.InExpo));
    }
    public void  StopLookAt()
    {
        Debug.Log("Eye Tracking Disabled");
        eyeconstraint.weight = 0;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
