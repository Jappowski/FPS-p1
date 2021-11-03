using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameInput : MonoBehaviour
{
    private const string PlayerPrefsNameKey = "PlayerName";
    [SerializeField] private Button ContinueButtton;

    [Header("UI")] [SerializeField] private TMP_InputField nameInputField = null;

    public static string DisplayName { get; private set; }

    private void start()
    {
        SetUpInputField();
    }

    private void SetUpInputField()
    {
        if (!PlayerPrefs.HasKey(PlayerPrefsNameKey)) return;

        var defaultName = PlayerPrefs.GetString(PlayerPrefsNameKey);
        nameInputField.text = defaultName;
        setPlayerName(defaultName);
    }

    public void setPlayerName(string name)
    {
        ContinueButtton.interactable = !string.IsNullOrEmpty(name);
    }

    public void SavePlayerName()
    {
        DisplayName = nameInputField.text;
        PlayerPrefs.SetString(PlayerPrefsNameKey, DisplayName);
    }
}