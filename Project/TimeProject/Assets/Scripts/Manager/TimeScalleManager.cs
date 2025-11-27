using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeScalleManager : MonoBehaviour
{
    private bool _isTime = false;

    public void SetTimeScale(bool isTime)
    {
        if(_isTime != isTime)
        {
            Time.timeScale = isTime ? 0 : 1;
            _isTime = isTime;
        }
    }

}
