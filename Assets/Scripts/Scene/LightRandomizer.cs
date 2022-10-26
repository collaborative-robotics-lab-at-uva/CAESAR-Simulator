using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

[RequireComponent(typeof(Light))] public class LightRandomizer: MonoBehaviour

{
    [SerializeField] Transform lightconstraint1; // set on the prefab.
    [SerializeField] Transform lightconstraint2; // set on the prefab.
    

    //   access light component, randomize stuff.
    public void Randomize(){
        
        var l = this.GetComponent<Light>();
        if (l != null){
            l.colorTemperature = (float)Random.Range(4000, 12000); // Light temperature in Kelvin (according to blackbody radiation) 6500 is pure white.
            l.intensity = (float)Random.Range(300,3000); //Light intensity a value between 300 - 3000 (min inclusive max exclusive) Lumens
        }

        if (lightconstraint1 !=null  && lightconstraint1 != null){
            this.transform.position = 
            new Vector3( Random.Range(lightconstraint1.position.x,lightconstraint2.position.x),
                Random.Range(lightconstraint1.position.y,lightconstraint2.position.y), Random.Range(lightconstraint1.position.z,lightconstraint2.position.z)); // chose random location within the light constraints.
        }

    }


}