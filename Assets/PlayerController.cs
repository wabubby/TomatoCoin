using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;


public class PlayerController : MonoBehaviour
{   
    //Movement
    public float moveSpeed;
    private Vector2 movementDirection;
    public ContactFilter2D contactFilter;
    private float collisionOffset = 0.04f;

    public static Vector3 characterPos; //used by enemy and bullet class 

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

    
    private void Start(){
        moveSpeed = 5f;
        rb = GetComponent<Rigidbody2D>();
        raycastHit2Ds = new List<RaycastHit2D>(0);
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        // aimPivot = transform.GetChild(0).gameObject.transform;
        maxHealth = 500;
        health = maxHealth;
        activeCoroutine = null;
    }

    private void Update(){    
        //for debugging
        if (Input.GetKeyDown(KeyCode.Space)){
            Debug.Break();
        }
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
    
        //movement
        movementDirection.x = Input.GetAxisRaw("Horizontal");
        movementDirection.y = Input.GetAxisRaw("Vertical");
        characterPos = transform.position; 
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



