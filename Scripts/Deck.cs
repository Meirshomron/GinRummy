using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Deck : NetworkBehaviour
{
    private readonly SyncList<int> m_cardsInDeck = new SyncList<int>();
    [SerializeField] private Transform m_discardCardParent;

    [SyncVar(hook = nameof(SyncDeckIdx))]
    public int deckIdx;

    private readonly SyncList<int> m_discardedCards = new SyncList<int>();

    private Collider m_deckCollider;
    private Collider m_discardCardCollider;
    public enum CardSource {DeckTop, DiscardPile};

    private Vector3 discardPosition;
    [SerializeField] private float discardRadius;

    #region Singleton
    private static Deck _instance;

    public static Deck Instance
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

    public override void OnStartClient()
    {
        base.OnStartClient();

        m_deckCollider = GetComponent<Collider>();
        m_discardCardCollider = m_discardCardParent.gameObject.GetComponent<Collider>();
        discardPosition = m_discardCardParent.position;
        m_discardedCards.Callback += OnDiscardedCardsUpdated;
    }

    private void OnDiscardedCardsUpdated(SyncList<int>.Operation op, int itemIndex, int oldItem, int newItem)
    {
        print("Deck:OnDiscardedCardsUpdated newItem = " + newItem + " oldItem = " + oldItem + " op = " + op.ToString());
        switch (op)
        {
            case SyncList<int>.Operation.OP_INSERT:
                if (m_discardedCards.Count > 1)
                    CardsManager.Instance.HideCard(m_discardedCards[1]);
                CardsManager.Instance.AddCard(newItem, m_discardCardParent);
                break;

            case SyncList<int>.Operation.OP_REMOVEAT:
                if (m_discardedCards.Count > 0)
                {
                    if (oldItem != m_discardedCards[0])
                        CardsManager.Instance.ShowCard(m_discardedCards[0]);
                }
                CardsManager.Instance.DestroyCardIfInParent(oldItem, m_discardCardParent);
                break;
        }
    }

    public void InitCards(List<int> cardIds)
    {
        print("Deck:InitCards isServer = " + isServer);
        if (isServer)
        {
            for (int i = 0; i < cardIds.Count; i++)
            {
                m_cardsInDeck.Add(cardIds[i]);
            }
            ShuffleAllCards();
            m_discardedCards.Add(m_cardsInDeck[0]);
            deckIdx = 1;
        }
        CardsManager.Instance.AddCard(m_cardsInDeck[0], m_discardCardParent);
    }

    void Update()
    {
        if (GameManager.Instance.IsGameActive() && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (m_deckCollider.Raycast(ray, out hit, 100))
            {
                OnDeckClicked();
            }
            if (m_discardCardCollider.Raycast(ray, out hit, 100))
            {
                OnDiscardCardClicked();
            }
        }
    }

    private void OnDiscardCardClicked()
    {
        print("Deck:OnDiscardCardClicked");
        if (m_discardedCards.Count > 0)
        {
            TriggerOnDeckCardClicked((int)CardSource.DiscardPile, m_discardCardParent.position, m_discardedCards[0]);
        }
    }

    public void OnDeckClicked()
    {
        print("Deck:OnDeckClicked");
        TriggerOnDeckCardClicked((int)CardSource.DeckTop, transform.position, m_cardsInDeck[deckIdx]);
    }

    private void TriggerOnDeckCardClicked(int source, Vector3 sourcePos, int cardId)
    {
        ActionParams data = new ActionParams();
        data.Put("source", source);
        data.Put("sourcePos", sourcePos);
        data.Put("cardId", cardId);
        EventManager.TriggerEvent("ON_DECK_CARD_CLICKED", data);
    }

    private void ShuffleAllCards() 
    {
        int temp;
        for (int i = 0; i < m_cardsInDeck.Count; i++)
        {
            int rnd = Random.Range(0, m_cardsInDeck.Count);
            temp = m_cardsInDeck[rnd];
            m_cardsInDeck[rnd] = m_cardsInDeck[i];
            m_cardsInDeck[i] = temp;
        }
    }

    public void OnTakeFromDeck(int deckSourceType, int cardId) 
    {
        if (deckSourceType == (int)CardSource.DeckTop)
        {
            UpdateDeckIdx(deckIdx + 1);
        }
        else if (deckSourceType == (int)CardSource.DiscardPile)
        {
            RemoveCardFromDiscardPile(cardId);
        }
    }

    public bool IsInDiscardPileRadius(Vector3 cardPosition)
    {
        return Vector3.Distance(cardPosition, discardPosition) < discardRadius;
    }

    public int[] GetStartHand() 
    {
        print("Deck:GetStartHand isServer = " + isServer);
        int[] res = new int[GameManager.CARDS_IN_HAND];

        for (int i = 0; i < GameManager.CARDS_IN_HAND; i++)
        {
            res[i] = m_cardsInDeck[i + deckIdx];
        }
        UpdateDeckIdx(deckIdx + GameManager.CARDS_IN_HAND);
        return res;
    }

    public void SyncDeckIdx(int oldVal, int newVal)
    {
        deckIdx = newVal;
    }

    [Command(requiresAuthority = false)]
    public void UpdateDeckIdx(int newVal)
    {
        deckIdx = newVal;
    }

    [Command(requiresAuthority = false)]
    public void DiscardCardFromHand(int cardId)
    {
        m_discardedCards.Insert(0, cardId);
    }

    [Command(requiresAuthority = false)]
    public void RemoveCardFromDiscardPile(int cardId)
    {
        m_discardedCards.Remove(cardId);
    }

    public bool isDeckEmpty() { return false; }
}
