using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientSoundShuffler : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] ambientClips;
    
    void Update()
    {
        PlayAmbientMusic();
    }

    private void PlayAmbientMusic() {
        if (!audioSource.isPlaying) {
            int index = Random.Range(0, ambientClips.Length);
            audioSource.clip = ambientClips[index];
            audioSource.Play();
        }
    }
}
