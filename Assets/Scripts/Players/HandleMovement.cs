using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleMovement : MonoBehaviour
{
    Rigidbody2D rb;
    StateManager states;
    HandleAnimation handleAnimation;

    public float acceleration = 30;
    public float airAcceleration = 15;
    public float maxSpeed = 60;
    public float jumpSpeed = 5;
    public float jumpDuration = 5;

    float actualSpeed;
    bool justJumped;
    bool canVariableJump;
    float jmpTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        states = GetComponent<StateManager>();
        handleAnimation = GetComponent<HandleAnimation>();
        rb.freezeRotation = true;
    }

    private void FixedUpdate()
    {
        if (!states.dontMove)
        {
            HorizontalMovement();
            Jump();
        }
    }
    
    void HorizontalMovement()
    {
        actualSpeed = this.maxSpeed;

        if(states.onGround && !states.currentlyAttacking)
        {
            // ei line ta bujhi nai
            rb.AddForce(new Vector2((states.horizontal * actualSpeed) - rb.velocity.x * this.acceleration, 0));

        }

        // if the character is sliding
        // we have to stop it from sliding
        if(states.horizontal == 0 && states.onGround)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    private void Jump()
    {
        if(states.vertical > 0)
        {
            if (!justJumped)
            {
                justJumped = true;

                if (states.onGround)
                {
                    handleAnimation.JumpAnim();

                    rb.velocity = new Vector3(rb.velocity.x, this.jumpSpeed);
                    jmpTimer = 0;
                    canVariableJump = true;
                }
            }
            else
            {
                if (canVariableJump)
                {
                    jmpTimer += Time.deltaTime;

                    if(jmpTimer < this.jumpDuration / 1000)
                    {
                        rb.velocity = new Vector3(rb.velocity.x, this.jumpSpeed);
                    }
                }
            }
        }
        else
        {
            justJumped = false;
        }
    }

    // mair kheye jokhon ure jabe oitar jonno 
    public void AddVelocityOnCharacter(Vector3 direction, float timer)
    {
        // direction hocche upore konakoni
        //((!lookRight) ? Vector3.right * 1 : Vector3.right * -1) + Vector3.up, 

        StartCoroutine(AddVelocity(timer, direction));
    }

    IEnumerator AddVelocity(float timer, Vector3 direction)
    {
        float t = 0;

        while(t < timer)
        {
            t += Time.deltaTime;
            //rb.velocity = direction;
            rb.AddForce(direction * 2);
            yield return null;
        }
    }
}
