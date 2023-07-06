using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryScreens : MonoBehaviour
{
    public void Play()
    {
        SceneManager.LoadScene("TreeLevel");
    }

    public void Back()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
