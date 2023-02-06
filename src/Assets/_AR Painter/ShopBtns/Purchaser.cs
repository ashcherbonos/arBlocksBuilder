using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Purchasing;


// Deriving the Purchaser class from IStoreListener enables it to receive messages from Unity Purchasing.
public class Purchaser : MonoBehaviour, IStoreListener
{
    public static Purchaser instance;
    private static IStoreController m_StoreController;                                                                  // Reference to the Purchasing system.
    private static IExtensionProvider m_StoreExtensionProvider;                                                         // Reference to store-specific Purchasing subsystems.

    // Product identifiers for all products capable of being purchased: "convenience" general identifiers for use with Purchasing, and their store-specific identifier counterparts 
    // for use with and outside of Unity Purchasing. Define store-specific identifiers also on each platform's publisher dashboard (iTunes Connect, Google Play Developer Console, etc.)


	public static string kProductIDslots5 = "com.captainflintgames.arpaint.iap.slots5";
	public static string kProductIDslots25 = "com.captainflintgames.arpaint.iap.slots25";
	public static string kProductIDslots100 = "com.captainflintgames.arpaint.iap.slots100";
	public static string kProductIDslotsInf = "com.captainflintgames.arpaint.iap.slotsinfinity";

    void Start()
    {
		instance = this;

        // If we haven't set up the Unity Purchasing reference
        if (m_StoreController == null)
        {
            // Begin to configure our connection to Purchasing
            InitializePurchasing();
        }


    }

    public void InitializePurchasing()
    {
        // If we have already connected to Purchasing ...
        if (IsInitialized())
        {
            // ... we are done here.
            return;
        }

        // Create a builder, first passing in a suite of Unity provided stores.
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        // Add a product to sell / restore by way of its identifier, associating the general identifier with its store-specific identifiers.
		builder.AddProduct(kProductIDslots5, ProductType.Consumable, new IDs() { { kProductIDslots5, AppleAppStore.Name }, { kProductIDslots5, GooglePlay.Name }, });// Continue adding the non-consumable product.
		builder.AddProduct(kProductIDslots25, ProductType.Consumable, new IDs() { { kProductIDslots25, AppleAppStore.Name }, { kProductIDslots25, GooglePlay.Name }, });// Continue adding the non-consumable product.
		builder.AddProduct(kProductIDslots100, ProductType.Consumable, new IDs() { { kProductIDslots100, AppleAppStore.Name }, { kProductIDslots100, GooglePlay.Name }, });// Continue adding the non-consumable product.
		builder.AddProduct(kProductIDslotsInf, ProductType.NonConsumable, new IDs() { { kProductIDslotsInf, AppleAppStore.Name }, { kProductIDslotsInf, GooglePlay.Name }, });// Continue adding the non-consumable product.

        UnityPurchasing.Initialize(this, builder);
    }


    private bool IsInitialized()
    {
        // Only say we are initialized if both the Purchasing references are set.
        return m_StoreController != null && m_StoreExtensionProvider != null;
    }







    void BuyProductID(string productId)
    {
        // If the stores throw an unexpected exception, use try..catch to protect my logic here.
        try
        {
            // If Purchasing has been initialized ...
            if (IsInitialized())
            {
                // ... look up the Product reference with the general product identifier and the Purchasing system's products collection.
                Product product = m_StoreController.products.WithID(productId);

                // If the look up found a product for this device's store and that product is ready to be sold ... 
                if (product != null && product.availableToPurchase)
                {
                    Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));// ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed asynchronously.
                    m_StoreController.InitiatePurchase(product);
                    
                }
                // Otherwise ...
                else
                {
                    // ... report the product look-up failure situation  
                    Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                }
            }
            // Otherwise ...
            else
            {
                // ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or retrying initiailization.
                Debug.Log("BuyProductID FAIL. Not initialized.");
            }
        }
        // Complete the unexpected exception handling ...
        catch (Exception e)
        {
            // ... by reporting any unexpected exception for later diagnosis.
            Debug.Log("BuyProductID: FAIL. Exception during purchase. " + e);
        }
    }


    // Restore purchases previously made by this customer. Some platforms automatically restore purchases. Apple currently requires explicit purchase restoration for IAP.
	public void RestorePurchases()
    {
        // If Purchasing has not yet been set up ...
        if (!IsInitialized())
        {
            // ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
            Debug.Log("RestorePurchases FAIL. Not initialized.");
            return;
        }

        // If we are running on an Apple device ... 
        if (Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.OSXPlayer)
        {
            // ... begin restoring purchases
            Debug.Log("RestorePurchases started ...");

            // Fetch the Apple store-specific subsystem.
            var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
            // Begin the asynchronous process of restoring purchases. Expect a confirmation response in the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
            apple.RestoreTransactions((result) =>
            {
                // The first phase of restoration. If no more responses are received on ProcessPurchase then no purchases are available to be restored.
                Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
            });
        }
        // Otherwise ...
        else
        {
            // We are not running on an Apple device. No work is necessary to restore purchases.
            Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
        }
    }


    //  
    // --- IStoreListener
    //

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        // Purchasing has succeeded initializing. Collect our Purchasing references.
        Debug.Log("OnInitialized: PASS");

        // Overall Purchasing system, configured with products for this application.
        m_StoreController = controller;
        // Store specific subsystem, for accessing device-specific store features.
        m_StoreExtensionProvider = extensions;

       // showLocalizedData();
    }


    public void OnInitializeFailed(InitializationFailureReason error)
    {
        // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
        Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);

        //setAllProductPanelsDisable();
    }


    


    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        // A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing this reason with the user.
        Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
    }
    



    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
		if (args.purchasedProduct.definition.id == kProductIDslots5) {
			PlayerPrefs.SetInt (Shop.KEY_BUY_SLOTS_5, 1);
			Shop.AddSlot (5);
		}
		if (args.purchasedProduct.definition.id == kProductIDslots25) {
			PlayerPrefs.SetInt (Shop.KEY_BUY_SLOTS_25, 1);
			Shop.AddSlot (25);
		}
		if (args.purchasedProduct.definition.id == kProductIDslots100) {
			PlayerPrefs.SetInt (Shop.KEY_BUY_SLOTS_100, 1);
			Shop.AddSlot (100);
		}
		if (args.purchasedProduct.definition.id == kProductIDslotsInf) {
			PlayerPrefs.SetInt (Shop.KEY_BUY_SLOTS_INF, 1);
			Shop.AddInfSlots ();
		}

        return PurchaseProcessingResult.Complete;
    }
    

    public void Buy5slots()
    {
		BuyProductID(kProductIDslots5);
    }
	public void Buy25slots()
	{
		BuyProductID(kProductIDslots25);
	}
	public void Buy100slots()
	{
		BuyProductID(kProductIDslots100);
	}
	public void BuyInfslots()
	{
		BuyProductID(kProductIDslotsInf);
	}


    void showLocalizedData()
    {
        //noAdsBtn.showLocalizedMetadata(m_StoreController.products.WithID(kProductIDnoAds).metadata);
        
    }

    void setAllProductPanelsDisable() {
        //noAdsBtn.disable();
        
    }
    
}