using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBehaviour : MonoBehaviour {

    public LineRenderer Line; //the visible laser
    Vector3 Facing; //direction laser is pointing
    GameManager Manager;

	void Start ()
    {
        Manager = FindObjectOfType<GameManager>();
        Line = gameObject.GetComponent<LineRenderer>();
        Line.SetPosition(0, transform.position);

        //points the laser in the intended direction
        if (transform.rotation.eulerAngles.y == 0)
        {
            Facing = new Vector3(0, 0, 1);
        }
        else if (transform.rotation.eulerAngles.y == 90)
        {
            Facing = new Vector3(1, 0, 0);
        }
        else if (transform.rotation.eulerAngles.y == 180)
        {
            Facing = new Vector3(0, 0, -1);
        }
        else if (transform.rotation.eulerAngles.y == 270)
        {
            Facing = new Vector3(-1, 0, 0);
        }
        else
            Debug.Log("Rotation is not set to a proper value");
	}
	
	void Update () //If raycast hits a player character, trigger the death routine
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Facing, out hit))
        {
            Line.SetPosition(1, hit.point);
            if (hit.collider.CompareTag("Player") || hit.collider.CompareTag("Invisible"))
            {
                Manager.OnDeath();
            }
        }
	}
}
