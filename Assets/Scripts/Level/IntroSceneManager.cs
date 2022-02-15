using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroSceneManager : MonoBehaviour
{
    public GameObject startText;
    float timer;
    bool loadingLevel;
    bool initialized;

    public int activeElementIndex;
    public GameObject menuObj;  // number of player selection menu
    public ButtonRef[] menuOptions;

    void Start()
    {
        menuObj.SetActive(false);
    }

    void Update()
    {
        // this is the screen where the "Press start button" blinks
        if (!initialized)
        {
            timer += Time.deltaTime;
            if (timer > 0.6f)
            {
                timer = 0;
                startText.SetActive(!startText.active);
            }

            if(Input.GetKeyUp(KeyCode.Space) || Input.GetButtonUp("Jump"))
            {
                initialized = true;
                startText.SetActive(false);
                menuObj.SetActive(true);   // Closes the text and opens the Player choose Menu
            }
        }
        else
        {
            // This is the selection screen where user choose if
            // single player or 2 player
            if (!loadingLevel)
            {

                // indicate which option is selected  * Single Player * 2 player
                menuOptions[activeElementIndex].Selected = true;

                if (Input.GetKeyUp(KeyCode.UpArrow))
                {
                    menuOptions[activeElementIndex].Selected = false;

                    if (activeElementIndex > 0)
                        activeElementIndex--;
                    else
                    {
                        // if the top element is selected and up pressed
                        //the selection will go to the bottom element
                        activeElementIndex = menuOptions.Length - 1;
                    }
                }

                if (Input.GetKeyUp(KeyCode.DownArrow))
                {
                    menuOptions[activeElementIndex].Selected = false;

                    if (activeElementIndex < menuOptions.Length - 1)
                        activeElementIndex++;
                    else
                    {
                        // if the bottom element is selected and down pressed
                        //the selection will go to the top element
                        activeElementIndex = 0;
                    }
                }

                if(Input.GetKeyUp(KeyCode.Space) || Input.GetButtonUp("Jump"))
                {
                    Debug.Log("Load Character Selection Screen");
                    loadingLevel = true;
                    StartCoroutine("LoadLevel");
                    menuOptions[activeElementIndex].transform.localScale *= 1.2f;
                }
            }            
        }
    }

    private void HandleSelection()
    {
        switch (activeElementIndex)
        {
            case 0:
                CharacterManager.Instance.numberOfUsers = 1;
                break;
            case 1:
                CharacterManager.Instance.numberOfUsers = 2;
                CharacterManager.Instance.players[1].playerType = PlayerBase.PlayerType.user;
                break;
        }
    }

    IEnumerator LoadLevel()
    {
        HandleSelection();
        yield return new WaitForSeconds(0.6f);
        SceneManager.LoadSceneAsync("select", LoadSceneMode.Single);
    }
}
