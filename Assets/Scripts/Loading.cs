using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    public Image BackgroundImage;
    public List<Sprite> Sprites;
    public GameObject Container;
    public TextMeshProUGUI LoadingText;
    private bool loading;
    static public Loading instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void EnableLoading()
    {
        BackgroundImage.sprite = Sprites[Random.Range(0, Sprites.Count)];
        loading = true;
        Container.SetActive(true);
        StartCoroutine(LoadingCoroutine());
    }
    public void DisableLoading()
    {
        loading = false;
        Container.SetActive(false);
    }
    public IEnumerator LoadingCoroutine()
    {
        LoadingText.text = "Loading";
        while (loading)
        {
            LoadingText.text += ".";
            yield return new WaitForSeconds(0.3f);

            if (LoadingText.text.Count(x => x == '.') >= 3)
            {
                LoadingText.text = "Loading";
            }
        }
    }
}
