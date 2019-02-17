using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//for controlling the next and back buttons on the controls scene

public class ControlsPage : MonoBehaviour {

    public GameObject[] Panels;
    int Index = 0;

    /// <summary>
    /// Closes current panel, opens next one in array
    /// </summary>
    public void NextPanel()
    {
        Panels[Index].SetActive(false);
        Index++;
        Panels[Index].SetActive(true);
    }

    /// <summary>
    /// Closes current panel, opens previous one in array
    /// </summary>
    public void PreviousPanel()
    {
        Panels[Index].SetActive(false);
        Index--;
        Panels[Index].SetActive(true);
    }
}
