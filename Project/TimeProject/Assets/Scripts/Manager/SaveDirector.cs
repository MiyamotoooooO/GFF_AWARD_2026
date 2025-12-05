using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveDirector : MonoBehaviour
{
    public void ResetGameScene()
    {
        SaveManager.Instance.ResetSaveData();
    }
}
