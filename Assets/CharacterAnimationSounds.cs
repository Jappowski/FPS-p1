using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

public class CharacterAnimationSounds : NetworkBehaviour {
    private CharacterController playerController;
    [SerializeField] private GameObject player;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] footstepClips;

    private void Start() {
        playerController = GetComponent<CharacterController>();
    }
    [Command(requiresAuthority = false)]
    private void PlayerFootstepSound(string playerId) {
        var player = GameManager.GetPlayer(playerId);
        var playerfootsteps = GameManager.GetFootsteps(player);
        
        playerfootsteps.RpcPlayFootStepsSound();
    }
    
    [ClientRpc]
    private void RpcPlayFootStepsSound() {
        if (playerController.isGrounded && player.GetComponent<Player>().isDead == false) {
            var index = Random.Range(0, footstepClips.Length);
            audioSource.clip = footstepClips[index];
            audioSource.Play();
        }
    }
}
