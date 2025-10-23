using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    //出現させたいプレハブ
    public GameObject objectToSpawn;
    private bool hasSpawned = false;

    private void OnCollisionEnter(Collision collision)
    {


        if (hasSpawned)
            return;

        //プレイヤーが踏んだときだけ出現させたい場合
        if (!hasSpawned && collision.gameObject.CompareTag("Player"))
        {


            //今の位置の少し上にオブジェクトを出す
            Vector3 spawnPosition = transform.position + Vector3.left;
            Quaternion spawnRotation = Quaternion.Euler(0, 0, 0);  // ここで向きを指定

            Instantiate(objectToSpawn, spawnPosition, spawnRotation);

            hasSpawned = true;  // 出現済みにする
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
