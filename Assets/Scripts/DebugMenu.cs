using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugMenu : MonoBehaviour
{
    public GameObject debugMenu;
    public TextMeshProUGUI timeText, tapText;

    bool onCamMov, onCamRot, onCamZoom, onTimeScale;
    Vector3 mousePos, mousePos1, camPos, camRot, offset;
    Camera cam;
    float time;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        timeText.text = "TIME x" + Time.timeScale;
        tapText.text = PlayerPrefs.HasKey("tap") ? "TAP ACTIVATED" : "SWIPE ACTIVATED";
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (onCamMov)
            {
                mousePos1 = cam.ScreenToViewportPoint(Input.mousePosition);
                offset = mousePos1 - mousePos;
                //cam.transform.position = camPos + new Vector3(offset.x * 20, offset.y * 20);
                cam.transform.position = camPos + cam.transform.right * offset.x * 20 + cam.transform.up * offset.y * 20;
            }
            if (onCamRot)
            {
                mousePos1 = cam.ScreenToViewportPoint(Input.mousePosition);
                offset = mousePos1 - mousePos;
                cam.transform.eulerAngles = camRot + new Vector3(offset.y * -120, offset.x * 120);
                //cam.transform.eulerAngles = camRot + cam.transform.right * offset.y * -120 + cam.transform.up * offset.x * 120;
            }
            if (onCamZoom)
            {
                mousePos1 = cam.ScreenToViewportPoint(Input.mousePosition);
                offset = mousePos1 - mousePos;
                cam.transform.position = camPos + cam.transform.forward * offset.y * 20;
            }
            if (onTimeScale)
            {
                mousePos1 = cam.ScreenToViewportPoint(Input.mousePosition);
                offset = mousePos1 - mousePos;
                Time.timeScale = time + (offset.x * 2);
                timeText.text = "TIME x" +  Time.timeScale;
            }
        }
    }

    public void ToggleMenu()
    {
        if (debugMenu.activeInHierarchy)
            debugMenu.SetActive(false);
        else
            debugMenu.SetActive(true);
    }
    public void OnCamMov(bool condn)
    {
        onCamMov = condn;
        mousePos = cam.ScreenToViewportPoint(Input.mousePosition);
        camPos = cam.transform.position;
    }
    public void OnCamZoom(bool condn)
    {
        onCamZoom = condn;
        mousePos = cam.ScreenToViewportPoint(Input.mousePosition);
        camPos = cam.transform.position;
    }
    public void OnCamRot(bool condn)
    {
        onCamRot = condn;
        mousePos = cam.ScreenToViewportPoint(Input.mousePosition);
        camRot = cam.transform.eulerAngles;
    }
    public void OnTimeScale(bool condn)
    {
        onTimeScale = condn;
        mousePos = cam.ScreenToViewportPoint(Input.mousePosition);
        time = Time.timeScale;
    }
    public void ToggleTapMechanics()
    {
        if (PlayerPrefs.HasKey("tap"))
            PlayerPrefs.DeleteKey("tap");
        else
            PlayerPrefs.SetInt("tap", 0);
        tapText.text = PlayerPrefs.HasKey("tap") ? "TAP ACTIVATED" : "SWIPE ACTIVATED";
    }
}
