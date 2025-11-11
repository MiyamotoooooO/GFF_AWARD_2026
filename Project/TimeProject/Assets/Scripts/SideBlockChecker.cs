using UnityEngine;

public class SideBlockChecker : MonoBehaviour
{
    [Header("手前と奥のブロックのタグを指定")]
    public string frontTag = "FrontBlock"; // 手前（カメラ側）のブロックのタグ
    public string backTag = "BackBlock";   // 奥（画面奥側）のブロックのタグ

    [Header("くっついていると判定する距離")]
    public float contactDistance = 1.1f; // Cubeが1×1×1なら1.1がちょうど良い

    private bool isCurrentlyConncted = false;

    void Update()
    {
        // 手前と奥のブロックを探す
        Transform frontBlock = FindNearbyBlock(Vector3.back, frontTag);    // 手前方向（カメラ側）
        Transform backBlock = FindNearbyBlock(Vector3.forward, backTag);   // 奥方向

        bool isFrontConnected = frontBlock != null;
        bool isBackConnected = backBlock != null;

        bool nowConnected = isFrontConnected && isBackConnected;

        //くっついた瞬間
        if (nowConnected && !isCurrentlyConncted)
        {
            Debug.Log("指定したブロックがくっつきました！");
        }
        if (!nowConnected && isBackConnected)
        {
            Debug.Log("ブロックが離れました！");
        }
        //状態を更新
        isCurrentlyConncted = nowConnected;
    }
    // 指定方向に特定タグのブロックがあるかを調べる
    Transform FindNearbyBlock(Vector3 direction, string tag)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, contactDistance))
        {
            if (hit.collider.CompareTag(tag))
                return hit.transform;
        }
        return null;
    }

    // Sceneビューで可視化
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.back * contactDistance);     // 手前
        Gizmos.DrawLine(transform.position, transform.position + Vector3.forward * contactDistance);  // 奥
    }
}

