using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

[ExecuteInEditMode]
public class AssignSkeletonTransforms : MonoBehaviour
{
    // Start is called before the first frame update

    public bool assignstuff = false;

    public string rignum;

    [Header("references")]
    public Transform TrackedRotR;
    public Transform TrackedRotL;

    [Header("locals")]

    public EaseWalker BezierEaseL;
    public EaseWalker BezierEaseR;
    public BezierTargetDriver IKL;
    public BezierTargetDriver IKR;

    public BezierSolution.GeneratePointingBezier generatePointingBezier;
    public TwoBoneIKConstraint armRconst;
    public TwoBoneIKConstraint armLconst;
    public ChainIKConstraint fingRconst;
    public ChainIKConstraint fingLconst;
    public MultiAimConstraint chestconst;
    public MultiAimConstraint headconst;
    public CopyRotation copyrotL;
    public CopyRotation copyrotR;
    public Transform other;
    void Start()
    {
        
    }

    [ExecuteInEditMode]
    void Update(){
        if (assignstuff){

            AssignConstraintsByName(rignum);
            AssignDynamicBone(rignum);
            AssignOtherInspectorVars();
            assignstuff = false;
        }

    }



    Transform FindInList(Transform[] list, string name){

        foreach(Transform t in list){
            if (t.name == name){
                return t;
            }
        }
        Debug.Log("couldn't find "+ name + " in children...");
        return null;
         

    }
 
 
    void AssignOtherInspectorVars(){
        Transform BezTargetL = BezierEaseL.transform;
        Transform BezTargetR = BezierEaseR.transform;

        Animator anim = this.GetComponent<Animator>();
        BezTargetL.GetComponent<EaseWalker>().SetAnimator(anim);
        BezTargetR.GetComponent<EaseWalker>().SetAnimator(anim);
        IKL.SetAnimator(anim);
        IKR.SetAnimator(anim);
        IKL.SetConstraints(armLconst, headconst, chestconst, fingLconst);
        IKR.SetConstraints(armRconst, headconst, chestconst, fingRconst);
    }

    void AssignDynamicBone(string rignum){
        DynamicBone db = this.GetComponent<DynamicBone>();

        List<Transform> shoulders = new List<Transform>();
        
        Transform[] all = transform.GetComponentsInChildren<Transform>();
        string prefix = "mixamorig" + rignum + "_";
        shoulders.Add(FindInList(all, prefix + "LeftShoulder"));
        shoulders.Add(FindInList(all, prefix + "RightShoulder"));
        db.m_Roots = shoulders;

        List<Transform> exclusions = new List<Transform>();
        exclusions.Add(FindInList(all, prefix + "LeftHand"));
        exclusions.Add(FindInList(all, prefix + "RightHand"));
        exclusions.Add(FindInList(all, prefix + "LeftHandIndex1"));
        exclusions.Add(FindInList(all, prefix + "LeftHandIndex2"));
        exclusions.Add(FindInList(all, prefix + "LeftHandIndex3"));
        
        exclusions.Add(FindInList(all, prefix + "RightHandIndex1"));
        exclusions.Add(FindInList(all, prefix + "RightHandIndex2"));
        exclusions.Add(FindInList(all, prefix + "RightHandIndex3"));
        
        exclusions.Add(FindInList(all, prefix + "RightHandPinky1"));
        exclusions.Add(FindInList(all, prefix + "RightHandMiddle1"));
        exclusions.Add(FindInList(all, prefix + "RightHandRing1"));

        exclusions.Add(FindInList(all, prefix + "LeftHandMiddle1"));
        exclusions.Add(FindInList(all, prefix + "LeftHandRing1"));
        exclusions.Add(FindInList(all, prefix + "LeftHandPinky1"));
        db.m_Exclusions =exclusions;
    }

    void AssignConstraintsByName(string rignum){
        string prefix = "mixamorig" + rignum + "_";

        Transform[] all = transform.GetComponentsInChildren<Transform>();
        foreach(Transform t in all){
            Debug.Log(t.name);
        }

        Transform leftarm = FindInList(all, prefix+ "LeftArm");
        Transform rightarm = FindInList(all, prefix+ "RightArm");
        Transform leftforearm = FindInList(all, prefix + "LeftForeArm");
        Transform rightforearm = FindInList(all, prefix + "RightForeArm");
        Transform lefthand = FindInList(all, prefix+ "LeftHand");
        Transform righthand = FindInList(all, prefix+ "RightHand");

        Transform lefthandindex4 = FindInList(all, prefix + "LeftHandIndex4");
        Transform righthandindex4 = FindInList(all, prefix + "RightHandIndex4");

        IKL.arm = leftarm;
        IKR.arm = rightarm;
        generatePointingBezier.armTransformL = leftarm;
        generatePointingBezier.armTransformR = rightarm;

        generatePointingBezier.handTransformL = lefthand;
        generatePointingBezier.handTransformR = righthand;

        generatePointingBezier.personcollider = this.GetComponentInChildren<CapsuleCollider>();

        armLconst.data.root  = leftarm;
        armLconst.data.mid = leftforearm;
        armLconst.data.tip = lefthand;

        armRconst.data.root = rightarm;
        armRconst.data.root = rightforearm;
        armLconst.data.root = righthand;

        fingLconst.data.root = lefthand;
        fingLconst.data.tip = lefthandindex4;

        fingRconst.data.tip = righthand;
        fingRconst.data.tip = righthandindex4;

        Transform spine2 = FindInList(all, prefix + "Spine2");
        Transform head = FindInList(all, prefix + "Head");

        chestconst.data.constrainedObject = spine2;
        headconst.data.constrainedObject = head;
        if (FindInList(all, TrackedRotL.name + "(Clone)") == null){
            GameObject trackedrotL = Instantiate(TrackedRotL.gameObject) as GameObject;
            trackedrotL.transform.SetParent(leftforearm);
            trackedrotL.transform.position = lefthand.position;
            trackedrotL.transform.localRotation = lefthand.transform.localRotation;
            trackedrotL.transform.localRotation.eulerAngles.Set(0,0,30);
            copyrotL.TrackedTransform = trackedrotL.transform;
        }
        IKL.HandRotationSource = FindInList(all, TrackedRotL.name + "(Clone)");
        if (FindInList(all, TrackedRotR.name + "(Clone)") == null){
            GameObject trackedrotR = Instantiate(TrackedRotR.gameObject ) as GameObject;
            trackedrotR.transform.SetParent(rightforearm);
            trackedrotR.transform.localRotation = righthand.localRotation;
            trackedrotR.transform.position = righthand.position;
            trackedrotR.transform.localRotation.eulerAngles.Set(0,0,-20);
            copyrotR.TrackedTransform = trackedrotR.transform ;
        }
        IKR.HandRotationSource = FindInList(all, TrackedRotR.name + "(Clone)");
        

        
    }

    

    // void ReAssignSkeleton(){
    //     SkinnedMeshRenderer rend = gameObject.GetComponent<SkinnedMeshRenderer>();
    //     Transform[] bones = rend.bones;
    //     Transform[] children = newSkeleton.GetComponentsInChildren<Transform>();
    //     //mixamorig -substring 0-8
    //     for (int i = 0; i < bones.Length; i++)
    //          for (int a = 0; a < children.Length; a++)
    //              if (bones[i].name == children[a].name.Substring(0,8) + children[a].name.Substring(8)) {
    //                  bones[i] = children[a];
    //                  break;
    //              }
 
    //      rend.bones = bones;


    // }
    // void ReAssignSkeleton2(){
    //     foreach(SkinnedMeshRenderer r in oldrends){
    //         r.bones = newrend.bones;
    //         r.transform.SetParent(newrend.transform.parent);
            
    //     }
    // }



 
}
