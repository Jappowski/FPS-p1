using Mirror;
using UnityEngine;

public class RandomBeanColor : NetworkBehaviour {
    [SerializeField] private Material material;

    void Start() {
        if (!isLocalPlayer) {
            material.color = Random.ColorHSV();
        }
    }
}