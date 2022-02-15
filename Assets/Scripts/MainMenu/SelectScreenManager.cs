using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SelectScreenManager : MonoBehaviour
{
    public int numberOfPlayers = 1;
    public List<PlayerInterfaces> playerInterfaces = new List<PlayerInterfaces>();
    public PotraitInfo[] potraitPrefabs;  // all out entries as potraits
    public int maxX;
    public int maxY;
    PotraitInfo[,] charGrid;   // the grid we are creating to selecting entries

    public GameObject potraitCanvas; // the canvas to hold all the potraits

    bool isLevelLoading;
    public bool bothPlayersSelected;

    CharacterManager characterManager;

    #region Singleton

    private static SelectScreenManager instance;
    public static SelectScreenManager Instance
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

    #endregion


    // Start is called before the first frame update
    void Start()
    {
        characterManager = CharacterManager.Instance;
        numberOfPlayers = characterManager.numberOfUsers;

        charGrid = new PotraitInfo[maxX, maxY];

        int x = 0;
        int y = 0;

        potraitPrefabs = potraitCanvas.GetComponentsInChildren<PotraitInfo>();

        // lets go into all our potraits
        for(int i=0; i<potraitPrefabs.Length; i++)
        {
            potraitPrefabs[i].posX += x;
            potraitPrefabs[i].posX += y;

            charGrid[x, y] = potraitPrefabs[i];

            if(x<maxX - 1)
            {
                x++;
            }
            else
            {
                x = 0;
                y++;
            }
        }
    }

    void Update()
    {
        if (!isLevelLoading)
        {
            // we will go through all the players player 1 and player 2
            for(int i=0; i<playerInterfaces.Count; i++)
            {

                if(i < numberOfPlayers)
                {
                    if (Input.GetButtonUp("Fire2" + characterManager.players[i].inputId) )
                    {
                        playerInterfaces[i].playerBase.hasCharacter = false;
                    }

                    if (!characterManager.players[i].hasCharacter)
                    {
                        playerInterfaces[i].playerBase = characterManager.players[i];

                        HandleSelectorPosition(playerInterfaces[i]);
                        HandleSelectorScreenInput(playerInterfaces[i], characterManager.players[i].inputId);
                        HandleCharacterPreview(playerInterfaces[i]);
                    }
                }
                else
                {
                    // this is for AI player
                    characterManager.players[i].hasCharacter = true;
                }
            }
        }

        if (bothPlayersSelected)
        {
            Debug.Log("Loading Fight Level");
            StartCoroutine("LoadLevel");
            isLevelLoading = true;
        }
        else
        {
            if (characterManager.players[0].hasCharacter && characterManager.players[1].hasCharacter)
            {
                bothPlayersSelected = true;
            }
        }
    }

    private void HandleCharacterPreview(PlayerInterfaces pl)
    {
        if(pl.previewPotrait != pl.activePotrait)
        {
            if(pl.createdCharacter != null)
            {
                Destroy(pl.createdCharacter);
            }

            // we destroyed the created character
            // create another one
            GameObject go = Instantiate(
                CharacterManager.Instance.FindCharacterWithID(pl.activePotrait.characterId).prefab,
                pl.charVisPos.position,
                Quaternion.identity) as GameObject;

            pl.createdCharacter = go;
            pl.previewPotrait = pl.activePotrait;

            if(!string.Equals(pl.playerBase.playerId, characterManager.players[0].playerId))
            {
                pl.createdCharacter.GetComponent<StateManager>().lookRight = false;
            }
        }
    }

    private void HandleSelectorScreenInput(PlayerInterfaces pl, string playerId)
    {
        #region Grid Navigation

        /* To navigate the grid
         * we simply change the active x and y to select what entry is active
         * we also smooth out the input so if the user keeps pressing the button
         * it won't swith more then once over half 
         */

        float vertical = Input.GetAxis("Vertical" + playerId);

        if (vertical != 0)
        {
            if (!pl.hitInputOnce)
            {
                if (vertical > 0)
                {
                    pl.activeY = (pl.activeY > 0) ? pl.activeY - 1 : maxY - 1;
                }
                else
                {
                    pl.activeY = (pl.activeY < maxY - 1) ? pl.activeY + 1 : 0;
                }
                pl.timerToReset = 0;
                pl.hitInputOnce = true;
            }
        }

        float horizontal = Input.GetAxis("Horizontal" + playerId);

        if (horizontal != 0)
        {
            if (!pl.hitInputOnce)
            {
                if (horizontal > 0)
                {
                    pl.activeX = (pl.activeX > 0) ? pl.activeX - 1 : maxX - 1;
                }
                else
                {
                    pl.activeX = (pl.activeX < maxX - 1) ? pl.activeX + 1 : 0;
                }
                pl.timerToReset = 0;
                pl.hitInputOnce = true;
            }
        }

        if (vertical == 0 && horizontal == 0)
        {
            pl.hitInputOnce = false;
        }

        if (pl.hitInputOnce)
        {
            pl.timerToReset += Time.deltaTime;

            if (pl.hitInputOnce)
            {
                pl.timerToReset += Time.deltaTime;

                if (pl.timerToReset > 0.8f)
                {
                    pl.hitInputOnce = false;
                    pl.timerToReset = 0;
                }
            }
        }

        #endregion

        // if the user presses spaces, he has selected a character
        if (Input.GetButtonUp("Fire1" + playerId) || Input.GetKeyUp(KeyCode.Space))
        {
            //Player does a selection reaction animation
            pl.createdCharacter.GetComponentInChildren<Animator>().Play("Kick");

            // pass the character to the character manager so that we know what character to create in the game scene
            pl.playerBase.playerPrefab = characterManager.FindCharacterWithID(pl.activePotrait.characterId).prefab;

            pl.playerBase.hasCharacter = true;
        }

        
    }
    private void HandleSelectorPosition(PlayerInterfaces pl)
    {
        pl.selector.SetActive(true);

        pl.activePotrait = charGrid[pl.activeX, pl.activeY];  // find the active potrait

        // and place the selector over it's position
        Vector2 selectorPosition = pl.activePotrait.transform.localPosition;
        selectorPosition = selectorPosition + new Vector2(potraitCanvas.transform.localPosition.x,
                                                          potraitCanvas.transform.localPosition.y);

        pl.selector.transform.localPosition = selectorPosition;
    }

    IEnumerator LoadLevel()
    {
        // if any of the players is AI, then assign a random character to the prefab
        for(int i=0; i<characterManager.players.Count; i++)
        {
            if(characterManager.players[i].playerType == PlayerBase.PlayerType.AI)
            {
                if(characterManager.players[i].playerPrefab == null)
                {
                    int ranValue = Random.Range(0, potraitPrefabs.Length);

                    characterManager.players[i].playerPrefab =
                        characterManager.FindCharacterWithID(potraitPrefabs[ranValue].characterId).prefab;
                    Debug.Log(potraitPrefabs[ranValue].characterId);
                }
            }
            characterManager.players[i].playerStates = characterManager.players[i].playerPrefab.GetComponent<StateManager>();

        }

        yield return new WaitForSeconds(2);  // load the level after 2 seconds
        SceneManager.LoadSceneAsync("level", LoadSceneMode.Single);
    }
}


[System.Serializable]
public class PlayerInterfaces
{
    public PotraitInfo activePotrait;
    public PotraitInfo previewPotrait;
    public GameObject selector; // selectIndicator of player 1
    public Transform charVisPos;  // visualization position of player 1
    public GameObject createdCharacter;  // created character for visualization of player 1

    public int activeX;
    public int activeY;

    // variable for smoothing out input
    public bool hitInputOnce;
    public float timerToReset;

    public PlayerBase playerBase;
}