using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    private int pickupDist = 2;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if ((PlayerController.instance.characterPos - transform.position).magnitude < pickupDist){
            PlayerController.instance.canPickup = true;
            if (PlayerController.instance.isPickingUp){
                PlayerController.instance.isPickingUp = false;
                transform.SetParent(PlayerController.instance.transform);
                transform.position = PlayerController.instance.transform.position;
                PlayerController.instance.Pickup(gameObject.tag);
                gameObject.SetActive(false);
            }
        }
    }
}
///wasdasfasd
