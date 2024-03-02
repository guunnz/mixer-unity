using System.Collections;
using System.Collections.Generic;
using Shapes;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
    public Rectangle HPRectangle;
    public Rectangle ManaRectangle;
    public SpriteRenderer sr;

    public void SetSR(Sprite sprite)
    {
        sr.sprite = sprite;
    }
    
    public void SetMana(float mana)
    {
        ManaRectangle.Width = mana;
    }

    public void SetHP(float mana)
    {
        HPRectangle.Width = mana;
    }
}