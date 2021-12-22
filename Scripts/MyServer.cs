using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MyServer : NetworkBehaviour
{

    public override void OnStartClient()
    {
        base.OnStartClient();
        CardsManager.Instance.InitAllCards();
    }
}
