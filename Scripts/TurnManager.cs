using Mirror;

public class TurnManager : NetworkBehaviour
{
    #region Singleton
    private static TurnManager _instance;

    public static TurnManager Instance
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

    [SyncVar(hook = nameof(SyncPlayerIdx))]
    public int currentPlayerIdx = 0;

    public void SyncPlayerIdx(int oldVal, int newVal)
    {
        currentPlayerIdx = newVal;
    }

    [Command(requiresAuthority = false)]
    public void OnTurnEnded()
    {
        currentPlayerIdx = (currentPlayerIdx + 1) % GameManager.Instance.GetPlayersAmount();
    }
}
