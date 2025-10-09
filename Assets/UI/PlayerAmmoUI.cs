using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerAmmoUI : MonoBehaviour
{
    
    [SerializeField] private TextMeshProUGUI ammoText;
    private PlayerShoot playerShoot;

    private void Start() {
        playerShoot = GetComponentInParent<PlayerShoot>();
        PlayerShoot.AmmoChanged += OnAmmoChanged;
    }

    private void OnAmmoChanged(object sender, AmmoChangedEventArgs e) {
        ammoText.text = e.NewAmmo.ToString() + "/" + playerShoot.maxBullets;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
