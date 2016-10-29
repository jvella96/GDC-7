﻿using UnityEngine;
using System.Collections;

/*
 * Enemy AI class
 * This class targets the player only if the player is on the platform that user is on
 * Otherwise if the enemy is not on the platform the enemy will move from one bound to another
 * 
 */
public class FlyingEnemyAI : MonoBehaviour {
    public Transform target;
    public LayerMask playerMask;

    public Transform topRightBound;
    public Transform topLeftBound;
    public Transform bottomRightBound;
    public Transform bottomLeftBound;

    public GameObject fireball; //prefab to fireball
    public Vector2 fireballVector;
    public GameObject emmiterFireball; //actual emmiter

    public Vector2[] possiblePositions;
    
    private Rigidbody2D rigidBody;
    private Animator animator;

    public float movementSpeed = 0.8f;
    public float projectileRange;
    public float dashRange;
    public float enemyStopDistance;
    public float changeDirectionInSeconds = 2f;
    public float precisionOfEnemyToBounds = 0.26f;

    public float fireRate;

    private float distanceToPlayer;
    private bool searchingPlayer = false;
    private bool isChasing;
    private bool isAttacking;

    private float time;

    private float nextFire;
    private Vector2 previousPosition;
    private Vector2 currentPosition;
    private Vector2 tempPosition;

    void Start() {
        animator = this.GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody2D>();
        target = GameObject.FindWithTag("Player").transform;
        time = 0;
        currentPosition = possiblePositions[0];
    }

    void Update() {
        distanceToPlayer = Vector2.Distance(target.position, transform.position);
        animatorSetting();
        if (SearchForPlayer()) { //player is in the ghost bounds
            /*
            if (distanceToPlayer > projectileRange) {
                //out of range
                //roam randomly in box
                isChasing = false;
                isAttacking = false;
                randomMovement();
            } else if ((distanceToPlayer <= projectileRange) && (distanceToPlayer > dashRange)) {
                //look at the player
                //shoot projectiles
                isChasing = false;
                isAttacking = false;
                //dont move and shoot projectile
                //shootProjectile();
            } else if ((distanceToPlayer <= dashRange) && (distanceToPlayer > enemyStopDistance)) {
                //start running after the enemy
                //and dash into him
                isChasing = true;
                isAttacking = false;
                dashToPlayer(target);
            } else if (distanceToPlayer <= enemyStopDistance) {
                isChasing = false;
                isAttacking = true;
                dashToPlayer(target);
            } else {
                Debug.Log(("Undefined State"));
            }
            */
        } else {
            isChasing = false;
            isAttacking = false;
            //patrol and throw bolts
            if (distanceToPlayer > projectileRange) {
                //keep roaming
                //Debug.Log("Moving to point: " + currentPosition.x+", "+currentPosition.y);
                //randomMovement();
                //movement();
            } else if ((distanceToPlayer <= projectileRange) && (distanceToPlayer > dashRange)) {
                //shoot projectile
                //shootProjectile();
            }
        }
    }

    public bool SearchForPlayer() {
        if ((Physics2D.Linecast(transform.position, topLeftBound.position, playerMask)) || (Physics2D.Linecast(transform.position, topRightBound.position, playerMask)) ||
            (Physics2D.Linecast(transform.position, bottomLeftBound.position, playerMask)) || (Physics2D.Linecast(transform.position, bottomRightBound.position, playerMask)) ||
            (Physics2D.Linecast(topRightBound.position, topLeftBound.position, playerMask)) || (Physics2D.Linecast(bottomRightBound.position, topRightBound.position, playerMask)) ||
            (Physics2D.Linecast(bottomRightBound.position, bottomLeftBound.position, playerMask)) || (Physics2D.Linecast(bottomLeftBound.position, topLeftBound.position, playerMask))) { //Bound of platform check
            return true;
        } else {
            return false;
        }
    }


    //new : remove random, use predefined path, and interrpurt in 1 sec and go to next point from current position

    //need to check that it is not out of bounds, if it is out of bounds send back to box
    //need to check if it stayed in the same position for a certain period of time
    public void randomMovement() {
        time += Time.deltaTime;
        var bound = outOfBounds();
        if (bound == 4) {
            if (time > changeDirectionInSeconds) {//every second possibly change direction
                time = 0;
                previousPosition = currentPosition;
                do {
                    currentPosition = nextPoint();
                } while (currentPosition == previousPosition);
            } else {
                moveEnemy(currentPosition);
            }
        }else if(bound == 0) {//go the bottom bound
            moveEnemy(bottomRightBound.position);
        } else if(bound == 1) {//go to the top bound
            moveEnemy(topRightBound.position);
        } else if(bound == 2) {//go to the right bound
            moveEnemy(topRightBound.position);
        } else if(bound == 3) {//go to the left bound
            moveEnemy(topLeftBound.position);
        }
    }

    public void movement() {
        if((transform.position.x == currentPosition.x)&&((transform.position.y == currentPosition.y))) {       
            if(currentPosition == possiblePositions[0]) {
                currentPosition = possiblePositions[1];
            }else if(currentPosition == possiblePositions[1]) {
                currentPosition = possiblePositions[2];
            } else if (currentPosition == possiblePositions[2]) {
                currentPosition = possiblePositions[3];
            } else if (currentPosition == possiblePositions[3]) {
                currentPosition = possiblePositions[0];
            }
        }else {
            moveEnemy(currentPosition);
        }
    }


    public Vector2 nextPoint() {
        return possiblePositions[Random.Range(0, possiblePositions.Length)];
    }

    public int outOfBounds() {
        var top = topLeftBound.position.y;
        var bottom = bottomRightBound.position.y;
        var left = topLeftBound.position.x;
        var right = topRightBound.position.x;

        if (transform.position.y >= top ) {//out from the top bound
            return 0;
        }else if(transform.position.y <= bottom) {//out from the bottom bound
            return 1;
        }else if (transform.position.x >= right) {//out from the right bound
            return 2;
        }else if (transform.position.x <= left) {//out from the left bound
            return 3;
        }else {//in bounds
            return 4;
        }
    }

    public void shootProjectile() {
        if (Time.time > nextFire) {
            nextFire = Time.time + fireRate;

            fireball.GetComponent<Projectile>().velocityVector = fireballVector + emmiterFireball.GetComponent<EmmiterVector>().directionVector; //speed + dir
            Instantiate(fireball, emmiterFireball.GetComponent<Transform>().position, emmiterFireball.GetComponent<Transform>().rotation);

            //Shoot = Instantiate(fireball, transform.position, transform.rotation);
            //Shoot.AddForce(transform.forward * 5000);

            //clone = Instantiate(projectile, transform.position, transform.rotation);
            //clone.velocity = transform.TransformDirection(Vector3(0, 0, speed));
        }
    }

    public void dashToPlayer(Transform objective) {
        float step = movementSpeed * Time.deltaTime;
        transform.position = Vector2.MoveTowards(transform.position, objective.position, step);
    }

    public void rotateGhost() {
        var lookPos = target.position - transform.position;
        var rotation = Quaternion.LookRotation(lookPos);
        rotation.y = 0;
        rotation.x = 0;
        float angle = rotation.eulerAngles.z;
        if ((angle > 35)&&(angle < 90)) {
            rotation.z = 0.2025f;// change 30 to quaternion
        } else if ((angle < 325)&&(angle > 270)) {
            rotation.z = -0.2025f;// change -30 to quaternion
        }
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 1);
    }

    public void animatorSetting() {
        var relativePoint = transform.InverseTransformPoint(target.position);
        rotateGhost();
        if (isChasing) {//chasing enemy
            if (relativePoint.x < 0.0) {//chasing to the left
                animator.SetInteger("Gdirection", 2);
            } else {//chaing to the right
                animator.SetInteger("Gdirection", 1);
            }
        } else {//looking at enemy // soon to attack
            if (isAttacking) {//attacking 
                animator.SetBool("IsAttack", true);
            } else {//looking at player
                animator.SetBool("IsAttack", false);
                if (relativePoint.x > 0.0) {//looking at the right
                    animator.SetInteger("Gdirection", 0);
                } else {//looking at the left
                    animator.SetInteger("Gdirection", 3);
                }
            }
        }
    }

    public void moveEnemy(Vector2 objective) {
        float step = movementSpeed * Time.deltaTime;
        transform.position = Vector2.MoveTowards(transform.position, objective, step);
    }
    
    /*
    public bool inAttackRange() {
        if(distanceToPlayer < rangeToAttackProjectile ) {
            //can start throwing projectile attacks
            return true;
        }else {
            //keep patrolling
            //bouncing between bounds
            return false;
        }
    }
    */

    public bool checkBoundDistance(Transform bound) {
        if ((Vector2.Distance(bound.position, transform.position)) <= precisionOfEnemyToBounds) {
            return true;
        } else {
            return false;
        }
    }

    public void attack() {
        isAttacking = true;
        //deal damage to player
        isAttacking = false;
    }

}