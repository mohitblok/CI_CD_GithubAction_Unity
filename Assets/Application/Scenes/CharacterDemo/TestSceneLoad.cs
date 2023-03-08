using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestSceneLoad : MonoBehaviour
{
    public void LoadScene(int scene)
    {
        switch (scene)
        {
            case 0:
                SceneManager.LoadScene("CharacterDemo 1");
                break;
            case 1:
                SceneManager.LoadScene("CharacterDemo 2");
                break;
            case 2:
                SceneManager.LoadScene("CharacterDemo 3");
                break;
        }
    }
}
