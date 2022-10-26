using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Vectrosity;

class SBoneRenderer : MonoBehaviour
{
    [SerializeField] private Transform rootNode;
    [SerializeField] private Transform[] childNodes;
    [SerializeField] private VectorLine[] vectorLines;
    [SerializeField] private float lineWidthMultiplier = 5;

    void OnEnable()
    {
        if (rootNode != null)
        {
            if (childNodes == null || childNodes.Length == 0)
            {
                PopulateChildren();
            }
        }
        vectorLines = new VectorLine[childNodes.Length];
        for (int i = 0; i < childNodes.Length; i++)
        {
            if (childNodes[i] == rootNode || childNodes[i].gameObject.CompareTag("IgnoreBones"))
            {
                continue;
            }
            float width = lineWidthMultiplier * Vector3.Distance(childNodes[i].position, childNodes[i].parent.position);
            List<Vector3> linePoints = new List<Vector3> { childNodes[i].position, childNodes[i].parent.position };
            vectorLines[i] = new VectorLine("bone", linePoints, Mathf.Ceil(width));
            vectorLines[i].color = Color.black;
            vectorLines[i].layer = LayerMask.NameToLayer("BoneLines");
            vectorLines[i].Draw3D();
            vectorLines[i].rectTransform.SetParent(rootNode, true);
        }
    }
    private void Update()
    {
        for (int i = 0; i < childNodes.Length; i++)
        {
            //childNodes[i].name.Contains("Constraint") || childNodes[i].name.Contains("Aim") || childNodes[i].name.Contains("Target") || childNodes[i].name.Contains("Armature") || childNodes[i].name.Contains("Sneakers") || childNodes[i].name.Contains("Pants")
            if (childNodes[i] == rootNode || childNodes[i].gameObject.CompareTag("IgnoreBones"))
            {
                continue;
            }
            else
            {
                
                float width = lineWidthMultiplier * Vector3.Distance(childNodes[i].position, childNodes[i].parent.position);
                vectorLines[i].points3 = new List<Vector3> { childNodes[i].position, childNodes[i].parent.position };
                vectorLines[i].lineWidth = Mathf.Max(Mathf.Ceil(width), 2);
                vectorLines[i].Draw3D();

            }
        }
    }
    public void PopulateChildren()
    {
        childNodes = rootNode.GetComponentsInChildren<Transform>();
    }
}
