using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText;
    private PlayerStats playerStats;
    // Start is called before the first frame update
    void Start() {
        playerStats = GetComponentInParent<PlayerStats>();
        if (!playerStats) Debug.LogError("PlayerStats not found");


        playerStats.HealthChanged += OnHealthChanged;
    }

    private void OnDestroy() {
        if (!playerStats) {
            playerStats.HealthChanged -= OnHealthChanged;
        }
    }

    private void OnHealthChanged(object sender, HealthChangedEventArgs e) {
        healthText.text = e.NewHealth.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
