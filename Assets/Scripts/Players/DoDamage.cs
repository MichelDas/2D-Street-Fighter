using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoDamage : MonoBehaviour
{

    StateManager states;

    public HandleDamageColliders.DamageType damageType;

    void Start()
    {
        states = GetComponentInParent<StateManager>();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.GetComponentInParent<StateManager>())
        {
            StateManager otherState = col.GetComponentInParent<StateManager>();

            if(otherState != states)
            {
                otherState.TakeDamage(30, damageType);
            }
        }
    }
}
