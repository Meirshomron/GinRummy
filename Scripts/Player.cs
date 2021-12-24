using Mirror;

public class Player : NetworkBehaviour
{
    public override void OnStartClient()
    {
        if (isLocalPlayer)
        {
            base.OnStartClient();

            CardsManager.Instance.InitAllCards();
            GetPlayerId();
        }
    }

    [Command]
    public void GetPlayerId()
    {
        GameManager.Instance.AddPlayerId(connectionToClient.connectionId);
        UpdatePlayerId(connectionToClient.connectionId);
    }

    [ClientRpc]
    public void UpdatePlayerId(int _id)
    {
        if (isLocalPlayer)
        {
            ActionParams data = new ActionParams();
            data.Put("playerId", _id);
            EventManager.TriggerEvent("PLAYER_ID", data);
        }
    }
}

