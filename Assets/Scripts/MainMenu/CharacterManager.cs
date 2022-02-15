using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class CharacterManager : MonoBehaviour
{
    public int numberOfUsers;

    public List<PlayerBase> players = new List<PlayerBase>();

    public List<CharacterBase> characterList = new List<CharacterBase>();

    public CharacterBase FindCharacterWithID(string id)
    {
        CharacterBase retVal = null;

        for (int i = 0; i < characterList.Count; i++)
        {
            if (string.Equals(characterList[i].charId, id))
            {
                retVal = characterList[i];
                break;
            }
        }
        return retVal;
    }


    public PlayerBase FindPlayerFromStates(StateManager states)
    {
        PlayerBase retVal = null;

        for (int i = 0; i < characterList.Count; i++)
        {
            if (players[i].playerStates == states)
            {
                retVal = players[i];
                break;
            }
        }
        return retVal;
    }

    private static CharacterManager instance;

    public static CharacterManager Instance
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

}


[System.Serializable]
public class CharacterBase
{
    public string charId;
    public GameObject prefab;
}

[System.Serializable]
public class PlayerBase
{
    public string playerId;
    public string inputId;
    public PlayerType playerType;
    public bool hasCharacter;
    public GameObject playerPrefab;
    public StateManager playerStates;
    public int score;

    public enum PlayerType
    {
        user,   // it's a real human
        AI,     // it's computer
        simulation   // for multiplayer over network
    }
}
