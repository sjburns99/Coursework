using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractScript : MonoBehaviour
{
    public float pushForce = 1000f;
    private Vector3 startPos; private Quaternion startRot;
    Rigidbody rb;
    public GameObject timelineManager; float interactDistance;
    public GameObject objectAffects;

    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;

        rb = GetComponent<Rigidbody>();
        interactDistance = 2f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void runInteraction(GameObject actor)
    {
        RaycastHit hunter; GameObject hunted;

        if (Physics.Raycast(actor.transform.position, actor.transform.forward, out hunter, interactDistance))
        {
            hunted = hunter.transform.gameObject;

            //Pushable object
            if (hunted.CompareTag("physicsObject") && pushForce != 0)
            {
                rb.AddForce(actor.transform.forward * pushForce);
            }
            //Button
            else if (hunted.CompareTag("pushButton"))
            {
                if (objectAffects)
                {
                    //Send signal to other object
                    Debug.Log("Sending signal to " + objectAffects.name);
                    objectAffects.SendMessage("doAction");
                }
            }
        }
    }

    public void loopBack()
    {
        transform.SetPositionAndRotation(startPos, startRot);
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}
