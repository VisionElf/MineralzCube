using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DepotEntity : Entity {

    //UNTIY PROPERTIES
    public ResourceContainer resourceContainer;

    //PROPERTIES
    void Start()
    {
        resourceContainer.Initialize(gameObject);
    }


    public bool IsEmpty()
    {
        return resourceContainer.IsEmpty();
    }
    
    public bool IsFull()
    {
        return resourceContainer.IsFull();
    }
}
