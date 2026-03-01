using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitStat : MonoBehaviour
{
    [SerializeField] private int attack = 10;
    [SerializeField] private int defense = 5;
    [SerializeField] private int moveRange = 4;

    public int GetAttack() => attack;
    public int GetDefense() => defense;
    public int GetMoveRange() => moveRange;

}
