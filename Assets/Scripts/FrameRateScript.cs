using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameRateScript : MonoBehaviour
{
    private float frameCount = 0;
    private float frameCountPerSecond;

    public int targetFrameRate = 60;

    void Start() {
        Application.targetFrameRate = targetFrameRate;
    }

    void Awake() {
        StartCoroutine(Loop());
        Application.targetFrameRate = targetFrameRate;
        QualitySettings.vSyncCount = 0;
    }

    void Update() {
        frameCount += 1;
    }

    void OnGUI() {
        GUIStyle fontSize = new GUIStyle(GUI.skin.GetStyle("label"));
        fontSize.fontSize = 24;
        GUI.Label(new Rect(100, 100, 200, 50), "Update: " + frameCountPerSecond.ToString(), fontSize);
    }

    IEnumerator Loop() {
        while (true) {
            yield return new WaitForSeconds(1);
            frameCountPerSecond = frameCount;

            frameCount = 0;
        }
    }
}
