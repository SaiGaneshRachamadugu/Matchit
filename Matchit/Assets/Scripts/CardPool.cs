using System.Collections.Generic;
using UnityEngine;

public class CardPool : MonoBehaviour
{
    [Header("Card Pool Configuration")]
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private int preloadAmount = 12;

    private readonly Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {
        if (cardPrefab == null)
        {
            Debug.LogError("[CardPool] Card prefab is not assigned.");
            return;
        }

        //Preload cards into the pool//
        for (int i = 0; i < preloadAmount; i++)
        {
            GameObject card = Instantiate(cardPrefab);
            card.SetActive(false);
            pool.Enqueue(card);
        }
    }

    //Retrieves a card from the pool, or instantiates one if the pool is empty//
    public GameObject GetCard()
    {
        GameObject card;

        if (pool.Count > 0)
        {
            card = pool.Dequeue();
        }
        else
        { 
            //If pool empty create new pool//
            card = Instantiate(cardPrefab);
        }

        card.SetActive(true);
        return card;
    }

    
    //Returns a card to the pool for reuse//
    public void ReturnCard(GameObject card)
    {
        if (card == null) return;

        card.SetActive(false);
        card.transform.SetParent(transform);
        pool.Enqueue(card);
    }

    //Clears the current pool//
    public void ClearPool()
    {
        pool.Clear();
    }
}
