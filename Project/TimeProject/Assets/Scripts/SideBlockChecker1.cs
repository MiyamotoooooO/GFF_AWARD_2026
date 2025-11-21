using UnityEngine;

public class SideBlockChecker : MonoBehaviour
{
    public enum CheckAxis
    {
        ForwardBack,
        LeftRight,
        UpDown
    }

    [Header("ブロックを置ける方向を指定")]
    public CheckAxis checkAxis = CheckAxis.ForwardBack;

    [Header("両側のブロックの名前を指定")]
    public string sideAName = "FrontBlock";
    public string sideBName = "BackBlock";

    [Header("くっついていると判定する距離")]
    public float contactDistance = 1.1f;

    [Header("くっついた瞬間にONにするスクリプトがあるオブジェクト")]
    public GameObject targetObject;

    [Header("ONにするスクリプト（targetObject上のスクリプトコンポーネントを直接指定）")]
    public MonoBehaviour targetScript;

    [Header("見た目の変更設定（マテリアル指定）")]
    public Material offMaterial;
    public Material onMaterial;

    private bool isCurrentlyConnected = false;
    private Renderer targetRenderer;
    private AudioSource chainAudio;

    void Start()
    {
        chainAudio = GetComponent<AudioSource>();

        if (targetObject != null)
        {
            targetRenderer = targetObject.GetComponent<Renderer>();
            if (targetRenderer != null && offMaterial != null)
                targetRenderer.material = offMaterial;
        }
    }

    void Update()
    {
        // 方向設定
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

        // 両サイドのブロックを Raycast で取得
        Transform blockA = FindNearbyBlock(dirA, sideAName);
        Transform blockB = FindNearbyBlock(dirB, sideBName);

        // 両方あったら接続判定
        bool nowConnected = (blockA != null && blockB != null);

        // ==========================
        // 接続した瞬間の処理
        // ==========================
        if (nowConnected && !isCurrentlyConnected)
        {
            Debug.Log("指定したブロックがくっつきました！");

            // スクリプトON
            if (targetScript != null)
            {
                targetScript.enabled = true;
                Debug.Log($"'{targetObject.name}' の '{targetScript.GetType().Name}' を ON にしました。");
            }

            // タグ変更
            if (targetObject != null)
            {
                targetObject.tag = "Ivent";
                Debug.Log($"'{targetObject.name}' のタグを 'Ivent' に変更しました。");
            }

            // 音
            if (chainAudio != null)
                chainAudio.Play();

            // 見た目変更
            if (targetRenderer != null && onMaterial != null)
                targetRenderer.material = onMaterial;

            // ==========================
            // 🔽 追加：両側のブロックのレイヤーを Default に戻す
            // ==========================
            if (blockA != null)
            {
                blockA.gameObject.layer = 0; // Default
                Debug.Log($"{blockA.name} のレイヤーを Default に変更しました。");
            }

            if (blockB != null)
            {
                blockB.gameObject.layer = 0; // Default
                Debug.Log($"{blockB.name} のレイヤーを Default に変更しました。");
            }
        }

        // 離れた瞬間
        if (!nowConnected && isCurrentlyConnected)
        {
            Debug.Log("ブロックが離れました。");
        }

        isCurrentlyConnected = nowConnected;
    }

    // 方向へ Raycast して指定名のブロックを探す
    Transform FindNearbyBlock(Vector3 direction, string targetName)
    {
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, contactDistance))
        {
            if (hit.collider.name == targetName)
                return hit.transform;
        }
        return null;
    }

    // Sceneビューに Ray を表示
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

