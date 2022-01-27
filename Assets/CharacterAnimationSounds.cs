using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

public class CharacterAnimationSounds : NetworkBehaviour {
    private CharacterController playerController;
    [SerializeField] private GameObject playerGO;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] footstepClips;
    private void Start() {
        playerController = GetComponent<CharacterController>();
    }
    
    [Command(requiresAuthority = false)]
    private void PlayerFootstepSound() {
        RpcPlayFootStepsSound();
    }
    
    [TargetRpc]
    private void RpcPlayFootStepsSound() {
        if (playerController.isGrounded && playerGO.GetComponent<Player>().isDead == false) {
            var index = Random.Range(0, footstepClips.Length);
            audioSource.clip = footstepClips[index];
            audioSource.Play();
        }
    }
}
