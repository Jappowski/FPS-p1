using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager instance;

    public GameObject player;

    public GameState gameState;
    [SerializeField] public HUD hud;

    void Awake() {
        CheckForGameManagerInstance();
        HandleEvents();
        gameState = GameState.Start;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy() {
        UnhandleEvents();
    }

    private void HandleEvents() {
        GameEvents.onGameStateChange += OnGameStateChangeHandler;
    }

    private void UnhandleEvents() {
        GameEvents.onGameStateChange -= OnGameStateChangeHandler;
    }

    private void OnGameStateChangeHandler(GameState obj) {
        gameState = obj;
    }

    private void CheckForGameManagerInstance() {
        if (instance != null)
            Debug.Log("More then one GameManager in scene.");
        else
            instance = this;
    }

    #region Player tracking

    private const string PLAYER_ID_PREFIX = "Player ";
    private static Dictionary<string, Player> players = new Dictionary<string, Player>();

    private static Dictionary<Player, CharacterAnimationSounds> footsteps =
        new Dictionary<Player, CharacterAnimationSounds>();
    public static void RegisterPlayer(string _netID, Player _player) {
        string _playerID = PLAYER_ID_PREFIX + _netID;
        players.Add(_playerID, _player);
        _player.transform.name = _playerID;
    }

    public static void UnRegisterPlayer(string _playerID) {
        players.Remove(_playerID);
    }

    public static Player GetPlayer(string _playerID) {
        return players[_playerID];
    }

    public static CharacterAnimationSounds GetFootsteps(Player player) {
        return footsteps[player];
    }

    // void OnGUI()
    // {
    //    GUILayout.BeginArea(new Rect(200, 200, 200, 500));
    //    GUILayout.BeginVertical();
    //    foreach (string _playerID in players.Keys)
    //    {
    //       GUILayout.Label(_playerID + "   -   "+ players[_playerID].transform.name);
    //    }
    //    GUILayout.EndVertical();
    //    GUILayout.EndArea();
    // }

    #endregion


    public enum GameState {
        Start,
        Lobby,
        InGame,
        Stop,
        Options
    }
}