using UnityEngine;

/// <summary>
/// ブロックの上を通れないようにするスクリプト。
/// 上面に不可視のコライダーを自動生成し、SceneビューでGizmosを表示。
/// </summary>
[RequireComponent(typeof(Collider))]
public class BlockTopBlocker : MonoBehaviour
{
    [Header("上面を塞ぐコライダーの厚み（Y方向）")]
    public float thickness = 0.2f;

    [Header("上面コライダーの高さオフセット（ブロック頂点より上に置く場合は正）")]
    public float verticalOffset = 0.0f;

    [Header("コライダーを少し内側へ縮める（X,Z方向）")]
    public Vector2 inset = new Vector2(0.02f, 0.02f);

    [Header("作成する子オブジェクト名（デバッグ用）")]
    public string blockerName = "TopBlocker";

    [Header("既に同名の子がある場合は置き換える")]
    public bool replaceIfExists = true;

    // Gizmos用キャッシュ
    private Bounds blockBounds;

    void Start()
    {
        CreateTopBlocker();
    }

    void CreateTopBlocker()
    {
        Collider col = GetComponent<Collider>();
        blockBounds = col.bounds;

        // 既存チェック
        Transform existing = transform.Find(blockerName);
        if (existing != null)
        {
            if (replaceIfExists)
                Destroy(existing.gameObject);
            else
                return;
        }

        // 子オブジェクト作成
        GameObject top = new GameObject(blockerName);
        top.transform.parent = transform;

        float sizeX = Mathf.Max(0.01f, blockBounds.size.x - inset.x * 2f);
        float sizeZ = Mathf.Max(0.01f, blockBounds.size.z - inset.y * 2f);
        float sizeY = Mathf.Max(0.01f, thickness);

        Vector3 topCenterWorld = blockBounds.center + Vector3.up * (blockBounds.extents.y + sizeY * 0.5f + verticalOffset);

        top.transform.position = topCenterWorld;
        top.transform.rotation = transform.rotation;

        BoxCollider box = top.AddComponent<BoxCollider>();
        Vector3 localSize = new Vector3(sizeX / transform.lossyScale.x, sizeY / transform.lossyScale.y, sizeZ / transform.lossyScale.z);
        box.size = localSize;
        box.center = Vector3.zero;
        box.isTrigger = false;

        Rigidbody rb = top.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    // --- Gizmos 表示部分 ---
    void OnDrawGizmosSelected()
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        Bounds b = col.bounds;
        float sizeX = Mathf.Max(0.01f, b.size.x - inset.x * 2f);
        float sizeZ = Mathf.Max(0.01f, b.size.z - inset.y * 2f);
        float sizeY = Mathf.Max(0.01f, thickness);

        Vector3 topCenterWorld = b.center + Vector3.up * (b.extents.y + sizeY * 0.5f + verticalOffset);

        // 半透明の赤い箱で表示
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawCube(topCenterWorld, new Vector3(sizeX, sizeY, sizeZ));

        // 枠線（濃い赤）で表示
        Gizmos.color = new Color(1f, 0f, 0f, 1f);
        Gizmos.DrawWireCube(topCenterWorld, new Vector3(sizeX, sizeY, sizeZ));
    }
}

