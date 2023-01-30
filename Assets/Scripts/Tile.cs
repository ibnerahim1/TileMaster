using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Tile : MonoBehaviour
{
    public bool isTile;
    public Vector2Int index;
    private Material material;
    private GameManager gManager;
    public bool active;

    private void Start()
    {
        gManager = FindObjectOfType<GameManager>();
        material = GetComponent<MeshRenderer>().material;
        if (!isTile)
            material.DOFade(0, 0);
    }

    public void Initialise()
    {
        if (isTile && gManager.remove)
            transform.DOMoveY(0.2f, 0.25f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo).SetDelay(transform.childCount * 0.01f).SetId(transform.GetHashCode());
        else
        {
            material.DOFade(1, 0.5f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo).SetDelay(transform.childCount * 0.01f).SetId(transform.GetHashCode());
            active = true;
        }
    }
    private void OnMouseEnter()
    {
        if (!isTile && !gManager.remove && gManager.gameStarted && !gManager.bonus)
        {
            gManager.Touched(index);
            DOTween.Kill(transform.GetHashCode());
            material.DOFade(1, 0);
        }
    }
    public void PauseHighlight()
    {
        DOTween.Kill(transform.GetHashCode());
        material.DOFade(0, 0);
    }
    private void OnMouseDown()
    {
        if (!isTile && !gManager.remove && gManager.gameStarted && gManager.bonus && !gManager.touched)
        {
            gManager.Touched(index);
            DOTween.Kill(transform.GetHashCode());
            material.DOFade(1, 0);
        }
        if (isTile && gManager.remove)
        {
            gManager.PlayHaptic(GameManager.hapticTypes.heavy);

            transform.DOScale(new Vector3(1.8f, 1, 1.8f), 0);
            isTile = false;
            gManager.touchCount--;
            DOTween.Kill(transform.GetHashCode());
            transform.DOMoveY(0, 0);
            GetComponent<MeshRenderer>().material = material;
            Instantiate(gManager.destroyedTileFX, transform.position, Quaternion.identity);
            material.DOFade(0, 0);
        }
    }
}