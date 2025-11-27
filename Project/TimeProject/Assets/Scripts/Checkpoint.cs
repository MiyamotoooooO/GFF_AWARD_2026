using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    // ★ 今どのチェックポイントが有効かを共通で管理する変数（重要）
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
        // スプライトを生成
        if (spritePrefab != null)
        {
            spawnedSprite = Instantiate(spritePrefab, transform.position + spriteOffset, Quaternion.identity);
            spawnedSprite.transform.SetParent(transform);
        }

        // ★ 最初の表示設定（currentIndex と ID が一致したものだけ表示）
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
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerOnTile = true;

            // ★ 踏んだものが現在のチェックポイントなら
            if (checkpointID == currentIndex)
            {
                // 自分のスプライトを消す
                if (spawnedSprite != null)
                    spawnedSprite.SetActive(false);

                // 次のチェックポイントへ進む
                currentIndex++;

                // 全チェックポイントに「表示更新」してもらう
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
        if (collision.gameObject.CompareTag("Player"))
            isPlayerOnTile = false;
    }


    private void Update()
    {
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
            platform.SetActive(true);
            StartCoroutine(DeactivateAfterTime(platform, activeTime));
            yield return new WaitForSeconds(interval);
        }
    }

    private IEnumerator DeactivateAfterTime(GameObject platform, float time)
    {
        yield return new WaitForSeconds(time);
        platform.SetActive(false);
    }
}

