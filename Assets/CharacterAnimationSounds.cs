using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimationSounds : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] footstepClips;

    private void PlayerFootstepSound()
    {
        var index = Random.Range(0, footstepClips.Length);
        audioSource.clip = footstepClips[index];
        audioSource.Play();
    }
}
