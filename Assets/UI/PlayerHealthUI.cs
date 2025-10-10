using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerHealthUI : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI healthText;
        //[SerializeField] private PlayerStats playerStats;
    // Start is called before the frst frame update
    void Start() {
        PlayerStats.HealthChanged += OnHealthChanged;
    }

    private void OnDestroy() {
        PlayerStats.HealthChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(object sender, HealthChangedEventArgs e) {
        healthText.text = e.NewHealth.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
