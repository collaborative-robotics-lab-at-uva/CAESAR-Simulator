using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attributes : MonoBehaviour
{
    // Spublic class Attributes{

    [SerializeField] public string size; // large or small
    [SerializeField] public string color_name; // gray, red, blue, green, brown, purple, cyan, or yellow
    [SerializeField] public string absolute_location_participant;  // left, right, middle, top, bottom, corner

    [SerializeField] public string absolute_location_observer; // inverse of participant.
    [SerializeField] public bool non_colorable;

    public static bool Equals(Attributes first, Attributes second){
        if (
            first.size == second.size&&
            first.color_name == second.color_name &&
            first.absolute_location_observer == second.absolute_location_observer &&
            first.absolute_location_participant == second.absolute_location_participant){
                return true;
            }
        else return false;
    }

}
