using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadManager : MonoBehaviour
{
   public void SetLoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
