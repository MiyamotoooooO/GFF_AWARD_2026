using System.Collections;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    public float fallDelay = 0.5f;       // 乗ってから落ちるまでの時間
    public float returnDelay = 3f;       // 落ちてから戻るまでの時間
    public float fallDistance = 3f;      // 沈む距離
    public float moveSpeed = 2f;         // 落下／上昇スピード共通

    private Vector3 startPos;             // 初期位置
    private bool isMoving = false;

    private Transform playerParentCache;  // プレイヤーの元の親
    private Transform playerOnPlatform;   // 床に乗っているプレイヤー

    void Start()
    {
        startPos = transform.position;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // プレイヤーが乗ったら反応（タグで判定）
        if (collision.gameObject.CompareTag("Player") && !isMoving)
        {
            playerOnPlatform = collision.transform;
            playerParentCache = playerOnPlatform.parent;
            playerOnPlatform.SetParent(transform); // プレイヤーを床の子にする

            StartCoroutine(FallAfterDelay());
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // 床から離れたら親子関係を戻す
        if (collision.gameObject.CompareTag("Player"))
        {
            if (playerOnPlatform != null)
            {
                playerOnPlatform.SetParent(playerParentCache);
                playerOnPlatform = null;
            }
        }
    }

    IEnumerator FallAfterDelay()
    {
        yield return new WaitForSeconds(fallDelay);
        isMoving = true;

        Vector3 targetPos = startPos + Vector3.down * fallDistance;

        // 下に沈む
        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(returnDelay);

        // 上に戻る
        while (Vector3.Distance(transform.position, startPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, startPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        isMoving = false;

        // プレイヤーを元の親に戻す（上昇完了後）
        if (playerOnPlatform != null)
        {
            playerOnPlatform.SetParent(playerParentCache);
            playerOnPlatform = null;
        }
    }
}

