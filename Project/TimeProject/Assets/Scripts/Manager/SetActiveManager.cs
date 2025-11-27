using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetActiveManager : MonoBehaviour
{
    [SerializeField] private bool _startState;

    void Start()
    {
        gameObject.SetActive(_startState);
    }

    public void SetGameObjectActiveSate(bool isActive)
    {
        gameObject.SetActive(isActive);
    }
}
