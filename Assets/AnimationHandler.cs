using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class AnimationHandler : MonoBehaviour {
    [SerializeField] private GunShot gunShot;

    public void PlayMusic(int number) {
        gunShot.ReloadSoundPlay(number);
    }
}
