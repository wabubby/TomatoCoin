    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Cinemachine;
    using Unity.VisualScripting;
    using UnityEngine;

    enum Guns{
        NormalGun,
        GrappleGun,
        VacuumGun
    }

    public class PlayerController : MonoBehaviour
    {   
        //singleton
        public static PlayerController instance;
    
        //Movement
        public float moveSpeed;
        private Vector2 movementDirection;
        public ContactFilter2D contactFilter;
        private float collisionOffset = 0.04f;

        public Vector3 characterPos; //used by enemy and bullet class 

        //Components
        private SpriteRenderer sr;
        private Animator animator;
        private Transform aimPivot;
        private Rigidbody2D rb;
        private List<RaycastHit2D> raycastHit2Ds;

        //Player Stats
        public static int maxHealth;

        public static int health;

        //Virtual Cam
        [SerializeField] CinemachineVirtualCamera cam;

        private Coroutine activeCoroutine;

        private const float DEFAULTZOOM = 5f;

        [SerializeField] GameObject ePrompt;

        public bool canPickup;
        public bool isPickingUp;
        private Gun equippedGun;
        private Gun primaryGun;
        private Gun secondaryGun;

        //Gun objects
        // [SerializeField] GameObject normalGun;
        [SerializeField] GameObject vacuumGun;
        [SerializeField] GameObject grappleGun;

        private void Start(){
            instance = this;
            moveSpeed = 5f;
            rb = GetComponent<Rigidbody2D>();
            raycastHit2Ds = new List<RaycastHit2D>(0);
            sr = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            aimPivot = transform.GetChild(0).gameObject.transform;
            maxHealth = 500;
            health = maxHealth;
            activeCoroutine = null;
            canPickup = false;
            isPickingUp = false;
            equippedGun = null;
            primaryGun = null;
            secondaryGun = null;
        }

        private void DisableGuns(){
            for(int i = 0; i < aimPivot.childCount; i++){
                if (aimPivot.GetChild(i).GetComponent<Gun>() == equippedGun){
                    aimPivot.GetChild(i).gameObject.SetActive(true);
                }else{
                    aimPivot.GetChild(i).gameObject.SetActive(false);
                }
            }


            // if (equippedGun == null){
            //     for (int i = 0; i < Enum.GetNames(typeof(Guns)).Length; i++){
            //         aimPivot.GetChild(i).gameObject.SetActive(false);
            //     }            
            // }else if (equippedGun is NormalGun){
            //     aimPivot.GetChild((int)Guns.NormalGun).gameObject.SetActive(true);
            //     for (int i = 0; i < Enum.GetNames(typeof(Guns)).Length; i++){
            //         if (i != (int)Guns.NormalGun){
            //             aimPivot.GetChild(i).gameObject.SetActive(false);
            //         }
            //     }
            // }else if (equippedGun is GrappleGun){
            //     aimPivot.GetChild((int)Guns.GrappleGun).gameObject.SetActive(true);
            //     for (int i = 0; i < Enum.GetNames(typeof(Guns)).Length; i++){
            //         if (i != (int)Guns.GrappleGun){
            //             aimPivot.GetChild(i).gameObject.SetActive(false);
            //         }
            //     }
            // }else if (equippedGun is VacuumGun){
            //     aimPivot.GetChild((int)Guns.VacuumGun).gameObject.SetActive(true);
            //     for (int i = 0; i < Enum.GetNames(typeof(Guns)).Length; i++){
            //         if (i != (int)Guns.VacuumGun){
            //             aimPivot.GetChild(i).gameObject.SetActive(false);
            //         }
            //     }
            // }
        }
        private void Update(){ 
            Debug.Log(equippedGun + "|" + primaryGun + ", " + secondaryGun);
            
            DisableGuns();

            //shoot
            if (Input.GetMouseButtonDown(0)) {
                equippedGun.Fire();
            }
            


            //pickup    
            if (canPickup){
                ePrompt.SetActive(true);
                if (Input.GetKeyDown(KeyCode.E)){
                    isPickingUp = true;
                }
            }else{
                ePrompt.SetActive(false);
            }
            
            //zoom
            if (Input.GetMouseButtonDown(1)) {
                if (activeCoroutine != null) {
                    StopCoroutine(activeCoroutine);
                }
                activeCoroutine = StartCoroutine(ZoomOut(DEFAULTZOOM*2));
            }else if (Input.GetMouseButtonUp(1)) {
                if (activeCoroutine != null){
                    StopCoroutine(activeCoroutine);
                }
                activeCoroutine = StartCoroutine(ZoomIn());
            }
        
            //gun switching
            if (Input.GetKeyDown(KeyCode.Alpha1)){
                if (primaryGun != null){
                    equippedGun = primaryGun.GetComponent<Gun>();
                }
            }else if (Input.GetKeyDown(KeyCode.Alpha2)){
                if (secondaryGun != null){
                    equippedGun = secondaryGun.GetComponent<Gun>();
                }
            }

            //movement
            movementDirection.x = Input.GetAxisRaw("Horizontal");
            movementDirection.y = Input.GetAxisRaw("Vertical");
            characterPos = transform.position; 

            canPickup = false;
        }

        private void FixedUpdate() {
            //movement loop
            if(movementDirection != Vector2.zero){
                if(CanMove(movementDirection)){
                    rb.MovePosition(rb.position + movementDirection * moveSpeed * Time.fixedDeltaTime);
                    animator.SetBool("isWalking", true);
                }else{
                    if(CanMove(new Vector2(movementDirection.x, 0))){
                        rb.MovePosition(rb.position + moveSpeed * Time.fixedDeltaTime * new Vector2(movementDirection.x, 0));
                        animator.SetBool("isWalking", true);
                    }else if(CanMove(new Vector2(0, movementDirection.y))){
                            rb.MovePosition(rb.position + moveSpeed * Time.fixedDeltaTime * new Vector2(0, movementDirection.y));
                            animator.SetBool("isWalking", true);
                    }else{
                        animator.SetBool("isWalking", false);
                    } 
                }
            }else{
                animator.SetBool("isWalking", false); 
            }

            //flip the character sprite to face movement direction
            if(movementDirection.x > 0){
                sr.flipX = false;
            }else if(movementDirection.x < 0){
                sr.flipX = true;
            }
        }

        private void DropEquippedGun(){
            for(int i = 0; i < transform.childCount; i++){
                if (transform.GetChild(i).CompareTag(equippedGun.GetType() + "")){
                    transform.GetChild(i).gameObject.SetActive(true);
                    transform.GetChild(i).SetParent(null);
                }
            }

        }

        private bool HasGun(GameObject gun){
            for(int i = 0; i < aimPivot.childCount; i++){
                if (aimPivot.GetChild(i).gameObject == gun){
                    return true;
                }
            }
            return false;
        }


        public void Pickup(GameObject gun){
            if (gun.GetComponent<Gun>() is NormalGun){
                
                if (HasGun(gun)){
                    GameObject normalGun = gun;
                    if (primaryGun == null){
                        primaryGun = normalGun.GetComponent<Gun>();
                    }else{
                        if (secondaryGun == null){
                            secondaryGun = normalGun.GetComponent<Gun>();
                        }else{
                            DropEquippedGun();
                            if (equippedGun == primaryGun){
                                primaryGun = normalGun.GetComponent<Gun>();
                                equippedGun = primaryGun;
                            }else{
                                secondaryGun = normalGun.GetComponent<Gun>();
                                equippedGun = secondaryGun;
                            }
                        }
                    }
                    equippedGun = normalGun.GetComponent<Gun>();

                }else{
                    GameObject normalGun = Instantiate(gun, aimPivot);
                    if (primaryGun == null){
                        primaryGun = normalGun.GetComponent<Gun>();
                    }else{
                        if (secondaryGun == null){
                            secondaryGun = normalGun.GetComponent<Gun>();
                        }else{
                            DropEquippedGun();
                            if (equippedGun == primaryGun){
                                primaryGun = normalGun.GetComponent<Gun>();
                                equippedGun = primaryGun;
                            }else{
                                secondaryGun = normalGun.GetComponent<Gun>();
                                equippedGun = secondaryGun;
                            }
                        }
                    }
                    equippedGun = normalGun.GetComponent<Gun>();

                }
            }else if(gun.GetComponent<Gun>() is GrappleGun){
                GameObject grappleGun = Instantiate(this.grappleGun, aimPivot);

                if (primaryGun == null){
                    primaryGun = grappleGun.GetComponent<Gun>();
                }else{
                    if (secondaryGun == null){
                        secondaryGun = grappleGun.GetComponent<Gun>();
                    }else{
                        DropEquippedGun();
                        if (equippedGun == primaryGun){
                            primaryGun = grappleGun.GetComponent<Gun>();
                            equippedGun = primaryGun;
                        }else{
                            secondaryGun = grappleGun.GetComponent<Gun>();
                            equippedGun = secondaryGun;
                        }
                    }
                }
                equippedGun = grappleGun.GetComponent<Gun>();
            }else if(gun.GetComponent<Gun>() is VacuumGun){
                GameObject vacuumGun = Instantiate(this.vacuumGun, aimPivot);

                if (primaryGun == null){
                    primaryGun = vacuumGun.GetComponent<Gun>();
                }else{
                    if (secondaryGun == null){
                        secondaryGun = vacuumGun.GetComponent<Gun>();
                    }else{
                        DropEquippedGun();
                        if (equippedGun == primaryGun){
                            primaryGun = vacuumGun.GetComponent<Gun>();
                            equippedGun = primaryGun;
                        }else{
                            secondaryGun = vacuumGun.GetComponent<Gun>();
                            equippedGun = secondaryGun;
                        }
                    }
                }
                equippedGun = vacuumGun.GetComponent<Gun>();
            }
        }

    
        private bool CanMove(Vector2 direction){
            if (direction != Vector2.zero){
                int raycastHits = rb.Cast(direction, contactFilter, raycastHit2Ds, moveSpeed * Time.fixedDeltaTime + collisionOffset);
                if(raycastHits == 0){
                    return true;
                }
                return false;
            }
            return false;
        }

        private IEnumerator ZoomOut(float maxZoom){
            float currentZoom = cam.m_Lens.OrthographicSize;

            for (float i = 0; i <= 1; i+=0.05f){
                cam.m_Lens.OrthographicSize = Mathf.Lerp(currentZoom, maxZoom, i);
                yield return new WaitForSeconds(.01f);
            }
        }

        private IEnumerator ZoomIn(){
            float currentZoom = cam.m_Lens.OrthographicSize;

            for (float i = 0; i <= 1; i+=0.05f){
                cam.m_Lens.OrthographicSize = Mathf.Lerp(currentZoom, DEFAULTZOOM, i);
                yield return new WaitForSeconds(.01f);
            }
        }



    }



