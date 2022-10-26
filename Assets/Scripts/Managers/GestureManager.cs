using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// These will translate to different source clips for the animation template.
public enum GestureType{
    Point1R,
    Point1L,
    Point2R,
    Point2L

}

public class GestureManager : MonoBehaviour
{
    public Animator HumanAnim;
    //public Gesture HumanGesture; 
    public BezierTargetDriver IKTargetL;
    public BezierTargetDriver IKTargetR;

    [SerializeField] private CopyRotation copyRotationL;
    [SerializeField] private CopyRotation copyRotationR;
    
    [SerializeField] public bool AnimationPlaying {get; private set;}

    public BezierSolution.GeneratePointingBezier generatePointingBezier;
    public SceneManager scenemanager;

    public bool busy = false;


    void Start(){
    }

    public void AssignFields(Transform otherstuff, GameObject person){
        generatePointingBezier = person.GetComponent<BezierSolution.GeneratePointingBezier>();
        HumanAnim = person.GetComponent<Animator>();
        BezierTargetDriver[] IKTargets = otherstuff.GetComponentsInChildren<BezierTargetDriver>();
        IKTargetL = IKTargets[0];
        IKTargetR = IKTargets[1];

        CopyRotation[] copyRotations = otherstuff.GetComponentsInChildren<CopyRotation>();
        copyRotationL = copyRotations[0];
        copyRotationR = copyRotations[1];

    }

    public void BeginGesture(bool gaze, bool gesture){
        AnimationPlaying = true;
        
        //Determine Right or Left Point

        int righthandedint = Random.Range(0,2);
        bool righthanded;
        if (righthandedint == 0){
            copyRotationR.enabled = false;
            righthanded = false;
        }else{
            copyRotationL.enabled = false;
            righthanded = true;
        }
        //first generate path, then begin pointing

        generatePointingBezier.GeneratePath(righthanded);

        int rightindex = HumanAnim.GetLayerIndex("Right Hand");
        int leftindex = HumanAnim.GetLayerIndex("Left Hand");
        if (!righthanded){
            IKTargetL.Setup(gaze, gesture);
            HumanAnim.SetLayerWeight(leftindex,1f);
            HumanAnim.SetLayerWeight(rightindex,0f);
        }   
        else{
            //first generate path, then begin pointing

            IKTargetR.Setup(gaze,gesture);
            HumanAnim.SetLayerWeight(leftindex,0f);
            HumanAnim.SetLayerWeight(rightindex,1f);
        }
       

    }

    public void EndGesture(){
        copyRotationL.enabled = true;
        copyRotationR.enabled = true;
        IKTargetL.transform.position = IKTargetL.HandRotationSource.position;
        IKTargetR.transform.position = IKTargetR.HandRotationSource.position;

        IKTargetL.bezierWalker.enabled = false;
        IKTargetR.bezierWalker.enabled = false;
        IKTargetL.bezierWalker.NormalizedT = 0;
        IKTargetR.bezierWalker.NormalizedT = 0;
        AnimationPlaying = false;
    }

    //BELOW is the OLD BeginGesture method for the data-driven animations.    


    
    // public IEnumerator BeginGesture(bool gaze, bool gesture)
    // {
    //     //THIS IS SOURCE OF BUGS.
    //     //TODO: Add variability based on GestureType, link animation clips with an id, set trigger accordingly
        
    //     //Debug.Log("Gesture Begin. Time: " + Time.fixedTime + " seconds");
        
    //     int pointtype = Random.Range(0,4);

    //     if(gesture){
    //         switch (pointtype){
    //             case 0: //Point1R
    //             // LeonardAnim.SetTrigger("point1R");
    //                 //Debug.Log("ONPOINT IS point1R");
    //                 HumanGesture.setLeftyPoint(false);
                    
    //             break;
    //             case 1: //Point1L
    //                 //LeonardAnim.SetTrigger("point1L");
    //                 //Debug.Log("ONPOINT IS point1L");
    //                 HumanGesture.setLeftyPoint(true);
    //             break;
    //             case 2: //Point2R
    //                 //LeonardAnim.SetTrigger("point2R");
    //                 //Debug.Log("ONPOINT IS point2R");
    //                 HumanGesture.setLeftyPoint(false);
    //             break;
    //             case 3: //Point2L
    //             // LeonardAnim.SetTrigger("point2L");
    //                 //Debug.Log("ONPOINT IS point2L");
    //                 HumanGesture.setLeftyPoint(true);
    //             break;
    //         }
    //     }

    //     //Debug.Log(HumanGesture); 
    //     //OnPoint
    //     HumanGesture.StartUpdateWeight(gaze, gesture);

    //     yield return new WaitForSeconds(.5f); //wait .5 seconds to synchronize point.
    //     if (gesture){
    //         switch (pointtype){
    //         case 0: //Point1R
    //             HumanAnim.SetTrigger("point1R");
    //             // Debug.Log("ONPOINT IS point1R");
    //             //LeonardGesture.setLeftyPoint(false);
                
    //         break;
    //         case 1: //Point1L
    //             HumanAnim.SetTrigger("point1L");
    //              //Debug.Log("ONPOINT IS point1L");
    //             //LeonardGesture.setLeftyPoint(true);
    //         break;
    //         case 2: //Point2R
    //             HumanAnim.SetTrigger("point2R");
    //            //  Debug.Log("ONPOINT IS point2R");
    //             // LeonardGesture.setLeftyPoint(false);
    //         break;
    //         case 3: //Point2L
    //             HumanAnim.SetTrigger("point2L");
    //             // Debug.Log("ONPOINT IS point2L");
    //             //LeonardGesture.setLeftyPoint(true);
    //         break;
    //         }

    //     }
    // }

    // public void EndGesture()
    // {
       

    //     busy = false;
    //     HumanGesture.leftyPoint = false;

    // }

}
