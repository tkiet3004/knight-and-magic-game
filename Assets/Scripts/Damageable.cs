using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Damageable : MonoBehaviour
{
    public UnityEvent<int, Vector2> damageableHit;
    public UnityEvent damageableDeath;
    public UnityEvent<int, int> healthChanged;

    Animator animator;
    MaterialController materialController;

    [SerializeField]
    private int _maxHealth = 100;

    public int MaxHealth
    {
        get
        {
            return _maxHealth;
        }
        set
        {
            _maxHealth = value;
        }
    }

    [SerializeField]
    private int _health = 100;

    public int Health
    {
        get
        {
            return _health;
        }
        set
        {
            _health = value;
            healthChanged?.Invoke(_health, MaxHealth);

            // If health drops below 0, character is no longer alive
            if(_health <= 0)
            {
                IsAlive = false;
            }
        }
    }

    [SerializeField]
    private bool _isAlive = true;

    [SerializeField]
    private bool isInvincible = false;

    private float timeSinceHit = 0;
    public float invincibilityTime = 0.25f;

    public bool IsAlive {
        get
        {
            return _isAlive;
        }
        set
        {
            // Only update if state changes to prevent multiple death events
            if (_isAlive == value) return;

            _isAlive = value;
            animator.SetBool(AnimationStrings.isAlive, value);
            // Debug.Log("IsAlive set " + value);

            if(value == false)
            {
                damageableDeath.Invoke();
            }
        }
    }

    
    // the player controller
    public bool LockVelocity
    {
        get
        {
            return animator.GetBool(AnimationStrings.lockVelocity);
        }
        set
        {
            animator.SetBool(AnimationStrings.lockVelocity, value);
        }
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        // Ensure animator is synced
        if (animator) animator.SetBool(AnimationStrings.isAlive, _isAlive);
        
        // Add MaterialController for shader effects
        materialController = GetComponent<MaterialController>();
        if (materialController == null)
        {
            materialController = gameObject.AddComponent<MaterialController>();
        }

        if (CompareTag("Player"))
        {
            int hpLevel = PlayerPrefs.GetInt("HealthUpgrades", 0);
            if (hpLevel > 0)
            {
                int bonusHP = hpLevel * 50;
                _maxHealth += bonusHP;
                _health += bonusHP; // Increase current health by same amount to keep percentage or just fill it up?
                // Usually we just increase capacity. If we want to fully heal on start, we can do _health = _maxHealth.
                _health = _maxHealth; 
            }
        }
    }

    private void Update()
    {
        if(isInvincible)
        {
            if(timeSinceHit > invincibilityTime)
            {
                // Remove invincibility
                isInvincible = false;
                timeSinceHit = 0;
            }

            timeSinceHit += Time.deltaTime;
        }
    }

    // Returns whether the damageable took damage or not
    public bool Hit(int damage, Vector2 knockback)
    {
        if(IsAlive && !isInvincible)
        {
            Health -= damage;
            isInvincible = true;

            
            animator.SetTrigger(AnimationStrings.hitTrigger);
            LockVelocity = true;
            CharacterEvents.characterDamaged.Invoke(gameObject, damage);
            
            // Apply white flash shader effect
            if (materialController != null)
            {
                materialController.Flash(ShaderEffectLibrary.FlashColor.White, 0.1f);
            }
            damageableHit?.Invoke(damage, knockback);
            

            return true;
        }

        // Unable to be hit
        return false;
    }

    // Returns whether the character was healed or not
    public bool Heal(int healthRestore)
    {
        if(IsAlive && Health < MaxHealth)
        {
            int maxHeal = Mathf.Max(MaxHealth - Health, 0);
            int actualHeal = Mathf.Min(maxHeal, healthRestore);
            Health += actualHeal;
            CharacterEvents.characterHealed(gameObject, actualHeal);
            return true;
        }

        return false;
    }
}
