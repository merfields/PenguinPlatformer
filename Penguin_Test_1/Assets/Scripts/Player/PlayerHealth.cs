using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{

    [SerializeField] private int health = 3;
    [SerializeField] private int numberOfHearts = 3;

    [SerializeField]
    Image[] hearts;

    [SerializeField]
    Sprite heartSprite;


    // Update is called once per frame
    void Update()
    {
        if (numberOfHearts > health)
        {
            numberOfHearts = health;
        }
        for (int i = 0; i < hearts.Length; i++)
        {

            if (i < health)
            {
                hearts[i].sprite = heartSprite;
            }

            if (i<numberOfHearts)
            {
                hearts[i].enabled = true;
            }
            else
            {
                hearts[i].enabled = false;
            }
        }
    }
}
