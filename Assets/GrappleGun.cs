using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleGun : Gun
{
    protected override void Start(){
        magazineCount = 5;
        fireRate = 2.5f;
        base.Start();
    }
    // Update is called once per frame
    // void Update()
    // {
        
    // }
}
