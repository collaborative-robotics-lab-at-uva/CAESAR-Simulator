using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class BezierTargetDriver : MonoBehaviour
{
    // Start is called before the first frame update

    public Transform arm;
    public Transform HandRotationSource;
    public Transform HandRotationAdjusted;

    public Transform bezierTarget;
    public Transform limpSphereTarget;
    public PhysicsAttractor attractorcube;

    [SerializeField] private Transform currTarget;
    public float armlength;
    
    public BezierSolution.BezierWalkerWithTime bezierWalker;
    public EaseWalker easeWalker;

    private GestureManager localGestureManager;

    [SerializeField] Animator animator;

    [SerializeField] TwoBoneIKConstraint armConstraint;
    [SerializeField] MultiAimConstraint headConstraint;
    [SerializeField] MultiAimConstraint chestConstraint;

    [SerializeField] ChainIKConstraint fingerConstraint;

    [SerializeField] private bool gesture;
    [SerializeField] private bool gaze;

    void Start()
    {   
        
        
    }

    public void SetAnimator(Animator anim){
        this.animator = anim;
    }
    public void SetConstraints(TwoBoneIKConstraint armConstraint, MultiAimConstraint headConstraint, MultiAimConstraint chestConstraint, ChainIKConstraint fingerConstraint){
        this.armConstraint = armConstraint;
        this.headConstraint = headConstraint;
        this.chestConstraint = chestConstraint;
        this.fingerConstraint = fingerConstraint;
    }

    public void SetLocalGestureManager(GestureManager gm){
        this.localGestureManager = gm;
    }

    public void Setup(bool gaze, bool gesture){
        armConstraint.weight = 0;
        chestConstraint.weight = 0;
        headConstraint.weight =0;
        fingerConstraint.weight = 0;
        //Debug.Log("Setup has been called.");
        armlength = (HandRotationSource.position -arm.position).magnitude;
        //Adjust HandRotation by a random amount of degrees on Z axis, {-15, 45} for Left, {15, -45} for Right
          
        attractorcube.enabled = true;
        attractorcube.strength = 10;

        this.gaze = gaze;
        this.gesture = gesture;
        bezierTarget.transform.position = HandRotationSource.transform.position;
        limpSphereTarget.position = HandRotationSource.transform.position;
        StartCoroutine(WaitForPhysicsSettle(gaze, gesture)); 
    }

    public IEnumerator WaitForPhysicsSettle(bool gaze, bool gesture){
        currTarget = bezierTarget;//TODO try reducing time
        
        yield return new WaitForSeconds(0.25f);
        if (gaze){
            StartCoroutine(FocusOnObject(1.5f));
        }
        if (gesture){
            easeWalker.SetTravelingTrue();
            armConstraint.weight = 1f;
            bezierWalker.enabled = true;
        }
        if (gaze && !gesture){
            yield return new WaitForSeconds(1.5f);

            HangLoose();

            yield return new WaitForSeconds(5f);

            armConstraint.weight = 0;
            chestConstraint.weight = 0;
            headConstraint.weight =0;
            fingerConstraint.weight = 0;
            localGestureManager.EndGesture();

        }else{
            yield return new WaitForSeconds(6.5f);

            armConstraint.weight = 0;
            chestConstraint.weight = 0;
            headConstraint.weight =0;
            fingerConstraint.weight = 0;
            localGestureManager.EndGesture();
        }

        //armConstraint.weight = 1f;
       
         
       
    }

    private IEnumerator FocusOnObject(float focustime){

        float t = 0;

        while (t<focustime){
            t += Time.deltaTime;
            headConstraint.weight = Mathf.Lerp(.3f,.9f, Tween.GetEasedValue(t, Tween.Easing.InOutQuad));
            chestConstraint.weight = Mathf.Lerp(0f,.6f, Tween.GetEasedValue(t, Tween.Easing.InOutQuad));
            
            yield return new WaitForFixedUpdate();

        }
        localGestureManager.scenemanager.recordingManager.SetStartKeyframe();


    }

    public IEnumerator PointAtObject(float pointtime){
        float t = 0;
        
        while(t<pointtime){
            t+=Time.deltaTime;
            fingerConstraint.weight = Mathf.Lerp(0f,.9f, t/pointtime);
            yield return new WaitForFixedUpdate();
        }
        localGestureManager.scenemanager.recordingManager.SetStartKeyframe();
        
    }



    public void HangLoose(){
        StartCoroutine(HoldPoint(Random.Range(.9f,3f)));
       
    }

    private IEnumerator HoldPoint(float t){
        float time=0;
        while (time < t){
            time+= Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }


        //TAKE SCREENSHOTS
        
        localGestureManager.scenemanager.TakeScreenshots();
        yield return new WaitForSeconds(0.2f);
        //int chance = Random.Range(0,2);
        if (gesture){
             StartCoroutine(HangLooseCoroutine(1f));
        }
       
        if (gaze){
            StartCoroutine(EndFocusOnObject(1f));  
        }
        localGestureManager.scenemanager.recordingManager.SetEndKeyframe();

        //if (chance > 0){

        //}else{
        //    limpSphereTarget.GetComponent<Rigidbody>().AddForce(Vector3.right*6f);
        //    fingerConstraint.weight = 0f;
        //}
        //
    }

    IEnumerator EndFocusOnObject(float focustime){
        float t = 0;
        
        float headweightbefore = headConstraint.weight;
        float chestweightbefore = chestConstraint.weight;


        while (t<focustime){
            t += Time.deltaTime;
            headConstraint.weight = Mathf.Lerp(headweightbefore,0f, Tween.GetEasedValue(t, Tween.Easing.InOutQuad));
            chestConstraint.weight = Mathf.Lerp(chestweightbefore,0f, Tween.GetEasedValue(t, Tween.Easing.InOutQuad));
            
            yield return new WaitForFixedUpdate();

        }

    }

    private IEnumerator HangLooseCoroutine(float transitiontime){
        float t = 0;
        currTarget = limpSphereTarget;
        float pointweightbefore = fingerConstraint.weight;
        animator.SetTrigger("RelaxPose");
        while (t< transitiontime){
            t+=Time.deltaTime;
            fingerConstraint.weight = Mathf.Lerp(pointweightbefore,0, t/ transitiontime);
            attractorcube.strength = Mathf.Lerp(10, 0, t/transitiontime); // todo increase transition time
            
            yield return  new WaitForEndOfFrame();
        }
        attractorcube.enabled = false;
        
        
        

    }

    // Update is called once per frame
    void Update()
    {
        if (currTarget != null){
            
            this.transform.position = currTarget.position;
            
        }
        
        transform.localRotation = HandRotationAdjusted.localRotation;
        
    }
}
