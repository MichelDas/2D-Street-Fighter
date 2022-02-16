using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICharacter : MonoBehaviour
{
    #region Variables

    StateManager states;
    public StateManager enemyStates;

    public float changeStateToleranceDistance = 3; // How close is considered close combat

    public float normalRate = 1;
    float nrmTimer;

    public float closeRate = 0.5f;
    float clTimer;

    public float blockRate = 1.5f;
    float blTimer;

    public float aiStateLife = 1;
    float aiTimer;

    bool initiateAI;  // when it has an AI state to run
    bool closeCombat; // if we are in close combat

    bool gotRandom;  // Helps when we are not updating our random variable every frame
    float storeRandom;

    // blocking variables
    bool checkForBlocking;
    bool isBlocking;
    float blockMultiplier;

    // attack variables
    bool randomizeAttacks;
    int maxNumberOfAttacks;
    int curNumAttacks;

    // Jump variables
    public float JumpRate = 1;
    float jRate;
    bool jump;
    float jtimer;
     
    #endregion

    public AttackPatterns[] attackPatterns;

    public enum AIState
    {
        closeState,
        normalState,
        resetAI
    }

    public AIState aIState;

    void Start()
    {
        states = GetComponent<StateManager>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckDistance();
        States();
        AIAgent();
    }

    

    private void States()
    {
        switch (aIState)
        {
            case AIState.closeState:
                CloseState();
                break;
            case AIState.normalState:
                NormalState();
                break;
            case AIState.resetAI:
                ResetAI();
                break;
        }

        // These functions are called independent of the AIState
        //Blocking();
        Jumping();
    }

    // This function manager what our agent does
    private void AIAgent()
    {
        // if our AI has done a full cycle
        if (initiateAI)
        {
            // reset our AI first
            aIState = AIState.resetAI;

            //create a multiplier
            float multiplier = 0;

            // get a random value
            if (!gotRandom)
            {
                storeRandom = ReturnRandom();
                gotRandom = true;
            }

            // if we are not in close combat
            // here we can implement some power throwing code
            if (!closeCombat)
            {
                // we have 30% more chances of moving
                multiplier += 30;
            }
            else
            {
                // if we are in close combat
                // we have 30% more chances to attack
                multiplier -= 30;
            }

            // if the sum of the random value and multiplier
            // is less then 50, attack
            if(storeRandom + multiplier < 50)
            {
                Attack();
            }
            else
            {
                // here we can put some logic about what the
                // AI will do when he is far from player
                // * he can power up, Taunt, throw enervy ball, move etc.
                Movement();
            }
        }
        
    }

    private void Movement()
    {
        // take a random value
        if (!gotRandom)
        {
            storeRandom = ReturnRandom();
            gotRandom = true;
        }

        // there is a 90% chance of moving close to the enemy
        if(storeRandom < 90)
        {
            if (enemyStates.transform.position.x < transform.position.x)
            {
                states.horizontal = -1;
            }
            else
                states.horizontal = 1;
        }
        else
        {
            // move aways from the enemy
            if (enemyStates.transform.position.x < transform.position.x)
            {
                states.horizontal = -1;
            }
            else
                states.horizontal = 1;
        }

        // Note: we can use some other logic to manipulate how
        // state.horizontal will be modified based of
        // health or difficulty level
    }

    // here we will reset all variables
    private void ResetAI()
    {
        aiTimer += Time.deltaTime;

        if(aiTimer > aiStateLife)
        {
            initiateAI = false;
            states.horizontal = 0;
            states.vertical = 0;
            aiTimer = 0;

            gotRandom = false;
        }

        // there is also a 50% chance of going
        //to a close state instead of normal state
        storeRandom = ReturnRandom();
        if (storeRandom < 50)
            aIState = AIState.normalState;
        else
            aIState = AIState.closeState;

        curNumAttacks = 1;
        randomizeAttacks = false;
    }

    private void NormalState()
    {
        nrmTimer += Time.deltaTime;

        if(nrmTimer > normalRate)
        {
            initiateAI = true;
            nrmTimer = 0;
        }
    }

    private void CloseState()
    {
        clTimer += Time.deltaTime;

        if(clTimer > closeRate)
        {
            clTimer = 0;
            initiateAI = true; // meaning a full cycle is completed
        }

        // As we completed a full cycle by saying initiateAI = true
        // we will go to AIAgent and Reset AI from there
    }

    private void CheckDistance()
    {
        // take the distance
        float distance = Vector3.Distance(transform.position, enemyStates.transform.position);

        // compare the distance with our tolerance
        if(distance < changeStateToleranceDistance)
        {
            // if we are not resetingAI, then close AI state
            if (aIState != AIState.resetAI)
                aIState = AIState.closeState;

            closeCombat = true;
        }
        else
        {
            if (aIState != AIState.resetAI)
                aIState = AIState.normalState;

            // if we are close to enemy, we will move away
            if (closeCombat)
            {
                if (closeCombat)
                {
                    if (!gotRandom)
                    {
                        storeRandom = ReturnRandom();
                        gotRandom = true;
                    }

                    // still a 60% chance that AI will follow the enemy instead of moving aways
                    if(storeRandom < 60)
                    {
                        Movement();
                    }
                }

                // probably we are not in close combat anymore
                closeCombat = false;
            }
        }
    }

    private void Attack()
    {
        // take a random value
        if (!gotRandom)
        {
            storeRandom = ReturnRandom();
            gotRandom = true;
        }

        // There is 75% chances of doing a normal attack
        if(storeRandom < 75)
        {
            // see how many attacks he will do
            if (!randomizeAttacks)
            {
                // getting a random value between 1-4
                maxNumberOfAttacks = (int)UnityEngine.Random.Range(1, 4);
                randomizeAttacks = true;
            }

            // if we haven't attacked more then the maximum times
            if(curNumAttacks < maxNumberOfAttacks)
            {
                // lets randomly decide which attacks we want to do
                int attackNumber = UnityEngine.Random.Range(0, attackPatterns.Length);

                StartCoroutine(OpenAttack(attackPatterns[attackNumber], 0));

                // increment how many attacks has been done
                curNumAttacks++;
            }

            
        }
        else   // Special Attack, 25% chance
        {
            if(curNumAttacks < 1) // special attack will happen only once
            {
                states.SpecialAttack = true;
                curNumAttacks++;
            }
        }
    }

    IEnumerator OpenAttack(AttackPatterns a, int i)
    {
        int index = i;
        float delay = a.attacks[index].delay;
        states.attack1 = a.attacks[index].attack1;
        states.attack2 = a.attacks[index].attack2;

        yield return new WaitForSeconds(delay);

        states.attack1 = false;
        states.attack2 = false;

        if(index < a.attacks.Length - 1)
        {
            index++;
            StartCoroutine(OpenAttack(a, index));
        }
    }

    private void Blocking()
    {
        // if we are about to recieve damage
        if (states.gettingHit)
        {
            // take a random value
            if (!gotRandom)
            {
                storeRandom = ReturnRandom();
                gotRandom = true;
            }

            // There is a 50% chance of blocking
            if(storeRandom < 50)
            {
                isBlocking = true;
                states.gettingHit = false;
            }
        }

        // when blocking we need to count how long we block
        if (isBlocking)
        {
            blTimer += Time.deltaTime;

            if(blTimer > blockRate)
            {
                blTimer = 0;
                isBlocking = false;
            }
        }
    }

    private void Jumping()
    {
        // if the enemy jumps or we want to jump

        if (!enemyStates.onGround)
        {
            float ranValue = ReturnRandom();

            if(ranValue < 50)
            {
                jump = true;
            }
        }

        if(jump)
        {
            states.vertical = 1;
            jRate = ReturnRandom();
            jump = false;
        }
        else
        {
            // we need to reset vertical input in StateManager
            // or it will jump forever
            states.vertical = 0;
        }

        // the jump timer determines how many seconds he will need
        // to determine weather he wants to jump or not
        jtimer += Time.deltaTime;

        if(jtimer > JumpRate * 10)
        {

            // there is 50% chance of jumping or not
            if(jRate < 50)
            {
                jump = true;
            }
            else
            {
                jump = false;
            }
            jtimer = 0;
        }

    }

    private float ReturnRandom()
    {
        return UnityEngine.Random.Range(0, 101);
    }


    [System.Serializable]
    public class AttackPatterns
    {
        public AttackBase[] attacks;
    }

    [System.Serializable]
    public class AttackBase
    {
        public bool attack1;
        public bool attack2;
        public float delay;
    }
}
