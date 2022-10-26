using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EaseWalker : MonoBehaviour
{

    public BezierSolution.BezierWalkerWithTime walkerWithTime;
    public Tween.Easing easingFunction;

    public BezierTargetDriver bezierTargetDriver;
    Vector3 neutralrotation;

    [SerializeField] bool looping;
    [SerializeField] bool traveling;

    [SerializeField] Animator animator;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void SetAnimator(Animator anim){
        this.animator = anim;
    }

    public void SetTravelingFalse(){
        traveling = false;
        if(looping){
            SetTravelingTrue();
        }
    }
    public void SetTravelingTrue(){
        traveling = true;
        StartCoroutine(EasedInterpolate(easingFunction));
        
    }

    
    // Update is called once per frame
    void Update()
    {
        
    }



     public IEnumerator EasedInterpolate(Tween.Easing easingFunction)
    {
        float t = 0;

        bool trigger= false;
        
        while (t <= 1 && traveling)
        {
             t += Time.deltaTime/ walkerWithTime.travelTime;
            //Debug.Log("t = " + t);
            //Debug.Log("normalized t = " +walkerWithTime.NormalizedT);
           
            walkerWithTime.NormalizedT = Mathf.Lerp(0f, walkerWithTime.travelTime, Tween.GetEasedValue(t, easingFunction))/walkerWithTime.travelTime;

            if (!trigger && walkerWithTime.NormalizedT > .8f){
                StartCoroutine(bezierTargetDriver.PointAtObject(walkerWithTime.travelTime * .2f));
                animator.SetTrigger("PointPose");
                trigger = true;
            }
            yield return null;
        }
        traveling = false;
        bezierTargetDriver.HangLoose();
        this.enabled = false;
        yield break;
    }
}
