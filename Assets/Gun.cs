using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] protected GameObject bullet;
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
    protected virtual void Start()
    {
        nextAvailFire = Time.time;
        aimTransform = transform.parent;
        ammo = magazineCount;
        muzzle = transform.GetChild(0).transform;
    }

    public virtual void Fire() {
        if(Time.time >= nextAvailFire && ammo > 0){
            Vector3 muzzlePos = muzzle.transform.position;
            Vector2 shootDirection = muzzle.right;

            float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;

            var bullet = Instantiate(this.bullet, muzzlePos, Quaternion.Euler(0f, 0f, angle));
            bullet.GetComponent<Rigidbody2D>().AddForce(shootDirection * 50, ForceMode2D.Impulse);

            ammo--;
            nextAvailFire = Time.time + 1/fireRate;
        }else{
            Debug.Log("outta ammo");
        }
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

    public void SetAmmo(int num){
        ammo = num;
    }
}
