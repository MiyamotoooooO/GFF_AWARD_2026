using System.Collections;
using UnityEngine;

public class rakka : MonoBehaviour
{
    [Header("判定")]
    public string playerTag = "Player";     // プレイヤーに付けたタグ

    [Header("生成設定")]
    public GameObject itemPrefab;           // 降らせたいプレハブ
    public float spawnHeight = 5f;          // マスの上から何ユニット上に生成するか
    public float spawnDelay = 0f;           // 踏んでから生成までの遅延（秒）
    public float cooldown = 1f;             // 再生成までの待ち時間（秒）
    public bool preventOverlap = true;      // 既に同じアイテムが近くにあったら生成しない

    [Header("オプション")]
    public float overlapCheckRadius = 0.6f; // preventOverlap の判定半径
    public LayerMask overlapLayer = ~0;     // チェックするレイヤー（デフォルト全て）

    bool canSpawn = true;

    // OnTriggerEnter は使わない（削除）
    // void OnTriggerEnter(Collider other) { ... }

    // isTrigger OFF の物理衝突のみで判定
    void OnCollisionEnter(Collision collision)
    {
        if (!canSpawn) return;
        if (collision.collider.CompareTag(playerTag))
            StartCoroutine(DoSpawn());
    }

    IEnumerator DoSpawn()
    {
        canSpawn = false;

        if (spawnDelay > 0f)
            yield return new WaitForSeconds(spawnDelay);

        if (itemPrefab != null)
        {
            Vector3 spawnPos = transform.position + Vector3.up * spawnHeight;

            bool okToSpawn = true;
            if (preventOverlap)
            {
                Collider[] hits = Physics.OverlapSphere(spawnPos, overlapCheckRadius, overlapLayer);
                if (hits != null && hits.Length > 0) okToSpawn = false;
            }

            if (okToSpawn)
            {
                GameObject spawned = Instantiate(itemPrefab, spawnPos, Quaternion.identity);

                Rigidbody rb = spawned.GetComponent<Rigidbody>();
                if (rb == null)
                    rb = spawned.AddComponent<Rigidbody>();

                FallingItem fi = spawned.AddComponent<FallingItem>();
                fi.playerTag = playerTag;
            }
        }
        else
        {
            Debug.LogWarning($"rakka: itemPrefab が設定されていません（{name}）。");
        }

        yield return new WaitForSeconds(cooldown);
        canSpawn = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * spawnHeight, overlapCheckRadius);
    }
}

// 補助スクリプト FallingItem は変更不要
public class FallingItem : MonoBehaviour
{
    [HideInInspector] public string playerTag;

    [Header("地面のタグ設定")]
    public string groundTag = "Ground";

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag(playerTag))
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                Destroy(rb);
            }
            return;
        }

        if (collision.collider.CompareTag(groundTag))
        {
            Destroy(gameObject);
        }
    }
}


