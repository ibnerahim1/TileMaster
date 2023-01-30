using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MoreMountains.NiceVibrations;
using YsoCorp.GameUtils;
//using UnityEditor;

public class GameManager : MonoBehaviour
{
    #region UI Properties

    [Header("UI Properties")]
    public Transform tilesContainer;
    public Transform decorContainer;
    [SerializeField] TextMeshProUGUI levelTxt, cashTxt, orderCashTxt, fillPercent;
    [SerializeField] Transform progressBar, removeTileImg;
    [SerializeField] GameObject[] stars;
    [SerializeField] GameObject hintButton, winPanel, menuPanel, gamePanel, storePanel, cashPanel, whiteScreen, nextButton, tilesSelection,
                                decorSelection, orderPanel, returnDoneButton, sampleButton, sampleFullButton, handTut, handTut1, storeButton;
    [SerializeField] Image nextTileFill, bonusImg;
    [SerializeField] Texture2D handCursor;
    #endregion

    #region GamePlay Properties

    [Header("GamePlay Properties")]
    public TileData[] tileDatas;
    public BonusData[] bonusDatas;
    public BonusData currentBonusData;
    public int touchCount, currentPattern, gridSize;
    [SerializeField] Mesh[] bonusMeshTiles;
    [SerializeField] Transform camPositions, sampleCam, levelParent, levelObj, tileObj, hintTile;
    [SerializeField] Customer customer;
    [SerializeField] Material houseMat, floorMat;
    [HideInInspector] public bool gameStarted, remove, bonus, touched, hint;
    [HideInInspector] public int level, tilesLevel, decorLevel, cash, orderCash;

    public Transform currentLevel;
    public Sprite[] decorSprites;
    public Tile[,] tiles = new Tile[5, 5];
    public List<TileData> currentMats = new List<TileData>();
    public int[,] TargetTiles = new int[5, 5], tilesSpawned = new int[5, 5];
    #endregion

    #region Sounds & Effects

    public enum hapticTypes { soft, light, medium, heavy, success, failure };
    public enum soundTypes { pop, upgrade, money, coins, tap, win };
    [Header("Sounds & Effects")]
    public Transform destroyedTileFX;
    public Transform[] audioClips;
    #endregion

    private int[,,] patterns;
    private Transform tile;
    private Camera cam;
    private const int camTween = 0;
    private int tileIndex, ABtest;
    private float popPitch = 0.5f;
    private bool removeTileTut, decorDone;

    private void Awake()
    {
        LoadData();
    }
    private void Start()
    {
        Initialise();

        YsoCorp.GameUtils.YCManager.instance.OnGameStarted(level + 1);

        cam.transform.DOMove(camPositions.GetChild(0).position, 0.5f).SetEase(Ease.Linear).SetId(camTween).SetDelay(0.5f);
        cam.transform.DORotate(camPositions.GetChild(0).eulerAngles, 0.5f).SetEase(Ease.Linear).SetId(camTween).SetDelay(0.5f).OnComplete(() =>
        {
            if (level == 0)
            {
                menuPanel.SetActive(true);
                menuPanel.transform.GetChild(0).gameObject.SetActive(true);
                menuPanel.transform.GetChild(1).gameObject.SetActive(false);
            }
            else if ((level + 1) % 5 == 0)
            {
                bonusImg.sprite = currentBonusData.sprite;
                MakeBonus();
            }
            else
                PlayLevel();
        });
        CreateLevel();
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
        if (Input.GetKeyDown("p"))
            PlayerPrefs.SetInt("level", PlayerPrefs.GetInt("level") - 1);
        if (Input.GetKeyDown("l"))
            StartCoroutine(LevelComplete());
#endif
        #endregion
        cashTxt.text = GetValue(cash);
        if (popPitch > 0.5f)
            popPitch = Mathf.Lerp(popPitch, 0.5f, Time.deltaTime);
        //if (Input.GetMouseButtonDown(0))
        //    Cursor.SetCursor(handCursor, new Vector2(35, 35), CursorMode.ForceSoftware);
        //if (Input.GetMouseButtonUp(0))
        //    Cursor.SetCursor(PlayerSettings.defaultCursor, new Vector2(35, 35), CursorMode.ForceSoftware);

    }
    void Initialise()
    {
        if (YCManager.instance.abTestingManager.IsPlayerSample("Skip"))
        {
            ABtest = 1;
        }
        else
            ABtest = 0;


        //#if UNITY_EDITOR
        //        Cursor.SetCursor(PlayerSettings.defaultCursor, new Vector2(35, 35), CursorMode.ForceSoftware);
        //#endif
        cam = Camera.main;
        level = PlayerPrefs.HasKey("level") ? PlayerPrefs.GetInt("level") : 0;
        levelTxt.text = "LEVEL " + (level + 1);
        currentBonusData = bonusDatas[(int)Mathf.Clamp((level / 5) % bonusDatas.Length, 0, Mathf.Infinity)];
        gridSize = 5;

        if (level > 2)
        {
            int temp = ((level + 3) / 6) + 2;
            tilesLevel = temp > tilesLevel ? temp : tilesLevel;
            temp = level / 6;
            decorLevel = temp > decorLevel ? temp : decorLevel;
        }
        print(level + "   " + tilesLevel + "  " + decorLevel);

        if ((level / 3) % 2 == 0 ? tilesLevel < 10 : decorLevel < 10)
        {
            nextTileFill.fillAmount = (float)(level % 3) / 3;
            nextTileFill.sprite = (level / 3) % 2 == 0 ? tileDatas[tilesLevel].sprite : decorSprites[decorLevel];
            nextTileFill.transform.parent.GetChild(0).GetComponent<Image>().sprite = (level / 3) % 2 == 0 ? tileDatas[tilesLevel].sprite : decorSprites[decorLevel];
            fillPercent.text = (level % 3) == 0 ? "30%" : ((level % 3) == 1 ? "60%" : "100%");
        }
        else
            nextTileFill.transform.parent.gameObject.SetActive(false);
    }

    void CreateLevel()
    {
        int l = level - 1;
        Random.State state = Random.state;
        Random.InitState(l);

        List<TileData> tempMats = new List<TileData>();
        currentMats.Clear();
        for (int i = 0; i < Mathf.Min((l / 3) + 2, tileDatas.Length); i++)
        {
            tempMats.Add(tileDatas[i]);
        }
        for (int i = 0; i < tempMats.Count; i++)
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
        currentLevel = Instantiate(levelObj, Vector3.right * -10.5f, Quaternion.identity, levelParent);
        currentLevel.GetChild(0).GetChild(Random.Range(0, currentLevel.GetChild(0).childCount)).gameObject.SetActive(true);
        currentPattern = l < 20 ? l % 10 : Random.Range(0, 10);
        MeshRenderer[] rends = currentLevel.GetChild(0).GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < rends.Length; i++)
        {
            rends[i].material = houseMat;
        }
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                int a = patterns[Mathf.Abs(currentPattern), i, j];
                while (a > currentMats.Count - 1)
                    a -= currentMats.Count;
                tile = Instantiate(tileObj, new Vector3((j * 2) - 14.5f, 0, (i * 2) + 1), Quaternion.identity, currentLevel);
                tile.GetComponent<MeshRenderer>().material = currentMats[a].mat;
            }
        }
        Random.state = state;

        state = Random.state;
        Random.InitState(level);
        currentLevel = Instantiate(levelObj, Vector3.zero, Quaternion.identity, levelParent);
        currentLevel.GetChild(0).GetChild(Random.Range(0, currentLevel.GetChild(0).childCount)).gameObject.SetActive(true);
        Random.state = state;

        state = Random.state;
        Random.InitState(level + 1);
        Transform temp = Instantiate(levelObj, Vector3.right * 10.5f, Quaternion.identity, levelParent);
        temp.GetChild(0).GetChild(Random.Range(0, temp.GetChild(0).childCount)).gameObject.SetActive(true);
        temp.GetComponent<MeshRenderer>().material = floorMat;
        Random.state = state;
    }
    void CreateTiles()
    {
        Random.State state = Random.state;
        Random.InitState(level);

        if (bonus)
        {
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    tiles[i, j] = Instantiate(tileObj, new Vector3(j - 4.5f, 0, i + 4.5f), Quaternion.identity, currentLevel).GetComponent<Tile>();
                    tiles[i, j].isTile = false;
                    tiles[i, j].index = new Vector2Int(i, j);
                    tiles[i, j].transform.localScale = new Vector3(1, 1, 1);
                    tiles[i, j].GetComponent<MeshFilter>().mesh = bonusMeshTiles[(i * gridSize) + j];
                }
            }
            currentPattern = 0;
        }
        else
        {
            currentMats.Clear();
            for (int i = 0; i < tilesLevel; i++)
            {
                currentMats.Add(tileDatas[i]);
            }
            //shuffle currentMats
            currentMats.Sort((a, b) => 1 - 2 * Random.Range(0, 2));

            print(currentMats.Count);
            for (int i = 0; i < currentMats.Count; i++)
            {
                tilesContainer.GetChild(i).GetChild(0).GetComponent<Image>().sprite = currentMats[i].sprite;
                tilesContainer.GetChild(i).GetComponent<Button>().interactable = true;
            }
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    tiles[i, j] = Instantiate(tileObj, new Vector3((j * 2) - 4, 0, (i * -2) + 9f), Quaternion.identity, currentLevel).GetComponent<Tile>();
                    tiles[i, j].isTile = false;
                    tiles[i, j].index = new Vector2Int(i, j);
                    tiles[i, j].transform.localScale = new Vector3(1.8f, 1, 1.8f);
                }
            }
            currentPattern = level < 20 ? level % 10 : Random.Range(0, 10);
        }
        if (ABtest > 0)
        {
            for (int i = 0; i < decorLevel; i++)
            {
                decorContainer.GetChild(i).GetChild(0).GetComponent<Image>().color = Color.white;
                decorContainer.GetChild(i).GetComponent<Button>().interactable = true;
            }
        }

        Random.state = state;
    }

    public void ShopUpdated()
    {
        if (!bonus)
        {
            currentMats.Clear();
            for (int i = 0; i < tilesLevel; i++)
            {
                currentMats.Add(tileDatas[i]);
            }
            print(currentMats.Count);
            for (int i = 0; i < currentMats.Count; i++)
            {
                tilesContainer.GetChild(i).GetChild(0).GetComponent<Image>().sprite = currentMats[i].sprite;
                tilesContainer.GetChild(i).GetComponent<Button>().interactable = true;
            }
        }
        for (int i = 0; i < decorLevel; i++)
        {
            decorContainer.GetChild(i).GetChild(0).GetComponent<Image>().color = Color.white;
            decorContainer.GetChild(i).GetComponent<Button>().interactable = true;
        }
    }
    //public void PlacedDecor(int a)
    //{
    //    PlaySound(soundTypes.upgrade);
    //    PlayHaptic(hapticTypes.soft);

    //    decors.GetChild(a).gameObject.SetActive(true);
    //    decorContainer.GetChild(a).GetComponent<Button>().interactable = false;
    //}

    public void RemoveTiles(bool condn)
    {
        YsoCorp.GameUtils.YCManager.instance.adsManager.ShowInterstitial(() =>
        {

            if (condn)
            {
                if (removeTileTut)
                {
                    removeTileTut = false;
                    DOTween.Kill(2);
                }
                tilesSelection.transform.DOLocalMoveX(1000, 0.25f).SetEase(Ease.Linear).OnComplete(() =>
                {
                    returnDoneButton.SetActive(true);
                    tilesSelection.SetActive(false);
                });
                for (int i = 0; i < gridSize; i++)
                {
                    for (int j = 0; j < gridSize; j++)
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
                UpdateTilesContainer();
                tilesSelection.transform.DOLocalMoveX(0, 0.25f).SetEase(Ease.Linear);
                if (!bonus)
                {
                    for (int i = 0; i < gridSize; i++)
                    {
                        for (int j = 0; j < gridSize; j++)
                        {
                            if (!tiles[i, j].isTile)
                                tiles[i, j].Initialise();
                        }
                    }
                }
                if (touchCount > (gridSize * gridSize) - 1)
                    nextButton.SetActive(true);
            }
            remove = condn;
        });
    }
    public void TileSelection(int index)
    {
        YsoCorp.GameUtils.YCManager.instance.adsManager.ShowInterstitial(() =>
        {
            PlaySound(soundTypes.tap);
            PlayHaptic(hapticTypes.light);

            handTut.SetActive(false);
            if (level < 1)
                handTut1.SetActive(true);

            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    tiles[i, j].PauseHighlight();
                }
            }

            tileIndex = index;
            if (bonus)
            {
                touched = false;
                if (hint)
                {
                    hint = false;
                    hintButton.SetActive(true);
                    //hintTile.position = tiles[bonusIndices[index] / gridSize, bonusIndices[index] % gridSize].transform.position + Vector3.up * 0.2f;
                    hintTile.GetComponent<MeshRenderer>().material.DOFade(1, 0.5f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo).SetId(hintTile.transform.GetHashCode());
                }

            }
            else
            {
                for (int i = 0; i < gridSize; i++)
                {
                    for (int j = 0; j < gridSize; j++)
                    {
                        //if (index == tiles[i, j].BonusIndex.x)
                        tiles[i, j].Initialise();
                    }
                }
            }

            //tilesSelection.SetActive(false);
            DOTween.Kill(camTween);
            cam.transform.DOMove(camPositions.GetChild(2).position, 0.5f).SetEase(Ease.Linear).SetId(camTween);
            cam.transform.DORotate(camPositions.GetChild(2).eulerAngles, 0.5f).SetEase(Ease.Linear).SetId(camTween);
        });
    }
    public void Touched(Vector2Int tIndex)
    {
        PlaySound(soundTypes.pop);
        PlayHaptic(hapticTypes.light);

        handTut.SetActive(false);
        handTut1.SetActive(false);

        DOTween.Kill(hintTile.transform.GetHashCode());
        hintTile.GetComponent<MeshRenderer>().material.DOFade(0, 0);

        touched = true;
        touchCount++;
        tiles[tIndex.x, tIndex.y].isTile = true;
        tiles[tIndex.x, tIndex.y].transform.localScale = bonus ? new Vector3(3.334f, 1, 3.334f) : new Vector3(2, 1, 2);
        tiles[tIndex.x, tIndex.y].transform.position += new Vector3(0, 0.5f, 0);
        //tiles[tIndex.x, tIndex.y].GetComponent<MeshRenderer>().material = bonus ? currentBonusData.mat : currentMats[tileIndex].mat;
        tiles[tIndex.x, tIndex.y].transform.DOMoveY(0, 0.25f).SetEase(Ease.Linear);
        tilesSpawned[tIndex.x, tIndex.y] = tileIndex;

        if (touchCount > (gridSize * gridSize) - 1)
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

        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                yield return new WaitForSeconds(0.01f);

                tile = Instantiate(tileObj, bonus ? new Vector3(j - 4.5f, -10, i + 19.5f) : new Vector3((j * 2), -10, (i * -2) - 20), Quaternion.identity, currentLevel);
                if (bonus)
                {
                    tile.GetComponent<MeshFilter>().mesh = bonusMeshTiles[(i * gridSize) + j];
                    tile.localScale = new Vector3(1, 1, 1);
                }
                else
                {
                    int a = patterns[currentPattern, i, j];
                    while (a > currentMats.Count - 1)
                        a -= currentMats.Count;
                    //tile.GetComponent<MeshRenderer>().material = bonus ? currentBonusData.mat : currentMats[a].mat;
                    TargetTiles[i, j] = a;
                }
            }
        }

        DOTween.Kill(camTween);
        cam.transform.DOMove(camPositions.GetChild(2).position, 0.5f).SetEase(Ease.Linear).SetId(camTween);
        cam.transform.DORotate(camPositions.GetChild(2).eulerAngles, 0.5f).SetEase(Ease.Linear).SetId(camTween).OnComplete(() =>
        {
            gameStarted = true;
            if (level < 3)
                handTut.SetActive(true);
            tilesSelection.SetActive(true);
            UpdateTilesContainer();
        });
    }

    public void RewardHint()
    {
        if (YsoCorp.GameUtils.YCManager.instance.adsManager.IsRewardBasedVideo())
        {
            YsoCorp.GameUtils.YCManager.instance.adsManager.ShowRewarded((bool ok) =>
            {
                if (ok)
                {
                    hint = true;
                    hintButton.SetActive(false);
                    if (!touched)
                    {
                        hint = false;
                        hintButton.SetActive(true);
                        //hintTile.position = tiles[bonusIndices[tileIndex] / gridSize, bonusIndices[tileIndex] % gridSize].transform.position + Vector3.up * 0.2f;
                        hintTile.GetComponent<MeshRenderer>().material.DOFade(1, 0.5f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo).SetId(hintTile.transform.GetHashCode());
                    }
                }
            });
        }
    }

    public void MakeBonus()
    {
        bonus = true;
        gridSize = 10;
        tiles = new Tile[gridSize, gridSize];
        TargetTiles = new int[gridSize, gridSize];
        tilesSpawned = new int[gridSize, gridSize];
        PlayLevel();
        //if (YsoCorp.GameUtils.YCManager.instance.adsManager.IsRewardBasedVideo())
        //{
        //    YsoCorp.GameUtils.YCManager.instance.adsManager.ShowRewarded((bool ok) =>
        //    {
        //        if (ok)
        //        {
        //        }
        //    });
        //}
    }

    public void UpdateTilesContainer()
    {
        int activeChilds = 0;
        for (int i = 0; i < tilesContainer.childCount; i++)
        {
            if (tilesContainer.GetChild(i).gameObject.activeInHierarchy)
                activeChilds++;
        }
        tilesContainer.GetComponent<RectTransform>().sizeDelta = new Vector2((activeChilds * 350) + 50, 400);
    }

    private void InitProgressBar()
    {
        for (int i = 0; i < (level + 1) % 5; i++)
        {
            progressBar.GetChild(i).GetChild(0).gameObject.SetActive(true);
        }
        if ((level + 1) % 5 > 0)
        {
            progressBar.GetChild(level % 5).GetChild(1).gameObject.SetActive(true);
        }
        else
        {
            if (bonus)
            {
                hintButton.SetActive(true);
                for (int i = 0; i < 4; i++)
                {
                    progressBar.GetChild(i).GetChild(0).gameObject.SetActive(true);
                }
                progressBar.GetChild(4).GetChild(0).GetChild(1).GetComponent<Image>().sprite = currentBonusData.sprite;
                progressBar.GetChild(4).GetChild(0).GetChild(1).GetComponent<Image>().color = Color.white;
            }
            else
            {
                progressBar.GetChild(5).gameObject.SetActive(true);
                progressBar.GetChild(4).gameObject.SetActive(false);
            }
        }
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
            //case soundTypes.money:
            //    Instantiate(audioClips[2]);
            //    break;
            //case soundTypes.coins:
            //    Instantiate(audioClips[0]);
            //    break;
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
            gameStarted = false;
            storePanel.SetActive(true);
            storeButton.SetActive(false);
        }
        else
        {
            gameStarted = true;
            storePanel.SetActive(false);
            storeButton.SetActive(true);
        }
    }

    public void PlayLevel()
    {
        YsoCorp.GameUtils.YCManager.instance.adsManager.ShowInterstitial(() =>
        {
            //PlaySound(soundTypes.tap);
            CreateTiles();
            
            menuPanel.SetActive(false);
            gamePanel.SetActive(true);
            cashPanel.SetActive(false);
            DOTween.Kill(camTween);
            cam.transform.DOMove(camPositions.GetChild(1).position, 0.5f).SetEase(Ease.Linear).SetId(camTween);
            cam.transform.DORotate(camPositions.GetChild(1).eulerAngles, 0.5f).SetEase(Ease.Linear).SetId(camTween).OnComplete(() => StartCoroutine(makeSample()));
        });
    }
    public void Next()
    {
        YsoCorp.GameUtils.YCManager.instance.adsManager.ShowInterstitial(() =>
        {
            if (decorDone || ABtest == 0 || decorLevel < 1)
            {
                PlaySound(soundTypes.win);

                orderPanel.SetActive(false);
                nextButton.SetActive(false);
                cam.transform.GetChild(0).gameObject.SetActive(true);
                tilesSelection.SetActive(false);

                //cashPanel.SetActive(true);
                gamePanel.SetActive(false);

                DOTween.Kill(camTween);
                cam.transform.DOMove(camPositions.GetChild(0).position, 0.5f).SetEase(Ease.Linear).SetId(camTween);
                cam.transform.DORotate(camPositions.GetChild(0).eulerAngles, 0.5f).SetEase(Ease.Linear).SetId(camTween);
                StartCoroutine(LevelComplete());
            }
            else
            {
                PlaySound(soundTypes.tap);
                PlayHaptic(hapticTypes.soft);

                decorDone = true;

                orderPanel.SetActive(false);
                nextButton.SetActive(false);
                tilesSelection.SetActive(false);
                decorSelection.SetActive(true);
                Invoke("NextButton", 2);
            }
        });
    }
    void NextButton()
    {
        nextButton.SetActive(true);
    }


    public void LoadData()
    {
        cash = PlayerPrefs.HasKey("cash") ? PlayerPrefs.GetInt("cash") : 0;
        tilesLevel = PlayerPrefs.HasKey("tilesLevel") ? PlayerPrefs.GetInt("tilesLevel") : 2;
        decorLevel = PlayerPrefs.HasKey("decorLevel") ? PlayerPrefs.GetInt("decorLevel") : 0;

        CreatePatterns();
    }
    public void SaveData()
    {
        PlayerPrefs.SetInt("cash", cash);

        PlayerPrefs.SetInt("tilesLevel", tilesLevel);
        PlayerPrefs.SetInt("decorLevel", decorLevel);
    }

    IEnumerator LevelComplete()
    {
        YsoCorp.GameUtils.YCManager.instance.OnGameFinished(true);

        gameStarted = false;
        //cashPanel.SetActive(true);
        level++;
        PlayerPrefs.SetInt("level", level);

        int MatchCount = 0;

        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
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

        if (bonus)
        {
            if (MatchCount < 7)
            {
                customer.anim.Play("angry");
            }
            else
            {
                customer.anim.Play("jump");
                orderCash = (int)(orderCash * 2.5);
            }
        }
        else
        {
            if (MatchCount < 5)
            {
                customer.anim.Play("angry");
            }
            else if (MatchCount < 10)
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
        }
        GetComponent<CoinMagnet>().SpawnCoins((int)(orderCash * 0.1f));
        cash += orderCash;
        SaveData();

        yield return new WaitForSeconds(2);

        gamePanel.SetActive(false);
        cashPanel.SetActive(false);
        winPanel.SetActive(true);

        yield return new WaitForSeconds(1.5f);

        if (bonus)
        {
            if (MatchCount < 7)
            {
                stars[0].SetActive(true);
            }
            else if (MatchCount < 9)
            {
                stars[0].SetActive(true);
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
        else
        {
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
        }
        nextTileFill.DOFillAmount((level % 3) == 0 ? 1 : ((level % 3) == 1 ? 0.33f : 0.66f), 0.5f).SetEase(Ease.Linear);
    }

    public void Restart()
    {
        YsoCorp.GameUtils.YCManager.instance.adsManager.ShowInterstitial(() =>
        {
            PlaySound(soundTypes.tap);
            PlayHaptic(hapticTypes.soft);

            whiteScreen.GetComponent<Image>().DOFade(1, 0.5f).SetEase(Ease.Linear).OnComplete(() => SceneManager.LoadScene(0));
        });
    }
    public void SampleFullScreen(bool condn)
    {
        YsoCorp.GameUtils.YCManager.instance.adsManager.ShowInterstitial(() =>
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
        });
    }
    void CreatePatterns()
    {
        patterns = new int[10, 5, 5]
        {
            {//Pattern 1
            {0, 0, 0, 0, 0 },
            {0, 0, 0, 0, 0 },
            {0, 0, 0, 0, 0 },
            {0, 0, 0, 0, 0 },
            {0, 0, 0, 0, 0 },
            },
            {//Pattern 2
            {0, 1, 0, 1, 0 },
            {0, 1, 0, 1, 0 },
            {0, 1, 0, 1, 0 },
            {0, 1, 0, 1, 0 },
            {0, 1, 0, 1, 0 },
            },
            {//Pattern 3
            {0, 0, 0, 0, 0 },
            {1, 1, 1, 1, 1 },
            {0, 0, 0, 0, 0 },
            {1, 1, 1, 1, 1 },
            {0, 0, 0, 0, 0 },
            },
            {//Pattern 4
            {0, 0, 0, 0, 0 },
            {0, 1, 1, 1, 0 },
            {0, 1, 2, 1, 0 },
            {0, 1, 1, 1, 0 },
            {0, 0, 0, 0, 0 },
            },
            {//Pattern 5
            {2, 0, 1, 0, 2 },
            {0, 0, 1, 0, 0 },
            {1, 1, 1, 1, 1 },
            {0, 0, 1, 0, 0 },
            {2, 0, 1, 0, 2 },
            },
            {//Pattern 6
            {0, 1, 2, 3, 4 },
            {0, 1, 2, 3, 4 },
            {0, 1, 2, 3, 4 },
            {0, 1, 2, 3, 4 },
            {0, 1, 2, 3, 4 },
            },
            {//Pattern 7
            {0, 0, 0, 0, 0 },
            {1, 1, 1, 1, 1 },
            {2, 2, 2, 2, 2 },
            {3, 3, 3, 3, 3 },
            {4, 4, 4, 4, 4 },
            },
            {//Pattern 8
            {0, 1, 1, 1, 1 },
            {0, 2, 3, 3, 1 },
            {0, 2, 4, 3, 1 },
            {0, 2, 2, 2, 1 },
            {0, 0, 0, 0, 0 },
            },
            {//Pattern 9
            {0, 1, 2, 3, 4 },
            {1, 2, 3, 4, 3 },
            {2, 3, 4, 3, 2 },
            {3, 4, 3, 2, 1 },
            {4, 3, 2, 1, 0 },
            },
            {//Pattern 10
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
[System.Serializable]
public class BonusData
{
    public Texture tex;
    public Sprite sprite;
}