using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour {

    public GameManager Manager;
    public Transform Camera;
    GenericPlayer player;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) //detects if a player has entered the field of view of the camera
        {
            player = other.GetComponent<GenericPlayer>();
        }
    }

	void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player.InCameraView(Camera); //kills the player if they are in direct view of the camera
        }
    }
}
