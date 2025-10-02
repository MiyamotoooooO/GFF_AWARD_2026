using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    //出現させたいプレハブ
    public GameObject objectToSpawn;

    private void OnTriggerEnter(Collider other)
    {
        //プレイヤーが踏んだときだけ出現させたい場合
        if (other.CompareTag("Player"))
        {
            //今の位置の少し上にオブジェクトを出す
            Vector3 spawnPosition = transform.position + Vector3.left;
            Instantiate(objectToSpawn, spawnPosition, Quaternion.identity);
            Debug.Log("スポーン完了");
        }
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}

