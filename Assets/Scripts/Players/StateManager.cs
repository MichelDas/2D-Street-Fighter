using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StateManager : MonoBehaviour
{

    public int health = 100;

    public float horizontal;
    public float vertical;
    public bool attack1;
    public bool attack2;
    public bool attack3;
    public bool crouch;

    public bool canAttack;
    public bool gettingHit;
    public bool currentlyAttacking;

    public bool dontMove;
    public bool onGround;
    public bool lookRight;

    public Slider healthSlider;
    SpriteRenderer sRenderer;

    [HideInInspector]
    public HandleDamageColliders handleDC;
    [HideInInspector]
    public HandleAnimation handleAnimation;
    [HideInInspector]
    public HandleMovement handleMovement;

    public GameObject[] movementColliders;

    public bool SpecialAttack { get; internal set; }


    // Start is called before the first frame update
    void Start()
    {
        handleDC = GetComponent<HandleDamageColliders>();
        handleAnimation = GetComponent<HandleAnimation>();
        handleMovement = GetComponent<HandleMovement>();
        sRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        sRenderer.flipX = lookRight;

        onGround = isOnGround();

        if(healthSlider != null)
        {
            healthSlider.value = (float)(health / 100.0f);
        }

        if(health <= 0)
        {
            if (LevelManager.Instance.countDown)
            {
                LevelManager.Instance.EndTurnFunction();
                handleAnimation.anim.Play("Dead"); 
            }
        }
    }

    private bool isOnGround()
    {
        bool retVal = false;

        LayerMask layer = ~(1 << gameObject.layer | 1 << 3);
        retVal = Physics2D.Raycast(transform.position, -Vector2.up, 0.1f, layer);

        return retVal;
    }

    public void ResetStateInputs()
    {
        horizontal = 0;
        vertical = 0;
        attack1 = false;
        attack2 = false;
        attack3 = false;
        crouch = false;
        gettingHit = false;
        currentlyAttacking = false;
        dontMove = false; // means can move
    }


    internal void CloseMovementCollider(int index)
    {
        movementColliders[index].SetActive(false);
    }

    public void OpenMovementCollider(int index)
    {
        movementColliders[index].SetActive(true);
    }

    public void TakeDamage(int damage, HandleDamageColliders.DamageType damageType)
    {
        if (!gettingHit)
        {
            switch (damageType)
            {
                case HandleDamageColliders.DamageType.light:
                    // will be able to get hit after 0.3 sec
                    StartCoroutine(CloseImmortality(0.3f)); 
                    break;
                case HandleDamageColliders.DamageType.heavy:
                    handleMovement.AddVelocityOnCharacter(
                        ((!lookRight) ? Vector3.right * 1 : Vector3.right * -1) + Vector3.up,
                        0.5f);

                    // will be able to get hit after 1 sec
                    StartCoroutine(CloseImmortality(1));
                    break;
            }

            health -= damage;
            gettingHit = true;
        }
    }

    IEnumerator CloseImmortality(float timer)
    {
        yield return new WaitForSeconds(timer);
        gettingHit = false;
    }
}
