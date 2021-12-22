using UnityEngine;
using Mirror;

public class GameManager : NetworkBehaviour
{
    #region Singleton
    private static GameManager _instance;

    public static GameManager Instance
    {
        get { return _instance; }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        _instance = this;
    }

    #endregion

    public const int CARDS_IN_HAND = 10;
    public const int TOTAL_CARDS = 52;
    public const int TOTAL_PLAYERS = 2;
    public readonly SyncList<int> playerIds = new SyncList<int>();

    public void Start()
    {
        playerIds.Callback += OnPlayerIdsUpdated;
    }

    private void OnPlayerIdsUpdated(SyncList<int>.Operation op, int itemIndex, int oldItem, int newItem)
    {
        print("GameManager:OnPlayerIdsUpdated newItem = " + newItem);
        switch (op)
        {
            case SyncList<int>.Operation.OP_INSERT:
                if (!playerIds.Contains(newItem))
                    AddPlayerId(newItem);
                break;
        }
    }

    public bool IsGameActive()
    {
        return playerIds.Count == TOTAL_PLAYERS;
    }

    public void AddPlayerId(int playerId)
    {
        playerIds.Insert(playerIds.Count, playerId);
    }

    public bool IsCurrentPlayer(int playerId)
    {
        //print("IsCurrentPlayer currentPlayer = " + playerIds[TurnManager.Instance.currentPlayerIdx] + " Got = " + playerId);
        return playerIds[TurnManager.Instance.currentPlayerIdx] == playerId;
    }

    public int GetPlayersAmount()
    {
        return playerIds.Count;
    }
}