using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;

public class CardsManager : NetworkBehaviour
{
    #region Singleton
    private static CardsManager _instance;

    public static CardsManager Instance
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

    private readonly Dictionary<int, Card> m_allCardsPrefabs = new Dictionary<int, Card>();
    private readonly Dictionary<int, Card> m_activeCards = new Dictionary<int, Card>();

    public void InitAllCards()
    {
        print("CardsManager:InitAllCards isServer = " + isServer);
        Card[] cardsObjs = Resources.LoadAll("Cards", typeof(Card)).Cast<Card>().ToArray();
        List<int> cardIds = new List<int>();
        foreach (Card cardObj in cardsObjs)
        {
            m_allCardsPrefabs.Add(cardObj.GetID(), cardObj);
            cardIds.Add(cardObj.GetID());
        }
        Deck.Instance.InitCards(cardIds);
    }

    public int[] GetAllCardIds()
    {
        int i = 0;
        int[] res = new int[m_allCardsPrefabs.Count];
        foreach(KeyValuePair<int, Card> item in m_allCardsPrefabs)
        {
            res[i] = item.Key;
            i++;
        }
        return res;
    }

    public void CreateHand(int[] cardIds, Transform[] placeHolders)
    {
        for (int i = 0; i < cardIds.Length; i++)
        {
            GameObject cardObj = Instantiate(m_allCardsPrefabs[cardIds[i]].gameObject);
            Card card = cardObj.GetComponent<Card>();
            m_activeCards.Add(card.GetID(), card);
            cardObj.transform.parent = placeHolders[i].transform;
            ResetCardTransform(cardIds[i]);
            UpdateCardAtHand(cardIds[i], true);
        }
    }

    public void AddCard(int cardId, Transform placeHolder)
    {
        GameObject cardObj;
        if (m_activeCards.ContainsKey(cardId))
        {
            cardObj = m_activeCards[cardId].gameObject;
        }
        else
        {
            cardObj = Instantiate(m_allCardsPrefabs[cardId].gameObject);
            Card card = cardObj.GetComponent<Card>();
            m_activeCards.Add(card.GetID(), card);
        }

        cardObj.transform.parent = placeHolder.transform;
        cardObj.transform.SetSiblingIndex(0);

        ResetCardTransform(cardId);
    }

    // If this cardId gameObject exists under the given parentPlaceHolder then destroy it.
    public void DestroyCardIfInParent(int cardId, Transform parentPlaceHolder)
    {
        if (m_activeCards.ContainsKey(cardId))
        {
            GameObject cardObj = m_activeCards[cardId].gameObject;
            if (cardObj.transform.parent == parentPlaceHolder)
                Destroy(cardObj);
        }
    }

    public void UpdateCardAtHand(int cardId, bool isInHand)
    {
        //print("UpdateCardAtHand " + cardId + " isInHand = " + isInHand);
        m_activeCards[cardId].isInHand = isInHand;
    }

    public void ResetCardTransform(int cardId)
    {
        ResetTranform(m_activeCards[cardId].transform);
    }

    private void ResetTranform(Transform _transform)
    {
        _transform.localPosition = Vector3.zero;
        _transform.localEulerAngles = Vector3.zero;
        _transform.localScale = Vector3.one;
    }

    public Transform GetCardParent(int cardId)
    {
        if (m_activeCards.ContainsKey(cardId))
        {
            GameObject cardObj = m_activeCards[cardId].gameObject;
            return cardObj.transform.parent;
        }

        return null;
    }

    public void ChangeCardParent(Transform previousParent, Transform newParent)
    {
        Transform cardTransform = previousParent.GetChild(0);
        cardTransform.parent = newParent;
        ResetTranform(cardTransform);
    }

    public void HideCard(int cardId)
    {
        GameObject cardObj = m_activeCards[cardId].gameObject;
        cardObj.SetActive(false);
    }

    public void ShowCard(int cardId)
    {
        GameObject cardObj = m_activeCards[cardId].gameObject;
        cardObj.SetActive(true);
    }
}
