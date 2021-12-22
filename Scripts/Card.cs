using System;
using UnityEngine;

[Serializable]
public class Card : MonoBehaviour
{
    public enum Suit {club, diamond, heart, spade }
    [SerializeField] private int value;
    [SerializeField] private Suit m_suit;

    [SerializeField] private Vector3 screenPoint;
    [SerializeField] private Vector3 offset;

    [SerializeField] private Camera m_mainCamera;
    public int Value { get => value; set => this.value = value; }
    public bool isInHand;

    public void Awake()
    {
        isInHand = false;
        m_mainCamera = FindObjectOfType<Camera>();
    }

    public void OnMouseDown()
    {
        screenPoint = m_mainCamera.WorldToScreenPoint(gameObject.transform.position);
        offset = gameObject.transform.position - m_mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
    }

    public void OnMouseDrag()
    {
        if (isInHand)
        {
            Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
            Vector3 curPosition = m_mainCamera.ScreenToWorldPoint(curScreenPoint) + offset;
            transform.position = curPosition;
        }
    }

    public void OnMouseUp()
    {
        if (isInHand)
        {
            ActionParams data = new ActionParams();
            data.Put("position", transform.position);
            data.Put("cardId", GetID());
            EventManager.TriggerEvent("ON_CARD_IN_HAND_RELEASE", data);
        }
    }

    public int GetID()
    {
        return value * 10 + ((int)m_suit);
    }
}
