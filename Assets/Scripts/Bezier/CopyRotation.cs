using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyRotation : MonoBehaviour
{
    [SerializeField] public Transform TrackedTransform;
    [SerializeField] private  float handRotationRangeMin; //{-15} Left, {15} Right
    [SerializeField] private float handRotationRangeMax; //{45} for Left, {-45} for Right

    public float rotationDisplacement;
    // Start is called before the first frame update
    void Start()
    {
        rotationDisplacement = Random.Range(handRotationRangeMin,handRotationRangeMax);
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.SetPositionAndRotation(TrackedTransform.position, TrackedTransform.rotation);
        Vector3 oldrot = this.transform.localEulerAngles;
        this.transform.Rotate(new Vector3(0,0,rotationDisplacement), Space.Self);
    }
}
