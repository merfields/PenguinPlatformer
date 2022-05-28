using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State
{
    public virtual void OnStateStart(StateMachine stateMachine){}

    public virtual void StateUpdate(StateMachine stateMachine){}

    public virtual void OnStateExit(StateMachine stateMachine){}

}
