using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Singleton
    {
        get => _singleton;
        set
        {
            if (value == null)
                _singleton = null;
            else if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Destroy(value);
                Debug.LogError($"There should only ever be one instance of {nameof(UIManager)}!");
            }

        }
    }

    private static UIManager _singleton;

    [SerializeField] private TextMeshProUGUI gameStateText;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private Slider breakCD;
    [SerializeField] private Image breakSelected;
    [SerializeField] private Slider cageCD;
    [SerializeField] private Image cageSelected;
    [SerializeField] private Slider shoveCD;
    [SerializeField] private Image shoveSelected;
    [SerializeField] private Slider grappleCD;
    [SerializeField] private Slider doubleJumpCD;
    [SerializeField] private LeaderboardItem[] leaderboardItems;

    public Player LocalPlayer;

    private void Awake()
    {
        Singleton = this;

        breakCD.value = 0f;
        cageCD.value = 0f;
        shoveCD.value = 0f;
        grappleCD.value = 0f;
        doubleJumpCD.value = 0f;

        SelectAbility(AbilityMode.BreakBlock);
    }

    private void Update()
    {
        if (LocalPlayer == null)
            return;

        breakCD.value = LocalPlayer.BreakCDFactor;
        cageCD.value = LocalPlayer.CageCDFactor;
        shoveCD.value = LocalPlayer.ShoveCDFactor;

        grappleCD.value = LocalPlayer.GrappleCDFactor;
        doubleJumpCD.value = LocalPlayer.DoubleJumpCDFactor;
    }

    private void OnDestroy()
    {
        if (Singleton == this)
            Singleton = null;
    }

    public void DidSetReady()
    {
        instructionText.text = "Waiting for other players to be ready...";
    }

    public void SetWaitUI(GameState newState, Player winner)
    {
        if(newState == GameState.Waiting)
        {

            if (winner == null)
            {
                gameStateText.text = "Waiting to Start";
                instructionText.text = "Press R when you're ready to begin!";
            }
            else
            {
                gameStateText.text = $"{winner.Name} Wins";
                instructionText.text = "Press R when you're ready to play again!";
            }
        }

        gameStateText.enabled = newState == GameState.Waiting;
        instructionText.enabled = newState == GameState.Waiting;
    }

    public void SelectAbility(AbilityMode mode)
    {
        breakSelected.enabled = mode == AbilityMode.BreakBlock;
        cageSelected.enabled = mode == AbilityMode.Cage;
        shoveSelected.enabled = mode == AbilityMode.Shove;
    }

    public void UpdateLeaderboard(KeyValuePair<Fusion.PlayerRef, Player>[] players)
    {
        for(int i = 0; i < leaderboardItems.Length; i++)
        {
            LeaderboardItem item = leaderboardItems[i];
            if(i < players.Length)
            {
                item.nameText.text = players[i].Value.Name;
                item.heightText.text = $"{players[i].Value.Score}m";
            }
            else
            {
                item.nameText.text = "";
                item.heightText.text = "";
            }
        }
    }

    public void UpdateWaitingReadyboard(KeyValuePair<Fusion.PlayerRef, Player>[] players)
    {
        for (int i = 0; i < leaderboardItems.Length; i++)
        {
            LeaderboardItem item = leaderboardItems[i];
            if (i < players.Length)
            {
                item.nameText.text = players[i].Value.Name;
                item.heightText.text = "";
                item.readyToggle.gameObject.SetActive(true);
                item.readyToggle.isOn = players[i].Value.isReady;
            }
            else
            {
                item.nameText.text = "";
                item.heightText.text = "";
                item.readyToggle.gameObject.SetActive(false);
            }
        }
    }

    public void DisableReadyToggle(KeyValuePair<Fusion.PlayerRef, Player>[] players)
    {
        for (int i = 0; i < leaderboardItems.Length; i++)
        {
            LeaderboardItem item = leaderboardItems[i];
            if (i < players.Length)
            {
                item.readyToggle.gameObject.SetActive(false);
                players[i].Value.isReady = false;
                item.readyToggle.isOn = players[i].Value.isReady;
            }
        }
    }

    [Serializable]
    private struct LeaderboardItem
    {
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI heightText;
        public Toggle readyToggle;
    }
}
