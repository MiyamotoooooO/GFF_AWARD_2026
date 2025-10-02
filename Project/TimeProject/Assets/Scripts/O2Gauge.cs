using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class O2Gauge : MonoBehaviour
{
    [Tooltip("é_ëféûä‘/ïb")]
    [SerializeField] private float maxO2 = 300f;
    [Tooltip("åªç›ÇÃé_ëfó ")]
    [SerializeField] private float currentO2;

    [Tooltip("UI")]
    [SerializeField] private Image o2Image;
    [Tooltip("é_ëfÉQÅ[ÉWÉXÉvÉâÉCÉg")]
    [SerializeField] private Sprite[] o2Sprite;
    
    void Start()
    {
        currentO2 = maxO2;        
    }
    void Update()
    {
        
    }
    void UIUpdate()
    {

    }
}
