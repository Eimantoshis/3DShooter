using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour {

    public int maxBullets { get; private set; }
    public int CurrBullets { get; private set; }
    public float ReloadTime { get; set; }
    [SerializeField] private Camera Playercamera; // where to shoot raycast from
    private bool isReloading = false;
    [SerializeField] private InputActionReference reloadInput;
    public static event EventHandler<AmmoChangedEventArgs> AmmoChanged;
    public static event EventHandler<ReloadEventArgs> Reload;
    private InputAction shoot;
    // Start is called before the first frame update
    void Start() {
        maxBullets = 8;
        ReloadTime = 2f;
        CurrBullets = maxBullets;
    }

    private void OnEnable() {
        shoot = new InputAction("Shoot", InputActionType.Button);
        shoot.AddBinding("<Mouse>/leftButton");
        shoot.started += Shoot;
        shoot?.Enable();
        reloadInput.action.started += ReloadInput;
    }
    private void OnDisable() {
        shoot.started -= Shoot;
        reloadInput.action.started -= ReloadInput;
    }

    private void Shoot(InputAction.CallbackContext context) {
        if (isReloading) return;
        if (CurrBullets > 0) {
            CurrBullets--;
            AmmoChanged?.Invoke(this, new AmmoChangedEventArgs(CurrBullets));
            if (Physics.Raycast(Playercamera.transform.position, Playercamera.transform.forward, out RaycastHit hit, Mathf.Infinity)) {
                if (hit.collider.CompareTag("Player")) {
                    Debug.Log("Hit player");
                }
            }

        }

        if (CurrBullets <= 0) {
            CallReload();
        }


        
    }
    
    private void ReloadInput(InputAction.CallbackContext context) {
        CallReload();
    }

    private void CallReload() {
        if (!isReloading) {
            StartCoroutine(ReloadRoutine());
        }
    }

    private IEnumerator ReloadRoutine() {
        isReloading = true;
        Reload?.Invoke(this, new ReloadEventArgs(ReloadTime));

        float time = ReloadTime;
        while (time > 0) {
            time -= Time.deltaTime;
            yield return null;
        }
        
        CurrBullets = maxBullets;
        AmmoChanged?.Invoke(this, new AmmoChangedEventArgs(CurrBullets));
        isReloading = false;
    }
    

    // Update is called once per frame
    void Update()
    {
    }
}

public class AmmoChangedEventArgs : EventArgs {
    public float NewAmmo { get; }

    public AmmoChangedEventArgs(float newAmmo) {
        NewAmmo = newAmmo;
    }
}

public class ReloadEventArgs : EventArgs {
    public float ReloadTime { get; }

    public ReloadEventArgs(float reloadTime) {
        ReloadTime = reloadTime;
    }
}
