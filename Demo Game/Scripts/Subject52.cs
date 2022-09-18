using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Subject52 : GenericPlayer
{

    SpriteRenderer sprite; //the character's sprite
    public Sprite normal; //normal-state sprite
    public Sprite invis; //invisible state sprite

    public float InvisTime = 2.5f; //time invisibility lasts (default 2.5 seconds)

	void Start ()
    {
        rb = GetComponent<Rigidbody>();
        AngleVelocity = new Vector3(90, 0, 0);
        player = this;
        if (!IsPlayer)
            CharText.enabled = false;

        sprite = GetComponent<SpriteRenderer>();
        source = GetComponent<AudioSource>();
    }
	
	void Update ()
    {
        if (IsPlayer && manager.alive)
            Movement();
	}

    public override void Ability()
    {
        if (tag != "Invisible") //set the sprite to the invisible version and set the character's tag to "Invisible"
        { 
            source.Play();
            tag = "Invisible";
            sprite.sprite = invis;
            StartCoroutine(Effect());
        }
    }

    IEnumerator Effect() //counts for 2.5 seconds before making the character visible again and returning tag to "Player"
    {
        yield return new WaitForSeconds(InvisTime);
        tag = "Player";
        sprite.sprite = normal;
    }
}
