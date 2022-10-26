using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointEvent : MonoBehaviour
{
    public Animator humanAnimator;
    public void TriggerPointPose(){
        humanAnimator.SetTrigger("PointPose");

    }

    public void TriggerRelaxPose(){
        humanAnimator.SetTrigger("RelaxPose");
    }

}

    
