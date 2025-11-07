using UnityEngine;

public class _SideBlockChecker : MonoBehaviour
{
    [Header("左右のブロックを指定")]
    public Transform leftBlock;   // 左に置く特定のブロック
    public Transform rightBlock;  // 右に置く特定のブロック

    [Header("くっついていると判定する距離")]
    public float contactDistance = 1.1f;  // Cubeが1×1×1なら1.1がちょうど良い

    private bool alreadyLogged = false;

    void Update()
    {
        // どちらか未指定なら警告を出してスキップ
        if (leftBlock == null || rightBlock == null)
        {
            Debug.LogWarning("左右のブロックをInspectorで指定してください。");
            return;
        }

        // 左右ブロックとの距離を計算
        float leftDistance = Vector3.Distance(transform.position, leftBlock.position);
        float rightDistance = Vector3.Distance(transform.position, rightBlock.position);

        // 各ブロックが「くっついている」か判定
        bool isLeftConnected = leftDistance <= contactDistance;
        bool isRightConnected = rightDistance <= contactDistance;

        // 両方ともくっついていたらログ
        if (isLeftConnected && isRightConnected)
        {
            if (!alreadyLogged)
            {
                Debug.Log("指定した左右のブロックが中央にくっついています！");
                alreadyLogged = true;
            }
        }
        else
        {
            alreadyLogged = false;
        }
    }

    // Sceneビューで線を可視化
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        if (leftBlock != null)
            Gizmos.DrawLine(transform.position, leftBlock.position);
        if (rightBlock != null)
            Gizmos.DrawLine(transform.position, rightBlock.position);
    }
}

