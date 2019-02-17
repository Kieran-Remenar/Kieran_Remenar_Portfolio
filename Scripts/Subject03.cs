using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Subject03 : GenericPlayer
{

    public bool NearObject; //is character near a MoveableObject?
    public bool isCarrying; //is character carrying a MoveableObject?
    RaycastHit rayHit;
    GameObject carried; //GameObject being caried, if any
    Transform child; //location attached to move the MoveableObject to when picked up
    Transform objParent;
    Rigidbody objBody; //rigidbody on the MoveableObject being carried

	void Start ()
    {
        rb = GetComponent<Rigidbody>();
        AngleVelocity = new Vector3(90, 0, 0);
        player = this;
        if (!IsPlayer)
            CharText.enabled = false;
        child = GetComponentInChildren<Transform>();

        source = GetComponent<AudioSource>();
    }
	
	void Update ()
    {
        if (IsPlayer && manager.alive)
            Movement();
	}

    public override void Ability()
    {
        if (carried == null)
        {
            PickUp();
        }
        else if (carried != null)
        {
            Drop();
        }
    }

    /// <summary>
    /// Attaches the MoveableObject to the character's child transform and removes its RigidBody
    /// </summary>
    void PickUp()
    {
        if (Physics.Raycast(transform.position, (DirectionAsVector() + new Vector3(0, .1f, 0)), out rayHit,Mathf.Infinity, -5, QueryTriggerInteraction.Collide) && 
            rayHit.collider.CompareTag("Moveable") && NearObject) //Is the player facing the object s/he is trying to pick up?
        {
            carried = rayHit.collider.gameObject;
            isCarrying = true;
            Destroy(carried.GetComponent<Rigidbody>()); //remove the RigidBody from the MoveableObject
            objBody = null;
            carried.transform.parent = child;
            carried.transform.position = carried.transform.position + new Vector3(0, 0.1f, 0);
            source.Play();
        }
    }


/// <summary>
/// Drops the MoveableObject on the ground, giving it a new RigidBody
/// </summary>
    void Drop()
    {
        isCarrying = false;
        carried.gameObject.AddComponent<Rigidbody>(); //add and configure the object's RigidBody
        objBody = carried.GetComponent<Rigidbody>();
        objBody.mass = 3f;
        objBody.drag = 10f;
        objBody.freezeRotation = true;
        carried.transform.parent = null;
        carried = null;
    }
}
