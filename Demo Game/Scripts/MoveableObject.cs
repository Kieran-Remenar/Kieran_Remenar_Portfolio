using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveableObject : MonoBehaviour
{
    
    Subject03 sub3; //the character Subject 03
    public Transform SubjectChild;

	void Start ()
    {
        sub3 = FindObjectOfType<Subject03>();
	}
	
	void Update () { }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.gameObject == sub3.gameObject) //detects if Subject 03 is within grabbing distance of the object
            {
                sub3.NearObject = true;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.gameObject == sub3.gameObject)
            {
                sub3.NearObject = false;
            }
        }
    }
}
