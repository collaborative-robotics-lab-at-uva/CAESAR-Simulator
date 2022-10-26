using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BezierSolution{
    [ExecuteInEditMode]
    public class GeneratePointingBezier: MonoBehaviour{
        
        public bool generate;
        public bool righthandeddebug = false;

        [Header("Left")]
        public Transform restTransformL;
        public Transform armTransformL;
        public Transform handTransformL;
        public Vector3 originalRestL;

        [Header("Right")]

        public Transform restTransformR;
        public Transform armTransformR;
        public Transform handTransformR;

        public Vector3 originalRestR;

        [Header("Misc")]

        public Transform targetTransform;
        public GameObject beziertemplate;
        public GameObject activeBezier;
        public Collider personcollider;

        public BezierTargetDriver driver;

        public BezierWalkerWithTime bezierTargetR;
        public BezierWalkerWithTime bezierTargetL;
        [SerializeField] private float shoulderDistance;
        
        void Start(){
            shoulderDistance = (armTransformL.position -handTransformL.position).magnitude;
//            targetTransform = GameObject.FindGameObjectWithTag("Target").transform;
            originalRestL = restTransformL.position;
            originalRestR = restTransformR.position;

        }

        //TODO: create script that sets normalized T values and shifts them with a smooth curve + engages the correct finger chainIK at the correct rate.

        //TODO: Engage head / chest aim.

        public GameObject GetActiveBezier(){
            return activeBezier;
        }

        [ExecuteInEditMode]
        void Update(){
            if(generate){
                GeneratePath(righthandeddebug);
                generate = false;
            }
        }

        public void GeneratePath(bool righthanded){
                Destroy(activeBezier);

                if(!righthanded){
                    restTransformL.position = originalRestL;
                    float arm_extension_ratio = Random.Range(.7f,.95f); //percent of arm extended
                    Vector3 point_position = armTransformL.position + (((targetTransform.position - armTransformL.position).normalized * shoulderDistance *arm_extension_ratio));
                    activeBezier = GameObject.Instantiate(beziertemplate, Vector3.zero, Quaternion.identity) as GameObject;
                    activeBezier.transform.SetParent(this.transform.parent);
                    GameObject control1 = activeBezier.transform.GetChild(0).gameObject;
                    GameObject control2 = activeBezier.transform.GetChild(1).gameObject;
                    GameObject control3 = activeBezier.transform.GetChild(2).gameObject;
                    control1.transform.position = restTransformL.position;
                    control2.transform.position = (( handTransformL.position + point_position) / 2 ) + new Vector3(Random.Range(-.1f,.1f), Random.Range(-.1f,0.1f), Random.Range(-.1f,.1f));
                    control3.transform.position = point_position;
                    bezierTargetL.spline = activeBezier.GetComponent<BezierSpline>();
                    bezierTargetL.spline.AutoConstructSpline2();

                    //ensure no intersection with collider in path
                    while (personcollider.bounds.Contains(control2.transform.position)){
                        Destroy(activeBezier);

                        restTransformL.position = originalRestL;
                        arm_extension_ratio = Random.Range(.7f,.95f); //percent of arm extended
                        point_position = armTransformL.position + (((targetTransform.position - armTransformL.position).normalized * shoulderDistance *arm_extension_ratio));
                        activeBezier = GameObject.Instantiate(beziertemplate, Vector3.zero, Quaternion.identity) as GameObject;
                        activeBezier.transform.SetParent(this.transform.parent);
                        control1 = activeBezier.transform.GetChild(0).gameObject;
                        control2 = activeBezier.transform.GetChild(1).gameObject;
                        control3 = activeBezier.transform.GetChild(2).gameObject;
                        control1.transform.position = restTransformL.position;
                        control2.transform.position = (( handTransformL.position + point_position) / 2 ) + new Vector3(Random.Range(-.1f,.1f), Random.Range(-.1f, 0.1f), Random.Range(-.1f,.1f));
                        control3.transform.position = point_position;
                        bezierTargetL.spline = activeBezier.GetComponent<BezierSpline>();
                        bezierTargetL.spline.AutoConstructSpline2();

                    }
                }
                else{
                    restTransformR.position = originalRestR;
                    float arm_extension_ratio = Random.Range(.7f,.95f); //percent of arm extended
                    Vector3 point_position = armTransformR.position + (((targetTransform.position - armTransformR.position).normalized * shoulderDistance *arm_extension_ratio));
                    activeBezier = GameObject.Instantiate(beziertemplate, Vector3.zero, Quaternion.identity) as GameObject;
                    activeBezier.transform.SetParent(this.transform.parent);
                    GameObject control1 = activeBezier.transform.GetChild(0).gameObject;
                    GameObject control2 = activeBezier.transform.GetChild(1).gameObject;
                    GameObject control3 = activeBezier.transform.GetChild(2).gameObject;
                    control1.transform.position = restTransformR.position;
                    control2.transform.position = (( handTransformR.position + point_position) / 2 ) + new Vector3(Random.Range(-.1f,.1f), Random.Range(-.3f, 0.03f), Random.Range(-.1f,.1f));
                    control3.transform.position = point_position;
                    bezierTargetR.spline = activeBezier.GetComponent<BezierSpline>();
                    bezierTargetR.spline.AutoConstructSpline2();

                    //ensure no intersection with collider in path
                    while (personcollider.bounds.Contains(control2.transform.position)){
                        Destroy(activeBezier);

                        restTransformR.position = originalRestR;
                        arm_extension_ratio = Random.Range(.7f,.95f); //percent of arm extended
                        point_position = armTransformR.position + (((targetTransform.position - armTransformR.position).normalized * shoulderDistance *arm_extension_ratio));
                        activeBezier = GameObject.Instantiate(beziertemplate, Vector3.zero, Quaternion.identity) as GameObject;
                        activeBezier.transform.SetParent(this.transform.parent);
                        control1 = activeBezier.transform.GetChild(0).gameObject;
                        control2 = activeBezier.transform.GetChild(1).gameObject;
                        control3 = activeBezier.transform.GetChild(2).gameObject;
                        control1.transform.position = restTransformR.position;
                        control2.transform.position = (( handTransformR.position + point_position) / 2 ) + new Vector3(Random.Range(-.1f,.1f), Random.Range(-.3f, 0.03f), Random.Range(-.1f,.1f));
                        control3.transform.position = point_position;
                        bezierTargetR.spline = activeBezier.GetComponent<BezierSpline>();
                        bezierTargetR.spline.AutoConstructSpline2();

                    }
                    
                }
                

        }

        

    }
}