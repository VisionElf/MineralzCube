using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class ActionEntity : Entity {

    //UNITY PROPERTIES
    public float actionSpeed;
    public float actionRange;
    public float actionQuantity;

    //PROPERTIES
    protected Entity targetEntity;
    protected bool isDoingAction;

    //FUNCTION
    void Start()
    {
    }

    public void ActionOnEntity(Entity target)
    {
        if (SetEntity(target))
            StartAction();
        else
            StopAction();
    }

    public virtual bool SetEntity(Entity target)
    {
        targetEntity = target;
        return targetEntity != null;
    }
    public virtual Entity GetEntity()
    {
        return targetEntity;
    }

    public void StartAction()
    {
        StopAction();
        OnStartAction();
        StartCoroutine("Action");
    }
    public void StopAction()
    {
        if (isDoingAction)
        {
            StopCoroutine("Action");
            OnStopAction();
        }
    }

    IEnumerator Action()
    {
        if (basicProperties.CanReach(targetEntity, actionRange))
        {
            while (ActionCondition())
            {
                //ATTEINTE DE LA CIBLE
                while (ReachTarget())
                    yield return null;
                //CIBLE ATTEINTE -> EFFECTUER ACTION A INTERVAL actionSpeed seconds
                while (DoAction())
                    yield return new WaitForSeconds(actionSpeed);
            }
            OnActionDone();
        }
        else
            OnCantReach();
    }

    public virtual void OnCantReach()
    {
        StopAction();
        OnActionDone();
    }
    public virtual void OnActionDone() { isDoingAction = false; }

    public virtual void OnStartAction() { isDoingAction = true; }
    public virtual void OnStopAction() { isDoingAction = false; }
    public bool IsDoingAction() { return isDoingAction; }

    public abstract bool DoAction();
    public abstract bool ActionCondition();

    public bool ReachTarget()
    {
        return !basicProperties.Reached(targetEntity, actionRange);
    }
}
