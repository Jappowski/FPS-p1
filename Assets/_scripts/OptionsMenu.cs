using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour {
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private AudioMixer ambientMixer;
    
    private Slider overallSlider;
    private Slider ambientSlider;

    private float soundOffValue = -80.0f;

    private void Start() {
        overallSlider = GameObject.Find("OptionsUI/Background/OverallVolumeSlider").GetComponent<Slider>();
        ambientSlider = GameObject.Find("OptionsUI/Background/AmbientVolumeSlider").GetComponent<Slider>();
    }

    public void SetOverallVolume(float volume) {
        mainMixer.SetFloat("Overall", volume > overallSlider.minValue ? volume : soundOffValue);
    }

    public void SetAmbientVolume(float volume) {
        ambientMixer.SetFloat("Ambient", volume > ambientSlider.minValue ? volume : soundOffValue);
    }

    public void SetFullscreen(bool isFullscreen) {
        Screen.fullScreen = isFullscreen;
    }
}