using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{

    WaitForSeconds waitOneSec;   // as this script use this a lot, so creating a ref of it. this way is faster

    public Transform[] spawnPositions;

    [SerializeField] CharacterManager characterManager;
    LevelUI levelUI;

    public int maxRoundWin = 2;
    int currentRound = 1;

    // variables for countdown
    public bool countDown;
    public int maxRoundTime = 30;
    [SerializeField] int currentTimer;
    float internalTimer;

    private static LevelManager instance;

    public static LevelManager Instance
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        characterManager = CharacterManager.Instance;
        levelUI = LevelUI.Instance;

        waitOneSec = new WaitForSeconds(1);

        levelUI.AnnouncerTextLine1.gameObject.SetActive(false);
        levelUI.AnnouncerTextLine2.gameObject.SetActive(false);

        StartCoroutine("StartGame");

    }

    private void FixedUpdate()
    {
        // if player 1 is left to the player 2, look Right
        if(characterManager.players[0].playerStates.transform.position.x <
            characterManager.players[1].playerStates.transform.position.x)
        {
            characterManager.players[0].playerStates.lookRight = true;
            characterManager.players[1].playerStates.lookRight = false;
        }
        else
        {
            characterManager.players[0].playerStates.lookRight = false;
            characterManager.players[1].playerStates.lookRight = true;

        }
    }

    void Update()
    {
        if (countDown)
        {
            HandleTurnTimer();
        }
    }

    private void HandleTurnTimer()
    {
        levelUI.LevelTimer.text = currentTimer.ToString();

        internalTimer += Time.deltaTime;

        if(internalTimer > 1)
        {
            currentTimer--;
            internalTimer = 0;
        }

        if(currentTimer <= 0)   // if count down is over
        {
            EndTurnFunction(true);  // end round
            countDown = false;
        }
    }

    void DisableControl()
    {
        // to disable the control, we need to disable the component that make the player move
        for(int i=0; i<characterManager.players.Count; i++)
        {
            // first reset the variables in their state manager
            characterManager.players[i].playerStates.ResetStateInputs();


            // for user player disable input handler
            if(characterManager.players[i].playerType == PlayerBase.PlayerType.user)
            {
                characterManager.players[i].playerStates.GetComponent<InputHandler>().enabled = false;
            }

            // TODO add disable for AI
        }
    }

    IEnumerator StartGame()
    {
        // this will just spawn the players in the scene
        yield return CreatePlayers();

        // to start a new roung what we need is this function ( coroutine actually :D ) 
        yield return InitRound();
    }

    IEnumerator InitRound()
    {
        // disable announcers first
        levelUI.AnnouncerTextLine1.gameObject.SetActive(false);
        levelUI.AnnouncerTextLine2.gameObject.SetActive(false);

        currentTimer = maxRoundTime;
        countDown = false;

        // initialize the players
        yield return IntPlayers();

        // enable the controls
        yield return EnableControl();

    }

    IEnumerator IntPlayers()
    {
        // Here we only need to reset their health
        for (int i = 0; i < characterManager.players.Count; i++)
        {
            characterManager.players[i].playerStates.health = 100;
            characterManager.players[i].playerStates.handleAnimation.anim.Play("Locomotion");
            characterManager.players[i].playerStates.transform.position = spawnPositions[i].position;
        }

        yield return null;
    }

    IEnumerator EnableControl()
    {
        // start with the announcer text
        levelUI.AnnouncerTextLine1.gameObject.SetActive(true);
        levelUI.AnnouncerTextLine1.text = "Turn " + currentRound;
        levelUI.AnnouncerTextLine1.color = Color.white;

        yield return waitOneSec;
        yield return waitOneSec;

        levelUI.AnnouncerTextLine1.text = "3";
        levelUI.AnnouncerTextLine2.color = Color.green;
        yield return waitOneSec;

        levelUI.AnnouncerTextLine1.text = "2";
        levelUI.AnnouncerTextLine2.color = Color.yellow;
        yield return waitOneSec;

        levelUI.AnnouncerTextLine1.text = "1";
        levelUI.AnnouncerTextLine2.color = Color.red;
        yield return waitOneSec;

        levelUI.AnnouncerTextLine1.text = "FIGHT!";
        levelUI.AnnouncerTextLine2.color = Color.red;

        //for every player enable what they need to be controlled
        for(int i=0; i<characterManager.players.Count; i++)
        {
            if(characterManager.players[i].playerType == PlayerBase.PlayerType.user)
            {
                InputHandler inputHandler = characterManager.players[i].playerStates.gameObject.GetComponent<InputHandler>();
                inputHandler.playerInput = characterManager.players[i].inputId;
                inputHandler.enabled = true;
            }
        }

        //after one sec disable the announcer
        yield return waitOneSec;
        levelUI.AnnouncerTextLine1.gameObject.SetActive(false);
        countDown = true;
    }

    IEnumerator CreatePlayers()
    {
        // we will go into character manager and go through all players we have
        for(int i=0; i<characterManager.players.Count; i++)
        {

            // instantiate the players
            GameObject go = Instantiate(characterManager.players[i].playerPrefab,
                spawnPositions[i].position, Quaternion.identity) as GameObject;

            characterManager.players[i].playerStates = go.GetComponent<StateManager>();

            characterManager.players[i].playerStates.healthSlider = levelUI.healthSliders[i];
        }
        yield return null;
    }

    

    public void EndTurnFunction(bool timeOut = false)
    {

        /* we will use this function to end the round
         * we need to know if the round is ended by a timeout or knockout
         */
        countDown = false;

        //reset the timer text
        levelUI.LevelTimer.text = maxRoundTime.ToString();

        //if it's a timeout
        if (timeOut)
        {
            levelUI.AnnouncerTextLine1.gameObject.SetActive(true);
            levelUI.AnnouncerTextLine1.text = "Time Out!";
            levelUI.AnnouncerTextLine1.color = Color.cyan;
        }
        else
        {
            levelUI.AnnouncerTextLine1.gameObject.SetActive(true);
            levelUI.AnnouncerTextLine1.text = "K.O.";
            levelUI.AnnouncerTextLine1.color = Color.red;
        }

        DisableControl();

        StartCoroutine("EndTurn");
    }

    IEnumerator EndTurn()
    {
        yield return waitOneSec;
        yield return waitOneSec;
        yield return waitOneSec;

        // find which player won the round
        PlayerBase victorPlayer = FindWinningPlayer();

        // if it's a draw, victor player will be null
        if(victorPlayer == null)
        {
            // its a draw
            levelUI.AnnouncerTextLine1.text = "Draw";
            levelUI.AnnouncerTextLine1.color = Color.blue;
        }
        else
        {
            // some player has won the round
            levelUI.AnnouncerTextLine1.text = victorPlayer.playerId +"Wins!";
            levelUI.AnnouncerTextLine1.color = Color.red;
        }

        // let's wait for three seconds
        yield return waitOneSec;
        yield return waitOneSec;
        yield return waitOneSec;

        if(victorPlayer != null)
        {
            //  if it is a flawless victory
            if(victorPlayer.playerStates.health == 100)
            {
                levelUI.AnnouncerTextLine2.gameObject.SetActive(true);
                levelUI.AnnouncerTextLine2.text = "Flawless Victory!";
            }
        }

        // wait for 3 seconds
        yield return waitOneSec;
        yield return waitOneSec;
        yield return waitOneSec;

        currentRound++;

        bool matchOver = isMatchOver();

        if (!matchOver)
        {
            StartCoroutine(InitRound());   // start for the next round
        }
        else
        {
            for(int i=0; i< characterManager.players.Count; i++)
            {
                characterManager.players[i].score = 0;
                characterManager.players[i].hasCharacter = false;
            }

            // here, we are going to the character select screen
            // but we can also go to somewhere else like
            // a winner scene like in king of fighters.
            SceneManager.LoadSceneAsync("select");
        }



    }

    private bool isMatchOver()
    {
        bool retVal = false;
        for(int i=0; i<characterManager.players.Count; i++)
        {
            if(characterManager.players[i].score >= maxRoundWin)
            {
                retVal = true;
                break;
            }
        }

        return retVal;
    }

    private PlayerBase FindWinningPlayer()
    {
        PlayerBase retVal = null;

        StateManager targetPlayer = null;

        // first check if both players have equal health
        if (characterManager.players[0].playerStates.health != characterManager.players[1].playerStates.health)
        {
            if (characterManager.players[0].playerStates.health < characterManager.players[1].playerStates.health)
            {
                characterManager.players[1].score++;
                targetPlayer = characterManager.players[1].playerStates;
                levelUI.AddWinIndicator(1);
                retVal = characterManager.players[1];
            }
            else
            {
                characterManager.players[0].score++;
                targetPlayer = characterManager.players[0].playerStates;
                levelUI.AddWinIndicator(0);
                retVal = characterManager.players[0];
            }

            //retVal = characterManager.ReturnPlayerFromStates(targetPlayer);
        }
        else
        {
            return null;
        }

        return retVal;
    }
}
