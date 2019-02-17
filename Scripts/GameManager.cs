using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    public GameObject DeathScreen;
    public GameObject WinScreen;
    public bool alive = true; //the player begins the game alive, obviously

	void Start ()
    {
        if (DeathScreen != null)
            DeathScreen.SetActive(false);
        if (WinScreen != null)
            WinScreen.SetActive(false);
	}

    /// <summary>
    /// Called when the player dies
    /// </summary>
    public void OnDeath()
    {
        alive = false;
        DeathScreen.SetActive(true);
    }

    /// <summary>
    /// called on completion of the demo
    /// </summary>
    public void OnWin()
    {
        WinScreen.SetActive(true);
    }

    /// <summary>
    /// opens the main menu scene
    /// </summary>
    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }

    /// <summary>
    /// opens the main scene (where the game takes place)
    /// </summary>
    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    /// <summary>
    /// opens the credits page scene
    /// </summary>
    public void Credits()
    {
        SceneManager.LoadScene(2);
    }

    /// <summary>
    /// opens the controls page scene
    /// </summary>
    public void Controls()
    {
        SceneManager.LoadScene(3);
    }
}
