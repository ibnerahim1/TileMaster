using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TipRenderer : MonoBehaviour
{
    public bool active, isTile;
    private Material material;
    private GameManager gManager;

    private void Start()
    {
        gManager = FindObjectOfType<GameManager>();
        material = GetComponent<MeshRenderer>().material;
        if(!isTile)
            material.DOFade(0, 0);
    }

    public void Initialise()
    {
        active = true;
        if (isTile && gManager.remove)
            transform.DOMoveY(0.2f, 0.25f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo).SetDelay(transform.childCount * 0.01f).SetId(transform.GetHashCode());
        else
            material.DOFade(1, 0.5f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo).SetDelay(transform.childCount * 0.01f).SetId(transform.GetHashCode());
    }
    private void OnMouseEnter()
    {
        if (active && !PlayerPrefs.HasKey("tap") && !isTile && !gManager.remove && Input.GetMouseButton(0))
        {
            active = false;
            gManager.Touched(transform.position);
            DOTween.Kill(transform.GetHashCode());
            material.DOFade(0, 0);
        }
    }
    public void PauseHighlight()
    {
        DOTween.Kill(transform.GetHashCode());
        material.DOFade(0, 0);
    }
    private void OnMouseDown()
    {
        if (active && PlayerPrefs.HasKey("tap") && !isTile && gManager.remove)
        {
            active = false;
            gManager.Touched(transform.position);
            DOTween.Kill(transform.GetHashCode());
            material.DOFade(0, 0);
        }
        else if(isTile && gManager.remove)
        {
            gManager.touchCount--;
            DOTween.Kill(transform.GetHashCode());
            transform.DOMoveY(0.2f, 0);
            gManager.currentLevel.GetChild(1).GetChild((int)(((transform.position.x + 9.5f) * 10) + transform.position.z + 4.5f)).GetComponent<TipRenderer>().active = true;
            gManager.placedTiles.Remove(GetComponent<TipRenderer>());
            Instantiate(gManager.tileFX, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
