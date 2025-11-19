using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Checkpoint : MonoBehaviour
{
    [Header("出現する足場を指定")]
    public GameObject[] platformsToActivate;

    [Header("出現してから消えるまでの時間")]
    public float activeTime = 3f;

    [Header("次の足場を出すまでの時間")]
    public float interval = 1f;

    private bool hasActivated = false;
    private bool isPlayerOnTile = false;   // ← プレイヤーがマスに乗っているか判定

    public int checkpointID; // 各チェックポイントにユニークなIDを設定



    private void OnCollisionEnter(Collision collision)
    {
        // プレイヤータグをもつか判定
        if (collision.gameObject.CompareTag("Player") ||
            collision.transform.root.CompareTag("Player"))
        {
            isPlayerOnTile = true;
        }

        if (collision.gameObject.name == "Player")
        {
            SaveManager save = SaveManager.Instance;



            if (!save.currentData.checkpointReached[checkpointID])
            {
                // 初めて踏んだときのみ更新
                save.currentData.checkpointReached[checkpointID] = true;



                Vector3 pos = transform.position;
                save.currentData.respawnPosition[0] = pos.x;
                save.currentData.respawnPosition[1] = pos.y;
                save.currentData.respawnPosition[2] = pos.z;



                if (OxygenGaugeController.Instance != null)
                {
                    OxygenGaugeController.Instance.RecoverFullOxygen();
                }

                if (BottleUIManager.Instance != null)
                {
                    BottleUIManager.Instance.ResetBottlesToFull();
                }

                if (ObjectController1.Instance != null)
                {
                    ObjectController1.Instance.ResetTakoCountAndGauge();
                }

                save.SaveGame();
                Debug.Log($"チェックポイント {checkpointID} を更新！");
            }

        }

    }
    private void OnCollisionExit(Collision collision)
    {
        // マスから降りたら false に戻す
        if (collision.gameObject.CompareTag("Player") ||
            collision.transform.root.CompareTag("Player"))
        {
            isPlayerOnTile = false;
        }
    }
    private void Update()
    {
        // すでに発動済みなら無視
        if (hasActivated) return;

        // 条件：マスに乗っている & スペースキーが押された
        if (isPlayerOnTile)
        {
            hasActivated = true;
            StartCoroutine(ActivatePlatformsSequentially());
        }
    }
    private IEnumerator ActivatePlatformsSequentially()
    {
        for (int i = 0; i < platformsToActivate.Length; i++)
        {
            GameObject platform = platformsToActivate[i];

            // 足場を出現
            platform.SetActive(true);

            // 一定時間後に消える
            StartCoroutine(DeactivateAfterTime(platform, activeTime));

            // 次の足場を出すまで待つ
            yield return new WaitForSeconds(interval);
        }
    }

    private IEnumerator DeactivateAfterTime(GameObject platform, float time)
    {
        yield return new WaitForSeconds(time);
        platform.SetActive(false);
    }
}


