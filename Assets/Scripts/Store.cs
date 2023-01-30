using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Store : MonoBehaviour
{
    public Transform tileContent, decorContent;

    public GameObject tilePanel, decorPanel, unlockTileButton, unlockDecorButton;
    public Image tileButtonBG, decorButtonBG;
    public Sprite blueBG, greyBG;

    GameManager gManager;
    // Start is called before the first frame update
    void Start()
    {
        gManager = FindObjectOfType<GameManager>();
        ToggleTilePanel();
        UpdateShop();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UnlockTile()
    {
        if (YsoCorp.GameUtils.YCManager.instance.adsManager.IsRewardBasedVideo())
        {
            YsoCorp.GameUtils.YCManager.instance.adsManager.ShowRewarded((bool ok) =>
            {
                if (ok)
                {
                    gManager.tilesLevel++;
                    gManager.SaveData();
                    UpdateShop();
                    if (gManager.tilesLevel > tileContent.childCount)
                        unlockTileButton.SetActive(false);
                    else
                        unlockTileButton.SetActive(true);
                }
            });
        }
    }
    public void UnlockDecor()
    {
        if (YsoCorp.GameUtils.YCManager.instance.adsManager.IsRewardBasedVideo())
        {
            YsoCorp.GameUtils.YCManager.instance.adsManager.ShowRewarded((bool ok) =>
            {
                if (ok)
                {
                    gManager.decorLevel++;
                    gManager.SaveData();
                    UpdateShop();
                    if (gManager.decorLevel > decorContent.childCount)
                        unlockDecorButton.SetActive(false);
                    else
                        unlockDecorButton.SetActive(true);
                }
            });
        }
    }

    public void ToggleTilePanel()
    {
        tilePanel.SetActive(true);
        decorPanel.SetActive(false);
        tileButtonBG.sprite = blueBG;
        decorButtonBG.sprite = greyBG;
        unlockDecorButton.SetActive(false);
        if (gManager.tilesLevel > tileContent.childCount)
            unlockTileButton.SetActive(false);
        else
            unlockTileButton.SetActive(true);
    }
    public void ToggleDecorPanel()
    {
        decorPanel.SetActive(true);
        tilePanel.SetActive(false);
        decorButtonBG.sprite = blueBG;
        tileButtonBG.sprite = greyBG;
        unlockTileButton.SetActive(false);
        if (gManager.decorLevel > decorContent.childCount)
            unlockDecorButton.SetActive(false);
        else
            unlockDecorButton.SetActive(true);
    }

    void UpdateShop()
    {
        for (int i = 0; i < gManager.tilesLevel; i++)
        {
            tileContent.GetChild(i).GetChild(0).gameObject.SetActive(true);
            tileContent.GetChild(i).GetChild(1).gameObject.SetActive(false);
            tileContent.GetChild(i).GetChild(2).gameObject.SetActive(false);
        }
        for (int i = 0; i < gManager.decorLevel; i++)
        {
            decorContent.GetChild(i).GetChild(0).GetComponent<Image>().color = Color.white;
            decorContent.GetChild(i).GetChild(1).gameObject.SetActive(false);
            decorContent.GetChild(i).GetChild(2).gameObject.SetActive(false);
        }
        gManager.ShopUpdated();
    }
}