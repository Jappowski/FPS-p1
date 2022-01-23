using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour {
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private AudioMixer ambientMixer;
    private Slider overallSlider;
    private Slider ambientSlider;

    private void Start() {
        overallSlider = GameObject.Find("OptionsUI/Background/OverallVolumeSlider").GetComponent<Slider>();
        ambientSlider = GameObject.Find("OptionsUI/Background/AmbientVolumeSlider").GetComponent<Slider>();
    }

    public void SetOverallVolume(float volume) {
        if (volume > overallSlider.minValue) {
            mainMixer.SetFloat("Overall", volume);
        }
        else {
            mainMixer.SetFloat("Overall", -80.0f);
        }
    }

    public void SetAmbientVolume(float volume) {
        if (volume > ambientSlider.minValue) {
            ambientMixer.SetFloat("Ambient", volume);
        }
        else {
            ambientMixer.SetFloat("Ambient", -80.0f);
        }
    }

    public void SetFullscreen(bool isFullscreen) {
        Screen.fullScreen = isFullscreen;
    }

    public void SetQuality(int qualityIndex) {
        QualitySettings.SetQualityLevel(qualityIndex);
    }
}