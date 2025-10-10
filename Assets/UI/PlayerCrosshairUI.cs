using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCrosshairUI : MonoBehaviour {

    [SerializeField] private Image ReloadImage;
    //[SerializeField] private PlayerShoot playerShoot;
    // Start is called before the first frame update
    void Start() {
        PlayerShoot.Reload += OnReload;
        //ReloadImage.gameObject.SetActive(false);
    }

    private void OnDestroy() {
        PlayerShoot.Reload -= OnReload;
    }


    private void OnReload(object sender, ReloadEventArgs e) {
        StartCoroutine(ReloadRoutine(e.ReloadTime));
    }

    private IEnumerator ReloadRoutine(float timeMax) {
        float time = timeMax;
        ReloadImage.fillAmount = 1;
        ReloadImage.gameObject.SetActive(true);

        while (time > 0) {
            time -= Time.deltaTime;
            ReloadImage.fillAmount = time / timeMax;
            yield return null; // wait until next frame
        }

        ReloadImage.fillAmount = 0;
        ReloadImage.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
