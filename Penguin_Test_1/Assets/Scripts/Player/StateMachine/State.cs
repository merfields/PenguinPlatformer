using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State
{
    public abstract void OnStateStart();

    public abstract void StateUpdate();

    public abstract void OnStateExit();
}
