using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public bool isGrabbing;
    public int ItemType;
    public bool isGrabbingOutput;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void PickedUpItem(int type)
    {
        isGrabbing = true;
        ItemType = type;
    }
    void PickedUpOutput()
    {
        isGrabbingOutput = true;
    }
}
