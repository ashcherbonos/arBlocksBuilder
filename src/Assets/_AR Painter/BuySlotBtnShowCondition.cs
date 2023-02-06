using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuySlotBtnShowCondition : MonoBehaviour
{

    public bool plus_5;
    public bool plus_25;
    public bool plus_100;
    public bool plus_inf;

    void OnEnable()
    {
        if (plus_5 && PlayerPrefs.GetInt(Shop.KEY_BUY_SLOTS_5) == 1)
        {
            gameObject.SetActive(true);
            return;
        }

        if (plus_25 && PlayerPrefs.GetInt(Shop.KEY_BUY_SLOTS_25) == 1)
        {
            gameObject.SetActive(true);
            return;
        }

        if (plus_100 && PlayerPrefs.GetInt(Shop.KEY_BUY_SLOTS_100) == 1)
        {
            gameObject.SetActive(true);
            return;
        }

        if (plus_inf && PlayerPrefs.GetInt(Shop.KEY_BUY_SLOTS_INF) == 1)
        {
            gameObject.SetActive(true);
            return;
        }

        gameObject.SetActive(false);
    }
}
