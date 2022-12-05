using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Settings : MonoBehaviour
{
    [SerializeField] GameObject settingsPanel, musicImg, hapticImg;
    [SerializeField] AudioListener audioListener;

    private GameManager gManager;
    private int music, haptic;

    void Start()
    {
        gManager = FindObjectOfType<GameManager>();
        InitialiseSettings();
    }

    public void ToggleSettings(bool condition)
    {
        gManager.PlaySound(GameManager.soundTypes.tap);

        if (condition)
        {
            settingsPanel.SetActive(true);
            settingsPanel.transform.GetChild(0).DOScale(Vector3.one, 0.2f).SetEase(Ease.Linear).From(Vector3.zero);
        }
        else
        {
            settingsPanel.SetActive(false);
        }
    }
    public void ToggleMusic()
    {
        gManager.PlaySound(GameManager.soundTypes.tap);

        music = music == 0 ? 1 : 0;
        PlayerPrefs.SetInt("music", music);
        musicImg.SetActive(music == 0);
        audioListener.enabled = (music == 1);
        ToggleSettings(false);
    }
    public void ToggleHaptic()
    {
        gManager.PlaySound(GameManager.soundTypes.tap);

        haptic = haptic == 0 ? 1 : 0;
        PlayerPrefs.SetInt("haptic", haptic);
        hapticImg.SetActive(haptic == 0);
        ToggleSettings(false);
    }

    private void InitialiseSettings()
    {
        music = PlayerPrefs.HasKey("music") ? PlayerPrefs.GetInt("music") : 1;
        haptic = PlayerPrefs.HasKey("haptic") ? PlayerPrefs.GetInt("haptic") : 1;
        musicImg.SetActive(music == 0);
        hapticImg.SetActive(haptic == 0);
        audioListener.enabled = (music == 1);
    }
}