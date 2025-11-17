using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    //出現させたいプレハブ
    public GameObject objectToSpawn;

    public Vector3 spawnOffset = Vector3.left;

    private AudioSource audioSource; //マス音


    private bool hasSpawned = false;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    private void OnCollisionEnter(Collision collision)
    {


        if (hasSpawned)
            return;

        //プレイヤーが踏んだときだけ出現させたい場合
        if (!hasSpawned && collision.gameObject.CompareTag("Player"))
        {


            //今の位置の少し上にオブジェクトを出す
            Vector3 spawnPosition = transform.position + spawnOffset;
            Quaternion spawnRotation = Quaternion.Euler(0, 0, 0);  // ここで向きを指定


            Instantiate(objectToSpawn, spawnPosition, spawnRotation);

            hasSpawned = true;  // 出現済みにする

            Debug.Log("スポーン完了");

            // マス生成音（このオブジェクトから鳴らす）
            if (audioSource != null)
            {
                audioSource.Play();
            }
        }
    }


}


