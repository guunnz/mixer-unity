using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ForfeitMenu : MonoBehaviour
{
    public void Forfeit()
    {
        Loading.instance.EnableLoading();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
//