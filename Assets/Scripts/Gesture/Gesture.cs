using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class Gesture : MonoBehaviour
{
    [SerializeField] public Transform PointTarget;
    [SerializeField] GestureManager localGestureManager;

    [SerializeField] ChainIKConstraint armConstraintR;
    [SerializeField] ChainIKConstraint armConstraintL;

    [SerializeField] ChainIKConstraint activeConstraint;


    public bool leftyPoint; //whether the character is using the left arm to point vs the right arm.

    public Tween.Easing EasingFunctionPoint;
    public Tween.Easing EasingFunctionChest;
    public Tween.Easing EasingFunctionEyes;
    [SerializeField] MultiAimConstraint headAim;
    [SerializeField] MultiAimConstraint chestAim;
    [SerializeField] MultiAimConstraint eyeAimR;
    [SerializeField] MultiAimConstraint eyeAimL;
    [SerializeField] float headWeightTarget;
    [SerializeField] float chestWeightTarget;
    [SerializeField] float pointWeightTarget;
    [SerializeField] float eyeWeightTarget;
    [SerializeField] float TimeToLook; // Eyes sync
    [SerializeField] float TimeToFocus; // Head / Chest sync
    [SerializeField] float TimeToPoint; // Arm pointing sync
    [SerializeField] float TimeToEndPoint;
    [SerializeField] float PointLag;

    public bool AnimationPlaying;

    [SerializeField] RigBuilder rigBuilder;

    // Start is called before the first frame update
    void Start()
    {
        //armConstraintR = this.GetComponentInChildren<ChainIKConstraint>();
    }

    public void SetGestureTime(float timetolook, float timetofocus, float timetopoint, float timetopointend, float pointlag){
        TimeToLook = timetolook;
        TimeToFocus = timetofocus;
        TimeToPoint = timetopoint;
        TimeToEndPoint = timetopointend;
        PointLag = pointlag;

    }
    

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    public void setLocalGestureManager(GestureManager manager){
        localGestureManager = manager;
        return;
    }

    public bool getLeftyPoint(){
        return leftyPoint;
    }

    public void setLeftyPoint(bool value){
        leftyPoint = value;
        return;
    }

    void EndUpdateWeight(){

        StartCoroutine(EndPoint(EasingFunctionPoint));
        localGestureManager.scenemanager.TakeScreenshots();
    }
    public void AnimationEnd(){
        //Debug.Log("AnimationPlaying is off");
        AnimationPlaying = false;
        
    }


    public virtual void StartUpdateWeight(bool gaze, bool gesture)
    {

        //Debug.Log("gaze then gesture" + gaze + gesture); 
        if (leftyPoint){
            activeConstraint = armConstraintL;
        }else{
            activeConstraint = armConstraintR;
        }
        //Debug.Log("leftypoint is "+ leftyPoint);
        if (gaze)
            StartCoroutine(AdjustBody(EasingFunctionPoint));
        //StartCoroutine(AdjustBody(EasingFunctionChest));  
        if(gesture)
            StartCoroutine(StartPoint(EasingFunctionPoint));
        
        StartCoroutine(AdjustGaze(EasingFunctionEyes));


        
    }
    
    // Resets all constraints to 0.
  
    public IEnumerator StartPoint(Tween.Easing easingFunction)
    {
        // For StartPoint we should find a way to make sure the interpolation is complete before the AnimationEvent.time
        
        yield return new WaitForSeconds(PointLag);
        float t = 0;
        
        while (t <= TimeToPoint)
        {
            t += Time.deltaTime;
            activeConstraint.weight = Mathf.LerpUnclamped(0f, pointWeightTarget, Tween.GetEasedValue(t, easingFunction));
            yield return null;
        }
        
        //StartCoroutine(EndPoint(easingFunction));
        yield break;
    }
    // EndPoint is fired after the duration of the point is complete, called from EndUpdateWeight
    public IEnumerator EndPoint(Tween.Easing easingFunction)
    {
        //yield return new WaitForSeconds(PointLag);
        float t = 0;
        
        while (t <= TimeToPoint)
        {
            t += Time.deltaTime;
            activeConstraint.weight = Mathf.LerpUnclamped(pointWeightTarget, 0, Tween.GetEasedValue(t, Tween.Easing.Linear));
            yield return null;
        }
    
        
        //Debug.Log("Gesture Complete. Time: " + Time.fixedTime +" seconds");
        
        
        yield break;
    }

      public virtual void CleanUp()
    {
        armConstraintR.weight = 0;
        armConstraintL.weight = 0;
        if (activeConstraint != null)
            activeConstraint.weight = 0;
        headAim.weight = 0;
        chestAim.weight = 0;
        eyeAimR.weight = 0;
        eyeAimL.weight = 0;
         // calls GestureManager to finish cleaning up, fire OnGestureEndEvent;
         //localGestureManager.EndGesture();
    }
    public IEnumerator AdjustBody(Tween.Easing easingFunction)
    {
        float t = 0;
        while (t <= TimeToPoint)
        {
            t +=  Time.deltaTime;
            headAim.weight = Mathf.Lerp(0, headWeightTarget, Tween.GetEasedValue(t, easingFunction));
            chestAim.weight = Mathf.Lerp(0, chestWeightTarget, Tween.GetEasedValue(t, easingFunction));
            
            yield return null;
        }

        
        yield break;

    }

    public IEnumerator AdjustGaze(Tween.Easing easingFunction)
    {
        float t = 0;
        while (t <= TimeToLook)
        {
            t +=  Time.deltaTime;
            eyeAimL.weight = Mathf.Lerp(0, eyeWeightTarget, Tween.GetEasedValue(t, easingFunction));
            eyeAimR.weight = Mathf.Lerp(0, eyeWeightTarget, Tween.GetEasedValue(t, easingFunction));
            
            yield return null;
        }
        
        yield break;


    }

    // Update is called once per frame

    void Update()
    {
        
    }
}
