using UnityEngine;
using Mirror;

public class Hand : NetworkBehaviour
{
    [SerializeField] private int[] m_cardsInHand;
    [SerializeField] private Transform m_selectedCardPlaceHolder;
    [SerializeField] private float m_cardRadius;

    private Transform[] m_placeHolders;
    private int m_selectedCardId;
    private float m_startDelay = 2f;
    private bool m_isSelectedCard;
    private int m_playerId;

    public override void OnStartClient()
    {
        base.OnStartClient();
        int i = 0;
        m_placeHolders = new Transform[transform.childCount];
        foreach (Transform child in transform)
        {
            m_placeHolders[i] = child;
            i++;
        }

        EventManager.StartListening("ON_DECK_CARD_CLICKED", OnDeckCardClicked);
        EventManager.StartListening("ON_CARD_IN_HAND_RELEASE", OnCardInHandRelease);
        EventManager.StartListening("PLAYER_ID", OnGetPlayerId);
    }

    private void OnGetPlayerId(string eventName, ActionParams data)
    {
        m_playerId = data.Get<int>("playerId");
    }

    private void OnCardInHandRelease(string eventName, ActionParams data)
    {
        Vector3 cardPos = data.Get<Vector3>("position");
        int cardId = data.Get<int>("cardId");
        bool isCardDiscarded = false;

        // Handle discarding a card to the discardPile and ending our turn.
        if (GameManager.Instance.IsCurrentPlayer(m_playerId) && m_isSelectedCard)
        {
            if (Deck.Instance.IsInDiscardPileRadius(cardPos))
            {
                MoveCardToHandPlaceHolder(m_selectedCardId, CardsManager.Instance.GetCardParent(cardId));
                Deck.Instance.DiscardCardFromHand(cardId);
                CardsManager.Instance.UpdateCardAtHand(cardId, false);
                CardsManager.Instance.UpdateCardAtHand(m_selectedCardId, true);
                TurnManager.Instance.OnTurnEnded();
                m_isSelectedCard = false;
                isCardDiscarded = true;
            }
        }

        // Handle changing the position of cards in our hand.
        if (!isCardDiscarded)
        {
            (int placeHolderIdx, float distance) = GetClosestPlaceHolderIdx(cardPos);

            // If it's not close to any card then reset the card, otherwise trade positions with the current card at the close placeHolder.
            if (distance < m_cardRadius)
            {
                print("Hand:Dragging " + cardId + " to " + m_placeHolders[placeHolderIdx].name);
                CardsManager.Instance.ChangeCardParent(m_placeHolders[placeHolderIdx], CardsManager.Instance.GetCardParent(cardId));
                MoveCardToHandPlaceHolder(cardId, m_placeHolders[placeHolderIdx]);
            }
            else
            {
                CardsManager.Instance.ResetCardTransform(cardId);
            }
        }
    }

    private (int, float) GetClosestPlaceHolderIdx(Vector3 cardPosition)
    {
        int placeHolderIdx = 0;
        float distance = Vector3.Distance(cardPosition, m_placeHolders[0].position);
        for (int i = 1; i < m_placeHolders.Length; i++)
        {
            float currentDistance = Vector3.Distance(cardPosition, m_placeHolders[i].position);
            if (currentDistance < distance)
            {
                distance = currentDistance;
                placeHolderIdx = i;
            }
        }

        return (placeHolderIdx, distance);
    }

    private void MoveCardToHandPlaceHolder(int cardId, Transform placeHolder)
    {
        CardsManager.Instance.AddCard(cardId, placeHolder);
    }

    private void OnDeckCardClicked(string eventName, ActionParams data)
    {
        if (GameManager.Instance.IsCurrentPlayer(m_playerId) && !m_isSelectedCard)
        {
            m_isSelectedCard = true;

            int deckSourceType = data.Get<int>("source");
            Vector3 deckSourcePos = data.Get<Vector3>("sourcePos");
            m_selectedCardId = data.Get<int>("cardId");
            CardsManager.Instance.AddCard(m_selectedCardId, m_selectedCardPlaceHolder);
            Deck.Instance.OnTakeFromDeck(deckSourceType, m_selectedCardId);
        }
        else
        {
            print("Hand:OnDeckCardClicked: m_isSelectedCard = " + m_isSelectedCard + " IsCurrentPlayer = " + GameManager.Instance.IsCurrentPlayer(m_playerId));
        }
    }

    private void Update()
    {
        if (m_startDelay > 0f)
        {
            m_startDelay -= Time.deltaTime;
            if (m_startDelay <= 0f)
            {
                m_cardsInHand = Deck.Instance.GetStartHand();
                CardsManager.Instance.CreateHand(m_cardsInHand, m_placeHolders);
            }
        }
    }
}
