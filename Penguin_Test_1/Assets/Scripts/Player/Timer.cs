using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{

    private float timeLeft;
    public Timer()
    {
        timeLeft = 0;
    }

    public float TimeLeft
    {
        get => this.timeLeft;
        set => this.timeLeft = value;
    }

    void Update()
    {
        if (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
        }
        else timeLeft = 0;
    }
}
