using System;

public static class GameEvents {
    public static event Action onEscClick;

    public static void BroadcastOnEscClick() {
        onEscClick?.Invoke();
    }

    public static event Action<GameManager.GameState> onGameStateChange;

    public static void BroadcastOnGameStateChange(GameManager.GameState gameState) {
        onGameStateChange?.Invoke(gameState);
    }
}