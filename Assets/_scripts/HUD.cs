using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUD : MonoBehaviour
{
    [SerializeField] private GameObject StartUi;
    [SerializeField] private GameObject StopUi;
    [SerializeField] protected GameObject InGameHUD;

    private void Awake()
    {
        HandleEvents();
        StartUi.SetActive(true);
    }

    private void HandleEvents()
    {
        GameEvents.onEscClick += OnEscClickHandle;
    }

    private void OnDisable()
    {
        GameEvents.onEscClick -= OnEscClickHandle;
    }

    private void OnEscClickHandle()
    {
        if (StopUi.activeSelf)
            StopUi.SetActive(false);
        StopUi.SetActive(true);
        GameEvents.BroadcastOnGameStateChange(GameManager.GameState.Stop);
    }
}
