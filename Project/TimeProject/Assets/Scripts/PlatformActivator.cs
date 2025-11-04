using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("衝突した相手: " + collision.gameObject.name + " / Tag: " + collision.gameObject.tag);
        if (hasActivated) return;

        // 衝突したオブジェクト
        GameObject obj = collision.gameObject;

        // 子や親のどこかが Player タグならOK
        bool isPlayer = obj.CompareTag("Player")
            || obj.transform.root.CompareTag("Player");


        //  Playerでなければ無視
        if (!isPlayer)
        {
            Debug.Log($"Player以外({obj.name})との衝突を無視しました。");
            return;
        }
        hasActivated = true;
        StartCoroutine(ActivatePlatformsSequentially());
    }
    private IEnumerator ActivatePlatformsSequentially()
    {
        for (int i = 0; i < platformsToActivate.Length; i++)
        {
            GameObject platform = platformsToActivate[i];

            // 足場を表示
            platform.SetActive(true);

            // 一定時間後に非表示にする
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
// Start is called before the first frame update

// Update is called once per frame

