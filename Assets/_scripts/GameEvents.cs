using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameEvents
{
    public static event Action onEscClick;
    public static void BroadcastOnEscClick()
    {
        onEscClick?.Invoke();
    }

    public static event Action<GameManager.GameState> onGameStateChange;
    public static void BroadcastOnGameStateChange(GameManager.GameState gameState)
    {
        onEscClick?.Invoke();
    }


}
