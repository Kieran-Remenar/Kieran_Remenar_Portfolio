using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class ControlPanel : MonoBehaviour
{

    public LaserBehaviour[] Lasers; //array of all lasers in the room (if any)
    public GameObject RoomCamera; //the security camera in the room
    public Animator Doors; //Animator for the doors to the next room
    public Text ConsoleText; //Text that shows the console is able to be used
    bool used = false; //has this console been turned off?

    void Start()
    {
        ConsoleText.enabled = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if ((other.CompareTag("Player") || other.CompareTag("Invisible")) && !used) //checks if a player is close enough to deactivate the console and turns on the text if true
        {
            ConsoleText.enabled = true;
        }
    }

    void OnTriggerStay(Collider other)
    {
        
        if ((other.CompareTag("Player") || other.CompareTag("Invisible")) && Input.GetButtonDown("Interact") && !used) //runs through deactivate protocols, such as
        {                                                                                                                                                                                                //turning off security and opening doors
            Deactivate();
            used = true;
            ConsoleText.enabled = false;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Invisible")) //disables text when player leaves trigger
        {
            ConsoleText.enabled = false;
        }
    }

    /// <summary>
    /// Deactivates all lasers and security cameras in a room
    /// </summary>
    void Deactivate()
    {
        foreach(LaserBehaviour Laser in Lasers)
        {
            Laser.Line.SetPosition(1, Laser.transform.position);
            Laser.enabled = false;
        }
        if (RoomCamera != null)
            RoomCamera.SetActive(false);
        Doors.SetTrigger("Console");
    }
}
