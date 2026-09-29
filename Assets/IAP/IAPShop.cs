using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAPShop : MonoBehaviour
{
    public void RewardPlayerWithStars(int amount)
    {
        Debug.Log(amount + " stars added");
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
    {
        Debug.Log("The purchase of " + product.definition.id + " failed due to " + reason);
    }
}
