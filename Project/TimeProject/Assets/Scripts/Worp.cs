using UnityEngine;
using System.Collections;

public class CollisionTeleporter : MonoBehaviour
{
    [Tooltip("テレポート先となるゾーンのタグ名を設定してください。")]
    public string destinationTag;

    [Tooltip("テレポート後に再テレポートを無効果にする時間（秒）")]
    public float coolDownTime = 2f;

    // 自身のColliderへの参照
    //private Collider ownCollider;
    private bool isCoolingDown = false;

    // Fixedbodyへ処理を引き渡すためのフラグとテレポート先
    private bool needsTeleport = false;
    private Rigidbody playerRigidbody;
    private Vector3 targetPosition;

    // 検索で見つけたテレポート先のTransform
    private Transform otherDestination;
    void Start()
    {
        // アタッチしているColliderを取得
        //ownCollider = GetComponent<Collider>();

        // 1,指定されたタグを持つオブジェクトをシーンから検索する
        GameObject destObject = GameObject.FindWithTag(destinationTag);

        if (destObject != null)
        {
            otherDestination = destObject.transform;
            Debug.Log(gameObject.name + "のテレポート先: " + destObject.name);
        }
        else
        {
            Debug.LogError(gameObject.name + "に対応するテレポート先 (" + destinationTag + "タグ)が見つかりませんでした！");
        }
    }

    // 衝突（Is Trigger: OFF）が発生したときに呼ばれる
    private void OnCollisionEnter(Collision collision)
    {
        // プレイヤーオブジェクトであり、かつクールダウン中でないか確認
        if (collision.gameObject.CompareTag("Player") && !isCoolingDown && otherDestination != null)
        {
            // プレイヤーのRigidbodyを記憶
            playerRigidbody = collision.rigidbody;

            // ターゲット座標を設定(着地計算はFizedUpdateで行う)
            targetPosition = otherDestination.position;

            // テレポート依頼フラグを立てる
            needsTeleport = true;

            // 1,プレイヤーをテレポート先の位置に移動
            collision.transform.position = otherDestination.position + Vector3.up * 1.0f;

            // 2,相手側のゾーンのColliderを無効化する処理を実行
            CollisionTeleporter destinationTeleporter = otherDestination.GetComponent<CollisionTeleporter>();
            if (destinationTeleporter != null)
            {
                //相手側のクールダウンを開始
                StartCoroutine(destinationTeleporter.StartCoolDownInternal());
            }
            // 3,自身のクールダウンフラグを立てる
            isCoolingDown = true;
            StartCoroutine(StopCoolDownFlag());
        }
    }

    // 物理演算の更新フレームでテレポートを実行する
    void FixedUpdate()
    {
        if (needsTeleport && playerRigidbody != null)
        {
            // プレイヤーのCapsule Collider情報を取得(着地計算用)
            CapsuleCollider capsule = playerRigidbody.GetComponent<CapsuleCollider>();

            if (capsule != null)
            {
                // 着地座標の修正
                float halfHeight = capsule.height;// / 1f;
                targetPosition.y += halfHeight;

                // Rigidbody専用のメソッドで瞬間移動を実行
                playerRigidbody.position = targetPosition;

                // 貫通を防ぐため、速度をリセット
                playerRigidbody.velocity = Vector3.zero;

                playerRigidbody.angularVelocity = Vector3.zero;
            }

            // 処理完了
            needsTeleport = false;
            playerRigidbody = null;
        }
    }

    // 相手側のテレポートゾーンから呼ばれるColliderを無効化するクールダウン
    public IEnumerator StartCoolDownInternal()
    {
        isCoolingDown = true;

        // 自分のColliderを無効化！衝突検出を物理的に不可能にする
        //if (ownCollider != null) ownCollider.enabled = true;

        Debug.Log(gameObject.name + "Collider OFF");
        yield return new WaitForSeconds(coolDownTime);

        // 待機時間が終わればフラグをOFFに戻し、テレポートを再度有効化
        //if (ownCollider != null) ownCollider.enabled = true;
        Debug.Log(gameObject.name + "Collider ON");
        isCoolingDown = false;
    }

    // 自身がテレポートされた後のクールダウンタイムフラグを処理するコルーチン
    IEnumerator StopCoolDownFlag()
    {
        yield return new WaitForSeconds(coolDownTime);
        isCoolingDown = false;
    }
}

