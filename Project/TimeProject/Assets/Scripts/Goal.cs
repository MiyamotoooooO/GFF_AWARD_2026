using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    private StageManager stageManager;
    // Start is called before the first frame update
   private void Start()
    {
        stageManager = FindObjectOfType<StageManager>();
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        //プレイやーがぶつかったらクリア
        if(collision.gameObject.CompareTag("Player"))
        {
            stageManager.OnStageClear();
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
