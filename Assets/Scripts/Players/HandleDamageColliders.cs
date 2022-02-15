using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleDamageColliders : MonoBehaviour
{
    public GameObject[] damageCollidersLeft;
    public GameObject[] damageCollidersRight;

    public enum DamageType
    {
        light,
        heavy
    }

    public enum DCtype
    {
        bottom,
        top
    }

    StateManager stateManager;

    void Start()
    {
        stateManager = GetComponent<StateManager>();
        CloseColliders();
    }

    public void OpenCollider(DCtype type, float delay, DamageType damageType)
    {
        if (!stateManager.lookRight)
        {
            switch (type)
            {
                case DCtype.bottom:
                    StartCoroutine(OpenCollider(damageCollidersLeft, 0, delay, damageType));   
                    break;
                case DCtype.top:
                    StartCoroutine(OpenCollider(damageCollidersLeft, 1, delay, damageType));
                    break;
            }
        }
        else
        {
            switch (type)
            {
                case DCtype.bottom:
                    StartCoroutine(OpenCollider(damageCollidersRight, 0, delay, damageType));
                    break;
                case DCtype.top:
                    StartCoroutine(OpenCollider(damageCollidersRight, 1, delay, damageType));
                    break;
            }
        }
    }


    // array is damageColliders * DamageColliderLeft, DamageColliderRight
    // index 0: bottom, 1: top
    IEnumerator OpenCollider(GameObject[] array, int index, float delay, DamageType damageType)
    {
        yield return new WaitForSeconds(delay);
        array[index].SetActive(true);
        array[index].GetComponent<DoDamage>().damageType = damageType;

    }

    public void CloseColliders()
    {
        for (int i = 0; i < damageCollidersLeft.Length; i++)
        {
            damageCollidersLeft[i].SetActive(false);
            damageCollidersRight[i].SetActive(false);
        }
    }


}
