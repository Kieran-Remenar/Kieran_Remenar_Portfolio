using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenericPlayer : MonoBehaviour
{
    public GameManager manager;

    public enum Player { Subject27, Subject03, Subject52 }; //the 3 player characters in the scene
    public Player subject; //the character currently selected
    public bool IsPlayer = false; //is this character the current subject?
    public float speed = 2; //movement speed
    public Text CharText; //helping text for character in use
    
    protected AudioSource source;
    protected Rigidbody rb;
    protected Vector3 MoveInput; //directional input
    protected Vector3 AngleVelocity; //direction to face the character in euler rotation
    protected GenericPlayer player;

    public GenericPlayer[] Characters; //array of character objects
    public Transform[] DockingLoc; //the locations for moving the non-player characters into the room when moving to a different room

	void Start ()
    {
        rb = GetComponent<Rigidbody>();
        AngleVelocity = new Vector3(90, 0, 0); //begin characters facing forward
	}

    protected void Movement()
    {
        MoveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")) * speed; //get movement information from player control

        //my clunky way of getting 2D movement in a 3D space
        if (rb.velocity == new Vector3(0, 0, speed))
        {
            AngleVelocity = new Vector3(90, 0, 0);
        }
        else if (rb.velocity == new Vector3(speed, 0, speed))
        {
            AngleVelocity = new Vector3(90, 45, 0);
        }
        else if (rb.velocity == new Vector3(speed, 0, 0))
        {
            AngleVelocity = new Vector3(90, 90, 0);
        }
        else if (rb.velocity == new Vector3(speed, 0, -speed))
        {
            AngleVelocity = new Vector3(90, 135, 0);
        }
        else if (rb.velocity == new Vector3(0, 0, -speed))
        {
            AngleVelocity = new Vector3(90, 180, 0);
        }
        else if (rb.velocity == new Vector3(-speed, 0, -speed))
        {
            AngleVelocity = new Vector3(90, 225, 0);
        }
        else if (rb.velocity == new Vector3(-speed, 0, 0))
        {
            AngleVelocity = new Vector3(90, 270, 0);
        }
        else if (rb.velocity == new Vector3(-speed, 0, speed))
        {
            AngleVelocity = new Vector3(90, 315, 0);
        }

        //call the overridden ability function for that character when ability button (default spacebar) is pressed
        if (Input.GetButtonDown("Ability"))
        {
            if (subject == Player.Subject27)
                Ability();
            else if (subject == Player.Subject03)
                Ability();
            else if (subject == Player.Subject52)
                Ability();
            else Debug.Log("Controlled subject does not have a number assigned");
        }

        //change character when button is pressed
        if (Input.GetButtonDown("Subject27"))
            ChangePlayer(Characters[0]);
        else if (Input.GetButtonDown("Subject03"))
            ChangePlayer(Characters[1]);
        else if (Input.GetButtonDown("Subject52"))
            ChangePlayer(Characters[2]);
    }

	void FixedUpdate() //translate euler rotation to quaternion rotation for use in-game
    {
        rb.velocity = MoveInput;
        Quaternion DeltaRotation = Quaternion.Euler(AngleVelocity);
        rb.MoveRotation(DeltaRotation);
        if (!manager.alive || !IsPlayer)
            rb.velocity = new Vector3(0, 0, 0);
	}

    /// <summary>
    /// Changes the player character to the one assigned to the button
    /// </summary>
    /// <param name="sub">The character to be swapped in</param>
    protected void ChangePlayer(GenericPlayer sub)
    {
        foreach (GenericPlayer player in Characters)
        {
            player.IsPlayer = false;
            player.CharText.enabled = false;
        }

        sub.IsPlayer = true;
        sub.CharText.enabled = true;
    }

    /// <summary>
    /// Overridden for each character. Activates a character's special ability
    /// </summary>
    virtual public void Ability()
    {
        Debug.Log("Ability has not been overridden");
    }

    /// <summary>
    /// Is the character in view of a camera?
    /// </summary>
    /// <param name="camera">The camera object's transform</param>
    public void InCameraView(Transform camera)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, (camera.transform.position - transform.position), out hit))
        {
            Debug.DrawRay(transform.position, (camera.transform.position - transform.position), Color.white);
            if (hit.collider.CompareTag("Camera"))
                manager.OnDeath();
        }
    }

    /// <summary>
    /// Translates the direction into a format easily usable by Unity
    /// </summary>
    /// <returns>Vector3 Direction</returns>
    public Vector3 DirectionAsVector()
    {
        Vector3 Direction;
        Direction = new Vector3(Mathf.Sin(AngleVelocity.y * Mathf.Deg2Rad), 0, Mathf.Cos(AngleVelocity.y * Mathf.Deg2Rad));
        return Direction;
    }

    /// <summary>
    /// Moves character into room currently in use
    /// </summary>
    /// <param name="index">Index of the transform to which the character is to be moved</param>
    public void MoveAgent(int index)
    {
        if (!IsPlayer && index < DockingLoc.Length)
        {
            transform.position = player.DockingLoc[index].position;
        }
    }
}
