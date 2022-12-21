using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MoreMountains.NiceVibrations;
using UnityEditor;

public class GameManager : MonoBehaviour
{
    public enum hapticTypes {soft, light, medium, heavy, success, failure};
    public enum soundTypes { pop, upgrade, money, coins, tap, win};
    public Transform walls, floors, tables, tileFX;
    public Transform[] audioClips;
    public Tile[] tiles;
    public int touchCount;

    [SerializeField] Transform progressBar, camPositions, tilesContainer, sampleCam, levelParent, levelObj, tileObj, patternsParent, removeTileImg;
    [SerializeField] GameObject winPanel, menuPanel, gamePanel, storePanel, cashPanel, whiteScreen, nextButton, tilesSelection, orderPanel, returnDoneButton, sampleButton, sampleFullButton;
    [SerializeField] TextMeshProUGUI levelTxt, cashTxt, orderCashTxt, fillPercent;
    [SerializeField] Customer customer;
    [SerializeField] GameObject[] stars;
    [SerializeField] Image tileFill;
    [SerializeField] Material houseMat, floorMat;
    [SerializeField] Transform hand;
    [SerializeField] Texture2D hand1;

    [HideInInspector] public bool gameStarted, remove;
    [HideInInspector] public int level, wallLevel, floorLevel, tableLevel, cash, orderCash;

    public List<Tile> currentMats = new List<Tile>();
    public List<TipRenderer> placedTiles = new List<TipRenderer>();
    public Transform currentpattern, currentLevel;
    public int[] TargetTiles = new int[100], tilesSpawned = new int[100];
    private Transform tile;
    private const int camTween = 0;
    private Camera cam;
    private float popPitch = 0.5f, matchPercentage;
    private int tileIndex;
    private bool removeTileTut;

    private void Awake()
    {
        LoadData();
    }
    private void Start()
    {
#if UNITY_EDITOR
        Cursor.SetCursor(PlayerSettings.defaultCursor, new Vector2(35, 35), CursorMode.ForceSoftware);
#endif
        cam = Camera.main;
        level = PlayerPrefs.HasKey("level") ? PlayerPrefs.GetInt("level") : 1;
        levelTxt.text = "LEVEL " + level;

        for (int i = 0; i < (level - 1) % 5; i++)
        {
            progressBar.GetChild(i).GetChild(0).gameObject.SetActive(true);
        }
        progressBar.GetChild((level - 1) % 5).GetChild(1).gameObject.SetActive(true);
        cam.transform.DOMove(camPositions.GetChild(0).position, 0.5f).SetEase(Ease.Linear).SetId(camTween).SetDelay(0.5f);
        cam.transform.DORotate(camPositions.GetChild(0).eulerAngles, 0.5f).SetEase(Ease.Linear).SetId(camTween).SetDelay(0.5f);

        //walls.GetChild(0).gameObject.SetActive(false);
        //floors.GetChild(0).gameObject.SetActive(false);
        //tables.GetChild(0).gameObject.SetActive(false);
        //walls.GetChild(wallLevel - 1).gameObject.SetActive(true);
        //floors.GetChild(floorLevel - 1).gameObject.SetActive(true);
        //tables.GetChild(tableLevel - 1).gameObject.SetActive(true);

        InitLevel(-1);
        InitLevel(0);
        InitLevel(1);
    }

    void Update()
    {
        #region MyDebug
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(1))
            Restart();
        if (Input.GetKeyDown("b"))
            Debug.Break();
        if (Input.GetKeyDown("d"))
            PlayerPrefs.DeleteKey("level");
        if (Input.GetKeyDown("d") && Input.GetKey(KeyCode.LeftShift))
            PlayerPrefs.DeleteAll();
        if (Input.GetKeyDown("n"))
            PlayerPrefs.SetInt("level", PlayerPrefs.GetInt("level") + 1);
#endif
        #endregion
        cashTxt.text = GetValue(cash);
        if (popPitch > 0.5f)
            popPitch = Mathf.Lerp(popPitch, 0.5f, Time.deltaTime);
        //hand.localPosition = cam.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButtonDown(0))
            Cursor.SetCursor(hand1, new Vector2(35, 35), CursorMode.ForceSoftware);
        if(Input.GetMouseButtonUp(0))
            Cursor.SetCursor(PlayerSettings.defaultCursor, new Vector2(35, 35), CursorMode.ForceSoftware);

            //StartCoroutine(HandClick());
    }
    //IEnumerator HandClick()
    //{
    //    yield return new WaitForSeconds(0.1f);
    //}

    void InitLevel(int order)
    {
        int l = level + order;
        Random.State state = Random.state;
        Random.InitState(l);

        if (order < 1)
        {
            List<Tile> tempMats = new List<Tile>();
            currentMats.Clear();
            for (int i = 0; i < Mathf.Min((l / 5) + 2, tiles.Length); i++)
            {
                tempMats.Add(tiles[i]);
            }
            int b = Mathf.Min(tempMats.Count, tilesContainer.childCount);
            for (int i = 0; i < b; i++)
            {
                if (l < 10 || l / 5 == 0)
                {
                    currentMats.Add(tempMats[tempMats.Count - 1]);
                    tempMats.RemoveAt(tempMats.Count - 1);
                }
                else
                {
                    int r = Random.Range(0, tempMats.Count);
                    currentMats.Add(tempMats[r]);
                    tempMats.RemoveAt(r);
                }
            }
            //shuffle currentMats
            currentMats.Sort((a, b) => 1 - 2 * Random.Range(0, 2));
            if (order == 0)
            {
                for (int i = 0; i < tilesContainer.childCount; i++)
                {
                    if(i < currentMats.Count)
                        tilesContainer.GetChild(i).GetChild(0).GetComponent<Image>().sprite = currentMats[i].sprite;
                    else
                        tilesContainer.GetChild(i).GetComponent<Button>().interactable = false;
                }
            }
            List<int> tempPattern = new List<int>();
            for (int i = 0; i < Mathf.Min((l / 2) + 1, patternsParent.childCount); i++)
            {
                tempPattern.Add(i);
            }
            currentpattern = patternsParent.GetChild(l < 20? tempPattern[tempPattern.Count - 1]: tempPattern[Random.Range(0, tempPattern.Count)]);
            currentLevel = Instantiate(levelObj, Vector3.forward * 10.5f * order, Quaternion.identity, levelParent);
            currentLevel.GetChild(0).GetChild(Random.Range(0, currentLevel.GetChild(0).childCount)).gameObject.SetActive(true);
            if (order == -1)
            {
                MeshRenderer[] rends = currentLevel.GetChild(0).GetComponentsInChildren<MeshRenderer>();
                for (int i = 0; i < rends.Length; i++)
                {
                    rends[i].material = houseMat;
                }
                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        int a = int.Parse(currentpattern.GetChild((i * 10) + j).name);
                        while (a > currentMats.Count - 1)
                            a -= currentMats.Count;
                        tile = Instantiate(tileObj, new Vector3(j - 9.5f, 0, i - 15f), Quaternion.identity, currentLevel);
                        tile.GetComponent<MeshRenderer>().material = currentMats[a].mat;
                    }
                }
            }
        }
        else
        {
            Transform temp = Instantiate(levelObj, Vector3.forward * 10.5f * order, Quaternion.identity, levelParent);
            temp.GetChild(0).GetChild(Random.Range(0, temp.GetChild(0).childCount)).gameObject.SetActive(true);
            temp.GetComponent<MeshRenderer>().material = floorMat;
        }
        Random.state = state;
    }

    public void RemoveTiles(bool condn)
    {
        if (condn)
        {
            if (removeTileTut)
            {
                removeTileTut = false;
                DOTween.Kill(2);
            }
            tilesSelection.transform.DOLocalMoveX(1000, 0.25f).SetEase(Ease.Linear).OnComplete(()=>
            {
                returnDoneButton.SetActive(true);
                tilesSelection.SetActive(false);
            });
            for (int i = 0; i < currentLevel.GetChild(1).childCount; i++)
            {
                currentLevel.GetChild(1).GetChild(i).GetComponent<TipRenderer>().PauseHighlight();
            }
            for (int i = 0; i < placedTiles.Count; i++)
            {
                placedTiles[i].Initialise();
            }
            nextButton.SetActive(false);
        }
        else
        {
            returnDoneButton.SetActive(false);
            tilesSelection.SetActive(true);
            tilesSelection.transform.DOLocalMoveX(0, 0.25f).SetEase(Ease.Linear);
            for (int i = 0; i < currentLevel.GetChild(1).childCount; i++)
            {
                if(currentLevel.GetChild(1).GetChild(i).GetComponent<TipRenderer>().active)
                    currentLevel.GetChild(1).GetChild(i).GetComponent<TipRenderer>().Initialise();
            }
            if (touchCount > 99)
                nextButton.SetActive(true);
        }
        remove = condn;
    }
    public void TileSelection(int index)
    {
        PlaySound(soundTypes.tap);

        tileIndex = index;
        
        //tilesSelection.SetActive(false);
        DOTween.Kill(camTween);
        cam.transform.DOMove(camPositions.GetChild(2).position, 0.5f).SetEase(Ease.Linear).SetId(camTween);
        cam.transform.DORotate(camPositions.GetChild(2).eulerAngles, 0.5f).SetEase(Ease.Linear).SetId(camTween);
    }
    public void Touched(Vector3 position)
    {
        PlaySound(soundTypes.pop);

        touchCount++;

        tile = Instantiate(tileObj, position + new Vector3(0, 0.5f, 0), Quaternion.identity, currentLevel);
        tile.GetComponent<MeshRenderer>().material = currentMats[tileIndex].mat;
        tile.DOMoveY(0, 0.25f).SetEase(Ease.Linear);
        tilesSpawned[(int)(((position.z + 4.5f) * 10) + (position.x + 9.5f))] = tileIndex;
        placedTiles.Add(tile.GetComponent<TipRenderer>());
        if (touchCount > 99)
            nextButton.SetActive(true);
        if (level < 3 && TargetTiles[(int)(((position.z + 4.5f) * 10) + (position.x + 9.5f))] != tileIndex && !removeTileTut)
        {
            removeTileTut = true;
            removeTileImg.DOScale(1.2f, 0.25f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo).SetId(2);
        }
        //if (TargetTiles[(int)(((position.z + 4.5f) * 10) + (position.x + 9.5f))] != tileIndex && Input.GetKey("t"))
        //    tile.GetComponent<MeshRenderer>().material.color = Color.black;
    }

    public IEnumerator makeSample()
    {
        orderCash = 150 + (level * 50);
        orderPanel.SetActive(true);
        orderCashTxt.text = GetValue(orderCash);
        sampleCam.GetComponent<Camera>().orthographicSize = 6;

        yield return new WaitForSeconds(0.25f);

        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                yield return new WaitForSeconds(0.01f);
                int a = int.Parse(currentpattern.GetChild((i * 10) + j).name);
                while (a > currentMats.Count - 1)
                    a -= currentMats.Count;
                tile = Instantiate(tileObj, new Vector3(j - 9.5f + 20, -10, i - 4.5f), Quaternion.identity, currentLevel);
                tile.GetComponent<MeshRenderer>().material = currentMats[a].mat;
                TargetTiles[(i * 10) + j] = a;
            }
        }

        DOTween.Kill(camTween);
        cam.transform.DOMove(camPositions.GetChild(2).position, 0.5f).SetEase(Ease.Linear).SetId(camTween);
        cam.transform.DORotate(camPositions.GetChild(2).eulerAngles, 0.5f).SetEase(Ease.Linear).SetId(camTween).OnComplete(() =>
        {

            for (int i = 0; i < currentLevel.GetChild(1).childCount; i++)
            {
                currentLevel.GetChild(1).GetChild(i).GetComponent<TipRenderer>().Initialise();
            }
            tilesSelection.SetActive(true);
        });
    }


    public void PlayHaptic(hapticTypes hType)
    {
        switch (hType)
        {
            case hapticTypes.soft:
                MMVibrationManager.Haptic(HapticTypes.SoftImpact);
                break;
            case hapticTypes.light:
                MMVibrationManager.Haptic(HapticTypes.LightImpact);
                break;
            case hapticTypes.medium:
                MMVibrationManager.Haptic(HapticTypes.MediumImpact);
                break;
            case hapticTypes.heavy:
                MMVibrationManager.Haptic(HapticTypes.HeavyImpact);
                break;
            case hapticTypes.success:
                MMVibrationManager.Haptic(HapticTypes.Success);
                break;
            case hapticTypes.failure:
                MMVibrationManager.Haptic(HapticTypes.Failure);
                break;
        }
    }
    public void PlaySound(soundTypes sType)
    {
        //Destroy(soundIns.gameObject, 1);
        switch (sType)
        {
            case soundTypes.pop:
                Transform temp = Instantiate(audioClips[0]);
                temp.GetComponent<AudioSource>().pitch = popPitch;
                popPitch += 0.05f;
                break;
            case soundTypes.upgrade:
                Instantiate(audioClips[1]);
                break;
            case soundTypes.money:
                Instantiate(audioClips[2]);
                break;
            case soundTypes.coins:
                Instantiate(audioClips[0]);
                break;
            case soundTypes.tap:
                Instantiate(audioClips[3]);
                break;
            case soundTypes.win:
                Instantiate(audioClips[4]);
                break;
        }
    }

    public string GetValue(float val)
    {
        //return (Mathf.Round(val * (Mathf.Abs(val) < 1000 ? 10 : 0.1f)) / (Mathf.Abs(val) < 1000 ? 10 : 100)).ToString() + (Mathf.Abs(val) < 1000 ? "M" : "B");
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
    public void ToggleStore(bool condition)
    {
        PlaySound(soundTypes.tap);

        if (condition)
        {
            DOTween.Kill(camTween);
            cam.transform.DOMove(camPositions.GetChild(3).position, 0.5f).SetEase(Ease.Linear).SetId(camTween);
            cam.transform.DORotate(camPositions.GetChild(3).eulerAngles, 0.5f).SetEase(Ease.Linear).SetId(camTween);
            storePanel.SetActive(true);
            menuPanel.SetActive(false);
        }
        else
        {
            DOTween.Kill(camTween);
            cam.transform.DOMove(camPositions.GetChild(0).position, 0.5f).SetEase(Ease.Linear).SetId(camTween);
            cam.transform.DORotate(camPositions.GetChild(0).eulerAngles, 0.5f).SetEase(Ease.Linear).SetId(camTween);
            storePanel.SetActive(false);
            menuPanel.SetActive(true);
        }
    }

    public void PlayLevel()
    {
        PlaySound(soundTypes.tap);

        gameStarted = true;
        menuPanel.SetActive(false);
        gamePanel.SetActive(true);
        cashPanel.SetActive(false);
        DOTween.Kill(camTween);
        cam.transform.DOMove(camPositions.GetChild(1).position, 0.5f).SetEase(Ease.Linear).SetId(camTween);
        cam.transform.DORotate(camPositions.GetChild(1).eulerAngles, 0.5f).SetEase(Ease.Linear).SetId(camTween).OnComplete(()=>StartCoroutine(makeSample()));
    }
    public void Next()
    {
        PlaySound(soundTypes.win);

        orderPanel.SetActive(false);
        nextButton.SetActive(false);
        cam.transform.GetChild(0).gameObject.SetActive(true);
        tilesSelection.SetActive(false);

        cashPanel.SetActive(true);
        gamePanel.SetActive(false);

        DOTween.Kill(camTween);
        cam.transform.DOMove(camPositions.GetChild(0).position, 0.5f).SetEase(Ease.Linear).SetId(camTween);
        cam.transform.DORotate(camPositions.GetChild(0).eulerAngles, 0.5f).SetEase(Ease.Linear).SetId(camTween);
        StartCoroutine(LevelComplete());
    }

    public void LoadData()
    {
        cash = PlayerPrefs.HasKey("cash") ? PlayerPrefs.GetInt("cash") : 0;
        wallLevel = PlayerPrefs.HasKey("wallLevel") ? PlayerPrefs.GetInt("wallLevel") : 1;
        floorLevel = PlayerPrefs.HasKey("floorLevel") ? PlayerPrefs.GetInt("floorLevel") : 1;
        tableLevel = PlayerPrefs.HasKey("tableLevel") ? PlayerPrefs.GetInt("tableLevel") : 1;
    }
    public void SaveData()
    {
        PlayerPrefs.SetInt("level", level);
        PlayerPrefs.SetInt("cash", cash);

        PlayerPrefs.SetInt("wallLevel", wallLevel);
        PlayerPrefs.SetInt("floorLevel", floorLevel);
        PlayerPrefs.SetInt("tableLevel", tableLevel);
    }

    IEnumerator LevelComplete()
    {
        gameStarted = false;
        cashPanel.SetActive(true);

        level++;
        PlayerPrefs.SetInt("level", level);

        int MatchCount = 0;

        for (int i = 0; i < tilesSpawned.Length; i++)
        {
            if (tilesSpawned[i] == TargetTiles[i])
                MatchCount++;
        }

        yield return new WaitForSeconds(0.5f);

        MeshRenderer[] rends = currentLevel.GetChild(0).GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < rends.Length; i++)
        {
            rends[i].material = houseMat;
        }

        PlayHaptic(hapticTypes.success);
        PlaySound(soundTypes.money);

        if(MatchCount< 25)
        {
            customer.anim.Play("angry");
        }
        else if(MatchCount < 50)
        {
            customer.anim.Play("talk");
            orderCash = (int)(orderCash * 1.1f);
        }
        else if (MatchCount < 75)
        {
            customer.anim.Play("happy");
            orderCash = (int)(orderCash * 1.2f);
        }
        else
        {
            customer.anim.Play("jump");
            orderCash = (int)(orderCash * 1.3f);
        }
        GetComponent<CoinMagnet>().SpawnCoins((int)(orderCash * 0.1f));
        cash += orderCash;
        SaveData();

        yield return new WaitForSeconds(2);

        gamePanel.SetActive(false);
        cashPanel.SetActive(false);
        winPanel.SetActive(true);

        yield return new WaitForSeconds(1.5f);

        if (MatchCount > 24)
        {
            if (MatchCount < 50)
            {
                stars[0].SetActive(true);
            }
            else if (MatchCount < 75)
            {
                stars[0].SetActive(true);
                yield return new WaitForSeconds(0.5f);
                stars[1].SetActive(true);
            }
            else
            {
                stars[0].SetActive(true);
                yield return new WaitForSeconds(0.5f);
                stars[1].SetActive(true);
                yield return new WaitForSeconds(0.5f);
                stars[2].SetActive(true);
            }
        }
        float f = (float)((level - 1) % 5) / 5;
        float f1 = (float)((level - 2) % 5) / 5;
        float f2 = f == 0 ? 1 : f;
        tileFill.DOFillAmount(f2, 0.5f).SetEase(Ease.Linear).From(f1 == 1 ? 0 : f1);
        fillPercent.text = (f2 * 100) + "%".ToString();
        tileFill.sprite = tiles[(level / 5) + 2].sprite;
    }

    public void Restart()
    {
        PlaySound(soundTypes.tap);

        whiteScreen.GetComponent<Image>().DOFade(1, 0.5f).SetEase(Ease.Linear).OnComplete(()=> SceneManager.LoadScene(0));
    }
    public void SampleFullScreen(bool condn)
    {
        if (condn)
        {
            sampleButton.SetActive(false);
            sampleFullButton.SetActive(true);
        }
        else
        {
            sampleButton.SetActive(true);
            sampleFullButton.SetActive(false);
        }

    }
}
[System.Serializable]
public class Tile
{
    public Material mat;
    public Sprite sprite;
}