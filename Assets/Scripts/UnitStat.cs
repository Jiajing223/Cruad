using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UnitStat : MonoBehaviour
{
    [SerializeField] private int attack = 10;
    [SerializeField] private int defense = 5;
    [SerializeField] private int speed = 5;

    [SerializeField] private static float accuracy = 80f;

    [SerializeField] private int critialChance = 5;

    [SerializeField] private int moveRange;

    private void Start()
    {
        moveRange = Mathf.RoundToInt(speed / 2f);
    }

    public int GetAttack() => attack;
    public int GetDefense() => defense;
    public int GetMoveRange() => moveRange;
    public int GetSpeed() => speed;
    public static float GetAccuracy() => accuracy;
    public int GetCriticalChance() => critialChance;


}
