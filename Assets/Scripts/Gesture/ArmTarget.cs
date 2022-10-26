using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class ArmTarget : MonoBehaviour
{

    [SerializeField] Transform targetTransform;
    [SerializeField] Transform root;
    [SerializeField] Vector3 armDisplacement;
    [SerializeField] float reach;
    // Start is called before the first frame update
    void Start()
    {
        targetTransform = GameObject.FindGameObjectWithTag("Target").transform;

        root = gameObject.GetComponentInParent<Transform>();

    }

    // Update is called once per frame
    void Update()
    {
        armDisplacement = (targetTransform.position - root.position).normalized * reach;

        this.transform.SetPositionAndRotation(armDisplacement, Quaternion.identity);
    }
}
