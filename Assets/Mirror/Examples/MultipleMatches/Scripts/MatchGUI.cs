using System;
using UnityEngine;
using UnityEngine.UI;

namespace Mirror.Examples.MultipleMatch
{
    public class MatchGUI : MonoBehaviour
    {
        [Header("Diagnostics - Do Not Modify")]
        public CanvasController canvasController;

        [Header("GUI Elements")] public Image image;

        private Guid matchId;
        public Text matchName;
        public Text playerCount;
        public Toggle toggleButton;

        public void Awake()
        {
            canvasController = FindObjectOfType<CanvasController>();
            toggleButton.onValueChanged.AddListener(delegate { OnToggleClicked(); });
        }

        public void OnToggleClicked()
        {
            canvasController.SelectMatch(toggleButton.isOn ? matchId : Guid.Empty);
            image.color = toggleButton.isOn ? new Color(0f, 1f, 0f, 0.5f) : new Color(1f, 1f, 1f, 0.2f);
        }

        public Guid GetMatchId()
        {
            return matchId;
        }

        public void SetMatchInfo(MatchInfo infos)
        {
            matchId = infos.matchId;
            matchName.text = "Match " + infos.matchId.ToString().Substring(0, 8);
            playerCount.text = infos.players + " / " + infos.maxPlayers;
        }
    }
}