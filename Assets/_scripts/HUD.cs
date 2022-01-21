using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour {
    [SerializeField] public GameObject StartUi;
    [SerializeField] public GameObject StopUi;
    [SerializeField] public GameObject OptionsUi;
    [SerializeField] public GameObject InGameHUD;
    [SerializeField] private GameObject ambientMusic;
    [SerializeField] public Text HP;

    private bool isLobby;

    private void Awake() {
        HandleEvents();
        StartUi.SetActive(true);
    }

    private void Update() {
        PlayAmbientMusicInGame();
        EscKeyClick();
    }

    private void PlayAmbientMusicInGame() {
        if (GameManager.instance.gameState == GameManager.GameState.InGame ||
            GameManager.instance.gameState == GameManager.GameState.Stop ||
            GameManager.instance.gameState == GameManager.GameState.Options) {
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

    public void OptionsOpenedFromLobby() {
        GameEvents.BroadcastOnGameStateChange(GameManager.GameState.Start);
        isLobby = true;
    }

    public void OptionsOpenedFromPauseMenu() {
        GameEvents.BroadcastOnGameStateChange(GameManager.GameState.Options);
        isLobby = false;
    }

    public void BackFromOptions() {
        switch (isLobby) {
            case true when GameManager.instance.gameState == GameManager.GameState.Start:
                GameEvents.BroadcastOnGameStateChange(GameManager.GameState.Start);
                StartUi.SetActive(true);
                OptionsUi.SetActive(false);
                break;
            case false when GameManager.instance.gameState == GameManager.GameState.Options:
                GameEvents.BroadcastOnGameStateChange(GameManager.GameState.Stop);
                StopUi.SetActive(true);
                OptionsUi.SetActive(false);
                break;
        }
    }

    private void EscKeyClick() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            BackFromOptions();
        }
    }

    public void GoToMainMenu() {
        if (GameManager.instance.gameState != GameManager.GameState.Stop) return;
        GameEvents.BroadcastOnGameStateChange(GameManager.GameState.Start);
        StopUi.SetActive(false);
        StartUi.SetActive(true);
    }

    public void ResumeButton() {
        GameEvents.BroadcastOnGameStateChange(GameManager.GameState.InGame);
        StopUi.SetActive(false);
    }

    public void HostLobbyButton() {
        GameEvents.BroadcastOnGameStateChange(GameManager.GameState.InGame);
    }

    public void LeaveGame() {
        Application.Quit();
    }
}