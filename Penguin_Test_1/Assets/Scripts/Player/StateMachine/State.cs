using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State
{
    public abstract void OnStateStart(StateMachine stateMachine);

    public abstract void StateUpdate(StateMachine stateMachine);

    public abstract void OnStateExit(StateMachine stateMachine);

}
