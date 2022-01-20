using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CharacterAnimationSounds : MonoBehaviour {
    [SerializeField] private GameObject player;
    [SerializeField] private CharacterController playerController;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] footstepClips;

    private void PlayerFootstepSound()
    {
        if (playerController.isGrounded && player.GetComponent<Player>().isDead == false) {
            var index = Random.Range(0, footstepClips.Length);
            audioSource.clip = footstepClips[index];
            audioSource.Play();
        }
    }
}
