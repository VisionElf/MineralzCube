using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DepotEntity : Entity {

    //UNTIY PROPERTIES
    public ResourceContainer resourceContainer;

    //PROPERTIES

    public void OnResouresChanged()
    {
        basicProperties.owner.OnResourcesChanged();
    }

    public bool IsEmpty(EResourceType resourceType)
    {
        return resourceContainer.IsEmpty(resourceType);
    }

    public bool IsFull(EResourceType resourceType)
    {
        return resourceContainer.IsFull(resourceType);
    }
}
