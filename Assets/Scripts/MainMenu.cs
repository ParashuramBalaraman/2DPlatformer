using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void Play()
    {
        SceneManager.LoadScene("TreeLevel");
    }

    public void Controls()
    {
        SceneManager.LoadScene("Controls");
    }

    public void Quit() 
    {
        Application.Quit();
    }
}
