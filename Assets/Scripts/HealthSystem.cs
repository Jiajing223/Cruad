using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private int health = 100;
    public event EventHandler OnDead;
    public event EventHandler OnDamaged;

    private int healthMax;

    private void Awake()
    {
        healthMax = health;
    }
    public int Damage(Unit userUnit = null ,Unit targetUnit = null , int damage = 0)
    {
        int finalDamage = calculateDamage(userUnit, targetUnit, damage);
        health -= finalDamage;
        if (health < 0) health = 0;
        OnDamaged?.Invoke(this, EventArgs.Empty);
        if (health == 0) Die();
        return finalDamage;
    }
    
    private int calculateDamage(Unit userUnit = null,Unit targetUnit = null, int damage = 0)
    {
        int finalDamage = 0;

        if (userUnit == null) // flat damage, no attacker
        {
            int targetDefense = targetUnit.GetUnitStat().GetDefense();
            // Damage formula: damage²/(damage + defense)
            finalDamage = Mathf.RoundToInt(
                (float)(damage * damage) / (damage + targetDefense)
            );
            
        }
        else // stat based damage
        {
            int userAttack = userUnit.GetUnitStat().GetAttack();
            int targetDefense = targetUnit.GetUnitStat().GetDefense();
            // Damage formula: attack²/(attack + defense)
            finalDamage = Mathf.RoundToInt(
                (float)(userAttack * userAttack) / (userAttack + targetDefense)
            );
            int critChance = userUnit.GetUnitStat().GetCriticalChance();
            bool isCrit = UnityEngine.Random.Range(0, 100) < critChance;
            if (isCrit)
                finalDamage = Mathf.RoundToInt(finalDamage * 1.5f);
        }
        
        return finalDamage;
    }
    private void Die()
    {
        OnDead?.Invoke(this, EventArgs.Empty);
    }

    public float GetHealthNormalized()
    {
        return (float)health / healthMax;
    }

    public int GetCalculatedDamage(Unit userUnit, Unit targetUnit)
{
    return calculateDamage(userUnit, targetUnit);
}
}
