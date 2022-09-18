using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomsControl : MonoBehaviour
{
    GameManager manager;

    Camera main;
    bool IsProgressing = true; //Is the player going forward or backward?
    public Transform[] CameraPosition; //locations where the camera can sit in the world
    static int CurrentPosition = 0; //camera's current location in the CameraPosition array
    GenericPlayer[] character; //array of character players

	void Start ()
    {
        main = FindObjectOfType<Camera>();
        manager = FindObjectOfType<GameManager>();
        character = FindObjectsOfType<GenericPlayer>();
        CurrentPosition = 0;
	}

    void OnTriggerExit(Collider other) //checks if a player goes through a trigger volume leading to a new room
    {
        if (other.CompareTag("Player") || other.CompareTag("Invisible"))
        {
            if (IsProgressing) //checks whether player is going forward or backward
            {
                IsProgressing = false;
                CurrentPosition++;
                CameraMove();
            }
            else
            {
                IsProgressing = true;
                CurrentPosition--;
                CameraMove();
            } 
            foreach (GenericPlayer player in character) //move characters into the current room
            {
                player.MoveAgent(CurrentPosition);
            }
        }
    }

    /// <summary>
    /// Moves the camera into position above the active room
    /// </summary>
    void CameraMove()
    {
        if (CurrentPosition <= 0)
            CurrentPosition = 0;

        if (CurrentPosition >= CameraPosition.Length)
            manager.OnWin();
        else
            main.gameObject.transform.position = CameraPosition[CurrentPosition].position;
    }
}
