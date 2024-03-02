using System.Collections;
using System.Collections.Generic;
using Shapes;
using UnityEngine;

public class HPManager : MonoBehaviour
{
    public Rectangle HPRectangle;
    public Rectangle ManaRectangle;
    
    public void SetMana(float mana)
    {
        ManaRectangle.Width = mana;
    }

    public void SetHP(float mana)
    {
        HPRectangle.Width = mana;
    }
}