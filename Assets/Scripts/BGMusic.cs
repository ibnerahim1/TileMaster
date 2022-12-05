using UnityEngine;

public class BGMusic : MonoBehaviour
{
    public static BGMusic bgMusic;

    private void Awake()
    {
        bgMusic = FindObjectOfType<BGMusic>();
        if (bgMusic != null && bgMusic != this)
        {
            Destroy(gameObject);
        }
        else
        {
            bgMusic = GetComponent<BGMusic>();
            DontDestroyOnLoad(gameObject);
        }
        //FindObjectOfType<Settings>().bgMusic = GetComponent<AudioSource>();
    }
}