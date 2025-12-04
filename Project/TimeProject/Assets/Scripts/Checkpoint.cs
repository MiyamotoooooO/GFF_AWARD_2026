using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    // ★ 今どのチェックポイントが有効かを共通で管理する変数
    public static int currentIndex = 0;

    [Header("このチェックポイントの順番（0 から）")]
    public int checkpointID;

    [Header("チェックポイントのスプライト（Prefab）")]
    public GameObject spritePrefab;

    [Header("スプライトの高さオフセット")]
    public Vector3 spriteOffset = new Vector3(0, 1.5f, 0);

    private GameObject spawnedSprite;

    private bool isPlayerOnTile = false;
    private bool hasActivated = false;

    [Header("出現する足場を指定")]
    public GameObject[] platformsToActivate;

    [Header("出現してから消えるまでの時間")]
    public float activeTime = 3f;

    [Header("次の足場を出すまでの時間")]
    public float interval = 1f;



    private void Start()
    {
        // ★ スプライト生成
        if (spritePrefab != null)
        {
            spawnedSprite = Instantiate(
                spritePrefab,
                transform.position + spriteOffset,
                Quaternion.identity
            );

            spawnedSprite.transform.SetParent(transform);
        }

        // ★ currentIndex に応じてスプライトを表示・非表示
        UpdateSpriteVisibility();
    }



    private void UpdateSpriteVisibility()
    {
        if (spawnedSprite != null)
        {
            // ★ 現在のチェックポイントIDなら表示、それ以外は非表示
            spawnedSprite.SetActive(checkpointID == currentIndex);
        }
    }



    private void OnCollisionEnter(Collision collision)
    {
        // プレイヤー判定
        if (collision.gameObject.CompareTag("Player") ||
            collision.transform.root.CompareTag("Player"))
        {
            isPlayerOnTile = true;
        }

        // プレイヤーオブジェクト名 "Player" のときにセーブ処理などを行う
        if (collision.gameObject.name == "Player")
        {
            SaveManager save = SaveManager.Instance;

            // ★ セーブデータ更新（初めて踏んだ時だけ）
            if (!save.currentData.checkpointReached[checkpointID])
            {
                save.currentData.checkpointReached[checkpointID] = true;

                Vector3 pos = transform.position;
                save.currentData.respawnPosition[0] = pos.x;
                save.currentData.respawnPosition[1] = pos.y;
                save.currentData.respawnPosition[2] = pos.z;

                // ★ 酸素・ボトル・タコ回復
                OxygenGaugeController.Instance?.RecoverFullOxygen();
                BottleUIManager.Instance?.ResetBottlesToFull();
                //ObjectController1.Instance?.ResetTakoCountAndGauge();

                save.SaveGame();
                Debug.Log($"チェックポイント {checkpointID} を更新！");
            }

            // ★ チェックポイントインデックスの更新（スプライト制御）
            if (checkpointID == currentIndex)
            {
                if (spawnedSprite != null)
                    spawnedSprite.SetActive(false);

                currentIndex++;

                // 全チェックポイントに「表示更新」させる
                Checkpoint[] cps = FindObjectsOfType<Checkpoint>();
                foreach (var cp in cps)
                {
                    cp.UpdateSpriteVisibility();
                }
            }
        }
    }



    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") ||
            collision.transform.root.CompareTag("Player"))
        {
            isPlayerOnTile = false;
        }
    }



    private void Update()
    {
        // プレイヤーが乗っていて、まだ発動していないなら足場開始
        if (!hasActivated && isPlayerOnTile)
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

            // 数秒後に消える
            StartCoroutine(DeactivateAfterTime(platform, activeTime));

            // 次の足場まで待つ
            yield return new WaitForSeconds(interval);
        }
    }



    private IEnumerator DeactivateAfterTime(GameObject platform, float time)
    {
        yield return new WaitForSeconds(time);
        platform.SetActive(false);
    }
}

