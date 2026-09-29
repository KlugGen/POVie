using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAPController : MonoBehaviour, IStoreListener
{

    static private IAPController instance;

    static public IAPController Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.FindObjectsOfTypeAll(typeof(IAPController))[0] as IAPController;

            return instance;
        }
    }

    public IStoreController controller;
    public IExtensionProvider extensions;

    void Start()
    {
        //InitIAP();
    }



    public void InitIAP()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        
        builder.AddProduct("com.kluggen.perspectives.10stars", ProductType.Consumable);
        builder.AddProduct("com.kluggen.perspectives.20stars", ProductType.Consumable);
        builder.AddProduct("com.kluggen.perspectives.50stars", ProductType.Consumable);
        builder.AddProduct("com.kluggen.perspectives.100stars", ProductType.Consumable);

        //builder.AddProduct("20_stars", ProductType.Consumable, new IDs
        //{
        //    {"20_stars", GooglePlay.Name},
        //    {"20_stars", MacAppStore.Name}
        //});

        //builder.AddProduct("50_stars", ProductType.Consumable, new IDs
        //{
        //    {"50_stars", GooglePlay.Name},
        //    {"50_stars", MacAppStore.Name}
        //});

        //builder.AddProduct("100_stars", ProductType.Consumable, new IDs
        //{
        //    {"100_stars", GooglePlay.Name},
        //    {"100_stars", MacAppStore.Name}
        //});

        UnityPurchasing.Initialize(this, builder);
    }

    /// <summary>
    /// Called when Unity IAP is ready to make purchases.
    /// </summary>
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        "IAP intialized".Log();
        this.controller = controller;
        this.extensions = extensions;
        
        foreach(Product p in controller.products.all)
        {
            string.Format("Product: {0}, price: {1}, {2}", p.definition.id, p.metadata.localizedPrice, p.metadata.localizedPriceString).Log();
        }
    }

    public void OnPurchaseClicked(string productId)
    {
        if(controller == null)
        {
            "Controller not intitialized".Log();
            return;
        }
        controller.InitiatePurchase(productId);
    }

    public void OnPurchaseComplete(Product product)
    {
        //product.transactionID.Log("Transaction ID: ");
      
        //product.availableToPurchase.Log("");
        //product.definition.Log("");
        //product.hasReceipt.Log("");
        //product.receipt.Log("");
        //product.metadata.Log("");

        //product.metadata.localizedPriceString.LogDev("Price");
        AnalyticsController.Instance.LogEvent(Firebase.Analytics.FirebaseAnalytics.EventPurchase, 
            new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterValue, product.metadata.localizedPriceString),
            new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterTransactionId, product.transactionID)
            );
    }

    public void PurchaseCompleteRewardStars(int stars)
    {
        UserController.Instance.AddStars(stars, "iap_purchase_");
        StarRewardScreen.Instance.Enable(TextTranslationModule.GetWord("Transaction successful"), stars);
        MapViewController.Instance.AddViewToClose(StarRewardScreen.Instance);
    }

    public void PurchaseCompleteRewardStars(int stars, string tID = "")
    {
        UserController.Instance.AddStars(stars, "iap_purchase_" + tID);
        StarRewardScreen.Instance.Enable(TextTranslationModule.GetWord("Transaction successful"), stars);
        MapViewController.Instance.AddViewToClose(StarRewardScreen.Instance);
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
    {
        string title = "";
        string message = TextTranslationModule.GetWord("Transaction failed");

        UniversalInfoPopUp.Instance.Enable(title, message);
        MapViewController.Instance.AddViewToClose(UniversalInfoPopUp.Instance);
        
        reason.Log("Reason: ");
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        "IAP initialization failed".Log();
        error.ToString().Log();
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
    {
        AnalyticsController.Instance.LogEvent(Firebase.Analytics.FirebaseAnalytics.EventPurchase,
            new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterValue, purchaseEvent.purchasedProduct.definition.id),
            new Firebase.Analytics.Parameter(Firebase.Analytics.FirebaseAnalytics.ParameterTransactionId, purchaseEvent.purchasedProduct.transactionID)

            );
        switch (purchaseEvent.purchasedProduct.definition.id)
        {
            case "10_stars":
                PurchaseCompleteRewardStars(10, purchaseEvent.purchasedProduct.transactionID);
                break;
            case "20_stars":
                PurchaseCompleteRewardStars(20, purchaseEvent.purchasedProduct.transactionID);
                break;
            case "50_stars":
                PurchaseCompleteRewardStars(50, purchaseEvent.purchasedProduct.transactionID);
                break;
            case "100_stars":
                PurchaseCompleteRewardStars(100, purchaseEvent.purchasedProduct.transactionID);
                break;
        }
      
        return PurchaseProcessingResult.Complete;
        //throw new System.NotImplementedException();
    }


    public void PrepareShopForPlatform()
    {

    }
 
}
