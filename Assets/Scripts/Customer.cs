using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Customer : MonoBehaviour
{
    public Animator anim;
    [SerializeField] GameManager gmanager;

    // Start is called before the first frame update
    void Start()
    {
        transform.GetChild(0).GetChild(Random.Range(0, transform.GetChild(0).childCount)).gameObject.SetActive(true);
    }
}
