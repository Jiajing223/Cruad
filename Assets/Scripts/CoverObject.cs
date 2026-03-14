using UnityEngine;
using System;
public class CoverObject : MonoBehaviour
{
    [SerializeField] private int dodgeBonus = 40;
    public int GetDodgeBonus() => dodgeBonus;
}