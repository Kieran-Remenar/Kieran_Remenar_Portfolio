using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Subject27 : GenericPlayer
{

    public float teleportDistance = 1.25f; //distance in world space to teleport
    RaycastHit rayHit;

	void Start ()
    {
        rb = GetComponent<Rigidbody>();
        AngleVelocity = new Vector3(90, 0, 0);
        player = this;
        if (!IsPlayer)
            CharText.enabled = false;

        if (IsPlayer) //set as default player when scene starts
            tag = "Player";
        else
            tag = "Standby";

        source = GetComponent<AudioSource>();
    }
	
	void Update ()
    {
        if (IsPlayer && manager.alive)
            Movement();
	}

    public override void Ability()
    {
        if (Physics.Raycast(transform.position, DirectionAsVector(), out rayHit, Mathf.Infinity, -5, QueryTriggerInteraction.Collide))
        {
            source.Play();
            transform.position = Vector3.MoveTowards(transform.position, rayHit.point, teleportDistance); //teleports character in the direction faced teleportDistance
        }                                                                                                                                                                  //units ahead or to a solid object in the way
    }
}
