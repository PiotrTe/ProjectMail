using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buttons : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneManagementController.Instance.LoadScene(sceneName);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
