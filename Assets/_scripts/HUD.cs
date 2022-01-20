using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour {
    [SerializeField] public GameObject StartUi;
    [SerializeField] public GameObject StopUi;
    [SerializeField] public GameObject InGameHUD;
    [SerializeField] private GameObject ambientMusic;
    [SerializeField] public Text HP;

    private void Awake() {
        HandleEvents();
        StartUi.SetActive(true);
    }

    private void Update() {
        PlayAmbientMusicInGame();
    }

    private void PlayAmbientMusicInGame() {
        if ((GameManager.instance.gameState == GameManager.GameState.InGame) ||
            (GameManager.instance.gameState == GameManager.GameState.Stop)) {
            ambientMusic.SetActive(true);
        }
        else {
            ambientMusic.SetActive(false);
        }
    }

    private void HandleEvents() {
        GameEvents.onEscClick += OnEscClickHandle;
    }

    private void OnDisable() {
        GameEvents.onEscClick -= OnEscClickHandle;
    }

    private void OnEscClickHandle() {
        if (GameManager.instance.gameState == GameManager.GameState.InGame) {
            GameEvents.BroadcastOnGameStateChange(GameManager.GameState.Stop);
            StopUi.SetActive(true);
            Debug.Log("ON");
        }
        else {
            GameEvents.BroadcastOnGameStateChange(GameManager.GameState.InGame);
            StopUi.SetActive(false);
            Debug.Log("OFF");
        }
    }

    public void HostLobbyButton() {
        GameEvents.BroadcastOnGameStateChange(GameManager.GameState.InGame);
    }

    public void LeaveGame() {
        Application.Quit();
    }
}