using Fusion;
using UnityEngine;


public class Player : NetworkBehaviour {
    private PlayerMovement movement;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }



}