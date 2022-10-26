using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class EgoCameraController : MonoBehaviour
{
    [SerializeField] private Vector3 offset;// used to offset camera from body

    private void Update()
    {
        Vector3 parentTransform = gameObject.transform.parent.gameObject.transform.position;
        //Debug.Log(parentTransform);
        gameObject.transform.position = parentTransform + offset;
        //Debug.Log(gameObject.transform.position);

    }

}