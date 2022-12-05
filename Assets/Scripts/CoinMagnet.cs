using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class CoinMagnet : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform coinObj;
    public Transform targetCoin;
    public Transform parent;

    [SerializeField] GameManager gManager;

    public void SpawnCoins(int size)
    {
        for (int i = 0; i < size; i++)
        {
            Transform temp = Instantiate(coinObj, parent);
            temp.DOLocalMove(new Vector3(Random.Range(-100, 100), Random.Range(-100, 100), 0), 0.5f).From(Vector3.zero).SetEase(Ease.Linear);
            temp.DOLocalMove(targetCoin.localPosition, Random.Range(0.5f, 0.7f)).SetDelay(Random.Range(0.7f, 1f)).SetEase(Ease.Linear).OnComplete(()=>
            {
                gManager.PlaySound(GameManager.soundTypes.coins);
                Destroy(temp.gameObject);
                targetCoin.DOScale(Vector3.one, 0).SetEase(Ease.Linear);
                targetCoin.DOPunchScale(Vector3.one * 1.1f, 0.2f, 1);
            });
        }
    }
    public void DropCoin(int size)
    {
        for (int i = 0; i < size; i++)
        {
            StartCoroutine(DroppingCoin(i * 0.05f));
        }
    }
    IEnumerator DroppingCoin(float t)
    {
        yield return new WaitForSeconds(t);

        Transform temp = Instantiate(coinObj, parent);
        temp.localPosition = targetCoin.localPosition;
        temp.DOLocalMoveY(temp.localPosition.y - 50, 0.25f).SetEase(Ease.Linear);
        temp.GetComponent<Image>().DOFade(0, 0.25f).SetEase(Ease.Linear).OnComplete(()=>Destroy(temp.gameObject));

        gManager.PlayHaptic(GameManager.hapticTypes.soft);
        gManager.PlaySound(GameManager.soundTypes.coins);
    }
}
