using UnityEngine;
using System.Collections;

public class SideBlockChecker : MonoBehaviour
{
    public enum CheckAxis
    {
        ForwardBack, // 奥 手前（Z軸）
        LeftRight,   // 左 右（X軸）
        UpDown       // 上 下（Y軸）
    }

    [Header("ブロックを置ける方向を指定")]
    public CheckAxis checkAxis = CheckAxis.ForwardBack;

    [Header("両側のブロックの名前を指定")]
    public string sideAName = "FrontBlock"; // 手前 or 左 or 下
    public string sideBName = "BackBlock";  // 奥 or 右 or 上

    [Header("くっついていると判定する距離")]
    public float contactDistance = 1.1f;

    [Header("Colliderを再有効化するまでの時間(秒)")]
    public float resetDelay = 0.05f;

    [Header("くっついたときに変更するタグ")]
    public string newTagOnConnect = "Player";

    [Header("Rigidbodyをつけておく時間(秒)")]
    public float rigidbodyDuration = 0.05f;

    [Header("Rigidbody設定")]
    public bool useGravity = false;
    public bool isKinematic = true;

    private bool isCurrentlyConnected = false;
    private string originalTag;
    private BoxCollider boxCollider;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            Debug.LogWarning("BoxColliderが見つかりません。このオブジェクトにBoxColliderを追加してください。");
        }

        originalTag = gameObject.tag;
    }

    void Update()
    {
        // ---- 向きに応じた方向ベクトル ----
        Vector3 dirA, dirB;
        switch (checkAxis)
        {
            case CheckAxis.LeftRight:
                dirA = Vector3.left;
                dirB = Vector3.right;
                break;
            case CheckAxis.UpDown:
                dirA = Vector3.down;
                dirB = Vector3.up;
                break;
            default:
                dirA = Vector3.back;
                dirB = Vector3.forward;
                break;
        }

        // ---- 両側のブロックを検出 ----
        Transform blockA = FindNearbyBlock(dirA, sideAName);
        Transform blockB = FindNearbyBlock(dirB, sideBName);

        bool nowConnected = (blockA != null && blockB != null);

        // ---- くっついた瞬間のみ ----
        if (nowConnected && !isCurrentlyConnected)
        {
            Debug.Log("指定したブロックがくっつきました！");

            if (boxCollider != null)
                StartCoroutine(ResetCollider());

            // タグ変更
            if (!string.IsNullOrEmpty(newTagOnConnect))
            {
                gameObject.tag = newTagOnConnect;
                Debug.Log($"タグを '{originalTag}' から '{newTagOnConnect}' に変更しました。");
            }

            // Rigidbodyを一瞬だけつける
            StartCoroutine(AddRigidbodyTemporarily());
        }

        // ---- 離れた瞬間 ----
        if (!nowConnected && isCurrentlyConnected)
        {
            gameObject.tag = originalTag;
            Debug.Log($"タグを '{newTagOnConnect}' から '{originalTag}' に戻しました。");
        }

        isCurrentlyConnected = nowConnected;
    }

    Transform FindNearbyBlock(Vector3 direction, string targetName)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, contactDistance))
        {
            if (hit.collider.name == targetName)
                return hit.transform;
        }
        return null;
    }

    IEnumerator ResetCollider()
    {
        boxCollider.enabled = false;
        yield return new WaitForSeconds(resetDelay);
        boxCollider.enabled = true;
    }

    // Rigidbodyを一瞬だけ追加して削除
    IEnumerator AddRigidbodyTemporarily()
    {
        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = useGravity;
        rb.isKinematic = isKinematic;

        Debug.Log("Rigidbodyを一時的に追加しました。");

        yield return new WaitForSeconds(rigidbodyDuration);

        Destroy(rb);
        Debug.Log("Rigidbodyを削除しました。");
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        Vector3 dirA, dirB;
        switch (checkAxis)
        {
            case CheckAxis.LeftRight:
                dirA = Vector3.left;
                dirB = Vector3.right;
                break;
            case CheckAxis.UpDown:
                dirA = Vector3.down;
                dirB = Vector3.up;
                break;
            default:
                dirA = Vector3.back;
                dirB = Vector3.forward;
                break;
        }

        Gizmos.DrawLine(transform.position, transform.position + dirA * contactDistance);
        Gizmos.DrawLine(transform.position, transform.position + dirB * contactDistance);
    }
}

