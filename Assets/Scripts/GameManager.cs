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
    public Transform tileRemoveFX;
    public Transform[] audioClips;
    public TileData[] tileDatas;
    public int touchCount, currentPattern;
    public int[,,] patterns;

    [SerializeField] Transform progressBar, camPositions, tilesContainer, sampleCam, levelParent, levelObj, tileObj, removeTileImg;
    [SerializeField] GameObject winPanel, menuPanel, gamePanel, storePanel, cashPanel, whiteScreen, nextButton, tilesSelection, orderPanel, returnDoneButton, sampleButton, sampleFullButton;
    [SerializeField] TextMeshProUGUI levelTxt, cashTxt, orderCashTxt, fillPercent;
    [SerializeField] Customer customer;
    [SerializeField] GameObject[] stars;
    [SerializeField] Image nextTileFill;
    [SerializeField] Material houseMat, floorMat;
    [SerializeField] Texture2D handCursor;

    [HideInInspector] public bool gameStarted, remove;
    [HideInInspector] public int level, wallLevel, floorLevel, tableLevel, cash, orderCash;

    public List<TileData> currentMats = new List<TileData>();
    public Tile[,] tiles = new Tile[5,5];
    public Transform currentLevel;
    public int[,] TargetTiles = new int[5,5], tilesSpawned = new int[5,5];
    private Transform tile;
    private const int camTween = 0;
    private Camera cam;
    private float popPitch = 0.5f;
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
        level = PlayerPrefs.HasKey("level") ? PlayerPrefs.GetInt("level") : 0;
        levelTxt.text = "LEVEL " + (level + 1);

        for (int i = 0; i < level % 5; i++)
        {
            progressBar.GetChild(i).GetChild(0).gameObject.SetActive(true);
        }
        progressBar.GetChild(level % 5).GetChild(1).gameObject.SetActive(true);
        cam.transform.DOMove(camPositions.GetChild(0).position, 0.5f).SetEase(Ease.Linear).SetId(camTween).SetDelay(0.5f);
        cam.transform.DORotate(camPositions.GetChild(0).eulerAngles, 0.5f).SetEase(Ease.Linear).SetId(camTween).SetDelay(0.5f);

        nextTileFill.fillAmount = (float)(level % 3) / 3;
        nextTileFill.sprite = tileDatas[(level / 3) + 2].sprite;
        fillPercent.text = (level%3) == 0? "30%" : ((level % 3) == 1 ? "60%" : "100%");

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
        if (Input.GetKeyDown("l"))
            StartCoroutine(LevelComplete());
#endif
        #endregion
        cashTxt.text = GetValue(cash);
        if (popPitch > 0.5f)
            popPitch = Mathf.Lerp(popPitch, 0.5f, Time.deltaTime);
        if (Input.GetMouseButtonDown(0))
            Cursor.SetCursor(handCursor, new Vector2(35, 35), CursorMode.ForceSoftware);
        if(Input.GetMouseButtonUp(0))
            Cursor.SetCursor(PlayerSettings.defaultCursor, new Vector2(35, 35), CursorMode.ForceSoftware);

    }

    void InitLevel(int order)
    {
        int l = level + order;
        Random.State state = Random.state;
        Random.InitState(l);

        if (order < 1)
        {
            List<TileData> tempMats = new List<TileData>();
            currentMats.Clear();
            for (int i = 0; i < Mathf.Min((l / 3) + 2, tileDatas.Length); i++)
            {
                tempMats.Add(tileDatas[i]);
            }
            int b = Mathf.Min(tempMats.Count, tilesContainer.childCount);
            for (int i = 0; i < b; i++)
            {
                if (l < 10 || l / 3 == 0)
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

            currentLevel = Instantiate(levelObj, Vector3.forward * 10.5f * order, Quaternion.identity, levelParent);
            currentLevel.GetChild(0).GetChild(Random.Range(0, currentLevel.GetChild(0).childCount)).gameObject.SetActive(true);

            if (order == 0)
            {
                for (int i = 0; i < tilesContainer.childCount; i++)
                {
                    if(i < currentMats.Count)
                        tilesContainer.GetChild(i).GetChild(0).GetComponent<Image>().sprite = currentMats[i].sprite;
                    else
                        tilesContainer.GetChild(i).GetComponent<Button>().interactable = false;
                }
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        tiles[i, j] = Instantiate(tileObj, new Vector3((i * 2) - 9, 0, (j * 2) - 4f), Quaternion.identity, currentLevel).GetComponent<Tile>();
                        tiles[i, j].isTile = false;
                        tiles[i, j].index = new Vector2Int(i, j);
                        tiles[i, j].transform.localScale = new Vector3(1.8f, 1, 1.8f);
                    }
                }
            }
            List<int> tempPattern = new List<int>();
            for (int i = 0; i < Mathf.Min((l / 2) + 1, 10); i++)
            {
                tempPattern.Add(i);
            }
            currentPattern = l < 20 ? tempPattern[l%tempPattern.Count] : tempPattern[Random.Range(0, tempPattern.Count)];

            if (order == -1)
            {
                MeshRenderer[] rends = currentLevel.GetChild(0).GetComponentsInChildren<MeshRenderer>();
                for (int i = 0; i < rends.Length; i++)
                {
                    rends[i].material = houseMat;
                }
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        int a = patterns[currentPattern, i, j];
                        while (a > currentMats.Count - 1)
                            a -= currentMats.Count;
                        tile = Instantiate(tileObj, new Vector3((i * 2) - 9, 0, (j * 2) - 14.5f), Quaternion.identity, currentLevel);
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
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (tiles[i, j].isTile)
                        tiles[i, j].Initialise();
                    else
                        tiles[i, j].PauseHighlight();
                }
            }
            nextButton.SetActive(false);
        }
        else
        {
            returnDoneButton.SetActive(false);
            tilesSelection.SetActive(true);
            tilesSelection.transform.DOLocalMoveX(0, 0.25f).SetEase(Ease.Linear);
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if(!tiles[i, j].isTile)
                        tiles[i, j].Initialise();
                }
            }
            if (touchCount > 24)
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
    public void Touched(Vector2Int tIndex)
    {
        PlaySound(soundTypes.pop);

        touchCount++;
        tiles[tIndex.x, tIndex.y].isTile = true;
        tiles[tIndex.x, tIndex.y].transform.localScale = new Vector3(2, 1, 2);
        tiles[tIndex.x, tIndex.y].transform.position += new Vector3(0, 0.5f, 0);
        tiles[tIndex.x, tIndex.y].GetComponent<MeshRenderer>().material = currentMats[tileIndex].mat;
        tiles[tIndex.x, tIndex.y].transform.DOMoveY(0, 0.25f).SetEase(Ease.Linear);
        tilesSpawned[tIndex.x, tIndex.y] = tileIndex;

        if (touchCount > 24)
            nextButton.SetActive(true);
        if (level < 3 && TargetTiles[tIndex.x, tIndex.y] != tileIndex && !removeTileTut)
        {
            removeTileTut = true;
            removeTileImg.DOScale(1.2f, 0.25f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo).SetId(2);
        }
        if (TargetTiles[tIndex.x, tIndex.y] != tileIndex && Input.GetKey("t"))
            tiles[tIndex.x, tIndex.y].GetComponent<MeshRenderer>().material.color = Color.black;
    }

    public IEnumerator makeSample()
    {
        orderCash = 150 + (level * 50);
        orderPanel.SetActive(true);
        orderCashTxt.text = GetValue(orderCash);
        sampleCam.GetComponent<Camera>().orthographicSize = 6;

        yield return new WaitForSeconds(0.25f);

        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                yield return new WaitForSeconds(0.01f);
                int a = patterns[currentPattern, i, j];
                while (a > currentMats.Count - 1)
                    a -= currentMats.Count;
                tile = Instantiate(tileObj, new Vector3((i * 2) - 9 + 20, -10, (j * 2) - 4), Quaternion.identity, currentLevel);
                tile.GetComponent<MeshRenderer>().material = currentMats[a].mat;
                TargetTiles[i, j] = a;
            }
        }

        DOTween.Kill(camTween);
        cam.transform.DOMove(camPositions.GetChild(2).position, 0.5f).SetEase(Ease.Linear).SetId(camTween);
        cam.transform.DORotate(camPositions.GetChild(2).eulerAngles, 0.5f).SetEase(Ease.Linear).SetId(camTween).OnComplete(() =>
        {
            gameStarted = true;
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    tiles[i, j].Initialise();
                }
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

        CreatePatterns();
    }
    public void SaveData()
    {
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

        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if (tilesSpawned[i, j] == TargetTiles[i, j])
                    MatchCount++;
            }
        }

        yield return new WaitForSeconds(0.5f);

        MeshRenderer[] rends = currentLevel.GetChild(0).GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < rends.Length; i++)
        {
            rends[i].material = houseMat;
        }

        PlayHaptic(hapticTypes.success);
        PlaySound(soundTypes.money);

        if(MatchCount< 5)
        {
            customer.anim.Play("angry");
        }
        else if(MatchCount < 10)
        {
            customer.anim.Play("talk");
            orderCash = (int)(orderCash * 1.1f);
        }
        else if (MatchCount < 15)
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

        if (MatchCount > 4)
        {
            if (MatchCount < 10)
            {
                stars[0].SetActive(true);
            }
            else if (MatchCount < 15)
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
        nextTileFill.DOFillAmount((level % 3) == 0 ? 1 : ((level % 3) == 1 ? 0.33f : 0.66f), 0.5f).SetEase(Ease.Linear);
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
    void CreatePatterns()
    {
        patterns = new int[10,5,5]
        {
            {//p1
            {0, 0, 0, 0, 0 },
            {0, 0, 0, 0, 0 },
            {0, 0, 0, 0, 0 },
            {0, 0, 0, 0, 0 },
            {0, 0, 0, 0, 0 },
            },
            {//p2
            {0, 1, 0, 1, 0 },
            {0, 1, 0, 1, 0 },
            {0, 1, 0, 1, 0 },
            {0, 1, 0, 1, 0 },
            {0, 1, 0, 1, 0 },
            },
            {//p3
            {0, 0, 0, 0, 0 },
            {1, 1, 1, 1, 1 },
            {0, 0, 0, 0, 0 },
            {1, 1, 1, 1, 1 },
            {0, 0, 0, 0, 0 },
            },
            {//p4
            {0, 0, 0, 0, 0 },
            {0, 1, 1, 1, 0 },
            {0, 1, 2, 1, 0 },
            {0, 1, 1, 1, 0 },
            {0, 0, 0, 0, 0 },
            },
            {//p5
            {2, 0, 1, 0, 2 },
            {0, 0, 1, 0, 0 },
            {1, 1, 1, 1, 1 },
            {0, 0, 1, 0, 0 },
            {2, 0, 1, 0, 2 },
            },
            {//p6
            {0, 1, 2, 3, 4 },
            {0, 1, 2, 3, 4 },
            {0, 1, 2, 3, 4 },
            {0, 1, 2, 3, 4 },
            {0, 1, 2, 3, 4 },
            },
            {//p7
            {0, 0, 0, 0, 0 },
            {1, 1, 1, 1, 1 },
            {2, 2, 2, 2, 2 },
            {3, 3, 3, 3, 3 },
            {4, 4, 4, 4, 4 },
            },
            {//p8
            {0, 1, 1, 1, 1 },
            {0, 2, 3, 3, 1 },
            {0, 2, 4, 3, 1 },
            {0, 2, 2, 2, 1 },
            {0, 0, 0, 0, 0 },
            },
            {//p9
            {0, 1, 2, 3, 4 },
            {1, 2, 3, 4, 3 },
            {2, 3, 4, 3, 2 },
            {3, 4, 3, 2, 1 },
            {4, 3, 2, 1, 0 },
            },
            {//p10
            {0, 1, 0, 1, 0 },
            {1, 0, 1, 0, 1 },
            {0, 1, 0, 1, 0 },
            {1, 0, 1, 0, 1 },
            {0, 1, 0, 1, 0 },
            }
        };
    }
}
[System.Serializable]
public class TileData
{
    public Material mat;
    public Sprite sprite;
}