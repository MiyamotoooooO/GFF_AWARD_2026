using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformActivator : MonoBehaviour
{
    [Header("出現する足場を指定")]
    public GameObject[] platformsToActivate;

    [Header("出現してから消えるまでの時間")]
    public float activeTime = 3f;

    [Header("次の足場を出すまでの時間")]
    public float interval = 1f;

    private bool hasActivated = false;
    private bool isPlayerOnTile = false;   // ← プレイヤーがマスに乗っているか判定

    private void OnCollisionEnter(Collision collision)
    {
        // プレイヤータグをもつか判定
        if (collision.gameObject.CompareTag("Player") ||
            collision.transform.root.CompareTag("Player"))
        {
            isPlayerOnTile = true;
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
        if (isPlayerOnTile && Input.GetKeyDown(KeyCode.Space))
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

