using System.Collections;
using System.Collections.Generic;
using System;
using Fusion;
using UnityEngine;

public class PlayerStats : NetworkBehaviour {

    
    public static event EventHandler<HealthChangedEventArgs> HealthChanged;
    
    [Networked] 
    public float Health { get; set; } = 100;
    // Start is called before the first frame update
    void Start() {
        
    }

    private void UpdateHealthUI() {
        Debug.Log(Health);
        //HealthChanged?.Invoke(this, new HealthChangedEventArgs(Health));
    }

    // Update is called once per frame
    void Update() {
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void TakeDamageRPC(float damage) {
        Health -= damage;
        if (Health < 0) Health = 0;
        HealthChanged?.Invoke(this, new HealthChangedEventArgs(Health));
        Debug.Log(Health);
    }

    public void Heal(float heal) {
        Health += heal;

    }
}

public class HealthChangedEventArgs : EventArgs {
    public float NewHealth { get; }

    public HealthChangedEventArgs(float newHealth) {
        NewHealth = newHealth;
    }
}
