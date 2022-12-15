using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlateScript : MonoBehaviour
{
    public GameObject objectAffects;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Entered");
        objectAffects.SendMessage("doActionStay");
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Left");
        objectAffects.SendMessage("doActionStop");
    }
}
