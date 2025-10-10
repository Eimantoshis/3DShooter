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
    private Camera playerCamera; // where to shoot raycast from
    private bool isReloading = false;
    public static event EventHandler<AmmoChangedEventArgs> AmmoChanged;
    public static event EventHandler<ReloadEventArgs> Reload;
    // Start is called before the first frame update
    void Start() {
        maxBullets = 8;
        ReloadTime = 2f;
        CurrBullets = maxBullets;
    }

    public void SetCamera(Camera camera) {
        playerCamera = camera;
    }
    

    private void Shoot() {
        if (!playerCamera) return;
        
        if (isReloading) return;
        if (Input.GetMouseButtonDown(0)) {
            if (CurrBullets > 0) {
                CurrBullets--;
                AmmoChanged?.Invoke(this, new AmmoChangedEventArgs(CurrBullets));
                if (!playerCamera) Debug.Log("Camera needed");
                if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, Mathf.Infinity)) {
                    if (hit.collider.CompareTag("Player")) {
                        Debug.Log("Hit player");
                    }
                }

            }
            
        }
        if (CurrBullets <= 0) {
            StartCoroutine(ReloadRoutine());
        }
        
    }
    

    private void CallReload() {
        if (Input.GetKeyDown(KeyCode.R)) {
            if (!isReloading) {
                StartCoroutine(ReloadRoutine());
            }
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
    void Update() {
        Shoot();
        CallReload();
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
