using UnityEngine;
using UnityEngine.Audio;

public class OptionsMenu : MonoBehaviour {
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private AudioMixer ambientMixer;

    public void SetOverallVolume(float volume) {
        mainMixer.SetFloat("Overall", volume);
    }

    public void SetAmbientVolume(float volume) {
        ambientMixer.SetFloat("Ambient", volume);
    }

    public void SetFullscreen(bool isFullscreen) {
        Screen.fullScreen = isFullscreen;
    }

    public void SetQuality(int qualityIndex) {
        QualitySettings.SetQualityLevel(qualityIndex);
    }
}