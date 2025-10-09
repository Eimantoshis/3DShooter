using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour {

    
    [SerializeField] private float maxHealth = 100f;
    public static event EventHandler<HealthChangedEventArgs> HealthChanged;
    
    private float health;
    // Start is called before the first frame update
    void Start() {
        health = maxHealth;
        HealthChanged?.Invoke(this, new HealthChangedEventArgs(health));
    }

    // Update is called once per frame
    void Update() {
    }

    public void TakeDamage(float damage) {
        health -= damage;
        if (health <= 0) {
            health = 0;
        }
        HealthChanged?.Invoke(this, new HealthChangedEventArgs(health));
    }

    public void Heal(float heal) {
        health += heal;
        if (health > maxHealth) health = maxHealth;
        HealthChanged?.Invoke(this, new HealthChangedEventArgs(health));
    }
    
    
}

public class HealthChangedEventArgs : EventArgs {
    public float NewHealth { get; }

    public HealthChangedEventArgs(float newHealth) {
        NewHealth = newHealth;
    }
}
