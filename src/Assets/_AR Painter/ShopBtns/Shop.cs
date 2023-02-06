using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;


public class Shop: MonoBehaviour {
	public static Shop instance;

	public static readonly string KEY_SLOTS_AMOUNT = "slotsAmount";
	public static readonly int DEFAULT_SLOTS_AMOUNT =5; 


	public static readonly string KEY_BUY_SLOTS_5 = "BUY_SLOTS_5";
	public static readonly string KEY_BUY_SLOTS_25 = "BUY_SLOTS_25";
	public static readonly string KEY_BUY_SLOTS_100 = "BUY_SLOTS_100";
	public static readonly string KEY_BUY_SLOTS_INF = "BUY_SLOTS_INF";


	public static void AddSlot(int amount =1){
		Shop.slotsAmount += amount;
		UiController.instance.BuildSaveIconsGrid ();
		PlayerPrefs.Save ();
	}
	public static void AddInfSlots(){
		PlayerPrefs.Save ();
		UiController.instance.BuildSaveIconsGrid ();
	}

	public void PlusOneSlot(){
		AddSlot (1);
	}

	void Start(){
		instance = this;
	}

	public void BuySlots5(){
		Purchaser.instance.Buy5slots ();
	}
	public void BuySlots25(){
		Purchaser.instance.Buy25slots ();
	}
	public void BuySlots100(){
		Purchaser.instance.Buy100slots ();
	}
	public void BuySlotsInf(){
		Purchaser.instance.BuyInfslots ();
	}
		
	public static int slotsAmount{
		get{ 
			return PlayerPrefs.GetInt (Shop.KEY_SLOTS_AMOUNT, Shop.DEFAULT_SLOTS_AMOUNT);
		}
		set{ 
			PlayerPrefs.SetInt (Shop.KEY_SLOTS_AMOUNT, value);
		}
	}



	private void OnTransactionsRestored(bool success) {
		Debug.Log("Transactions restored " + success.ToString());
	}


	public void RestorePurchases(){
		Purchaser.instance.RestorePurchases ();
	}
}