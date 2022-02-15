using UnityEngine;
using System.Collections;

[System.Serializable]
public class ButtonRef : MonoBehaviour {

    public GameObject selectIndicator;

    private bool selected;

    public bool Selected
    {
        get => selected;

        set
        {
            selected = value;
            selectIndicator.SetActive(selected);
        }
    }

    void Start()
    {
        selectIndicator.SetActive(false);
    }
}
