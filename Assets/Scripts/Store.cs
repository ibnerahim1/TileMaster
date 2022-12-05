using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Store : MonoBehaviour
{
    public Button wallButton, floorButton, tableButton;
    public TextMeshProUGUI wallCostTxt, wallLevelTxt, floorCostTxt, floorLevelTxt, tableCostTxt, tableLevelTxt;

    GameManager gManager;
    // Start is called before the first frame update
    void Start()
    {
        gManager = FindObjectOfType<GameManager>();
        UpdateStore();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void LateUpdate()
    {
        wallButton.interactable = (gManager.cash > GetWallCost() && gManager.wallLevel < gManager.walls.childCount);
        floorButton.interactable = (gManager.cash > GetFloorCost() && gManager.floorLevel < gManager.floors.childCount);
        tableButton.interactable = (gManager.cash > GetTableCost() && gManager.tableLevel < gManager.tables.childCount);
    }
    public void Upgrade(string val)
    {
        gManager.PlaySound(GameManager.soundTypes.upgrade);

        switch (val)
        {
            case "wall":
                gManager.walls.GetChild(gManager.wallLevel - 1).gameObject.SetActive(false);
                gManager.cash -= GetWallCost();
                gManager.wallLevel++;
                gManager.walls.GetChild(gManager.wallLevel - 1).gameObject.SetActive(true);
                break;
            case "floor":
                gManager.floors.GetChild(gManager.floorLevel - 1).gameObject.SetActive(false);
                gManager.cash -= GetFloorCost();
                gManager.floorLevel++;
                gManager.floors.GetChild(gManager.floorLevel - 1).gameObject.SetActive(true);
                break;
            case "table":
                gManager.tables.GetChild(gManager.tableLevel - 1).gameObject.SetActive(false);
                gManager.cash -= GetTableCost();
                gManager.tableLevel++;
                gManager.tables.GetChild(gManager.tableLevel - 1).gameObject.SetActive(true);
                break;
        }
        UpdateStore();
        gManager.SaveData();
    }
    void UpdateStore()
    {
        wallLevelTxt.text = gManager.wallLevel < gManager.walls.childCount? "LEVEL " + gManager.wallLevel : "MAX";
        floorLevelTxt.text = gManager.floorLevel < gManager.floors.childCount ? "LEVEL " + gManager.floorLevel : "MAX";
        tableLevelTxt.text = gManager.tableLevel < gManager.tables.childCount ? "LEVEL " + gManager.tableLevel : "MAX";

        wallCostTxt.text = gManager.GetValue(GetWallCost());
        floorCostTxt.text = gManager.GetValue(GetFloorCost());
        tableCostTxt.text = gManager.GetValue(GetTableCost());
    }

    public int GetWallCost()
    {
        return (int)Mathf.Pow((gManager.wallLevel) * 10, 2);
    }
    public int GetFloorCost()
    {
        return (int)Mathf.Pow((gManager.floorLevel) * 10, 2);
    }
    public int GetTableCost()
    {
        return (int)Mathf.Pow((gManager.tableLevel) * 10, 2);
    }

    public string GetValue(float val)
    {
        string str = null;

        if (val < 1000)
            str = Mathf.RoundToInt(val).ToString();
        else if (val < 1000000)
            str = Round2Frac(val / 1000) + "K";
        else if (val < 1000000000)
            str = Round2Frac(val / 1000000) + "M";
        else if (val < 1000000000000)
            str = Round2Frac(val / 1000000000) + "B";

        return str;
    }
    string Round2Frac(float val)
    {
        string str = null;

        if (Mathf.Round(val) < 10)
        {
            str = (Mathf.Round(val * 100) / 100).ToString();
        }
        else if (Mathf.Round(val) < 100)
        {
            str = (Mathf.Round(val * 10) / 10).ToString();
        }
        else
            str = Mathf.Round(val).ToString();

        return str;
    }
}