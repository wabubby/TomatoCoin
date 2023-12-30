using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] protected GameObject bulletTrail;
    protected int ammo;
    protected int magazineCount;
    protected Vector3 mousePos;
    protected Transform aimTransform; //pivot
    protected Transform muzzle; //firepoint
    protected float fireRate;
    protected float nextAvailFire;
    protected float reloadTime;
    protected float damage;
    protected float bulletSpeed;
    public bool canShoot;

    // Start is called before the first frame update
    protected void Start()
    {
        nextAvailFire = Time.time;
        aimTransform = transform.parent;
        ammo = magazineCount;
        // muzzle = transform.GetChild(0).transform;
    }

    // Update is called once per frame
    protected void Update(){
        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        //orient gun around pivot point --> towards mousePos
        Vector2 aimDirection = mousePos - aimTransform.position;
        float aimAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        aimTransform.eulerAngles = new Vector3(0,0,aimAngle);

        //flip gun 
        Vector3 aimLocalScale = Vector3.one;
        if(aimAngle > 90 || aimAngle < -90){
            aimLocalScale.y = -1f;
        }else{
            aimLocalScale.y = 1f;
        }
        aimTransform.localScale = aimLocalScale;
    }
}
