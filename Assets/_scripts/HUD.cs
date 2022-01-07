using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [SerializeField] public GameObject StartUi;
    [SerializeField] public GameObject StopUi;
    [SerializeField] public GameObject InGameHUD;
    [SerializeField] public Text HP;
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
        if (GameManager.instance.gameState == GameManager.GameState.InGame)
        {
            if (StopUi.activeSelf)
            {
                StopUi.SetActive(false);
                GameManager.instance.player.SetActive(true);
            }

            StopUi.SetActive(true);
            GameManager.instance.player.SetActive(false);
        }
    }

    public void HostLobbyButton()
    {
        GameEvents.BroadcastOnGameStateChange(GameManager.GameState.InGame);
    }
    public void LeaveGame()
    {
        Application.Quit();
    }
}
