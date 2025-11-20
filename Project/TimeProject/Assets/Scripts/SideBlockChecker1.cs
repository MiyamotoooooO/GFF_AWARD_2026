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

        Transform blockA = FindNearbyBlock(dirA, sideAName);
        Transform blockB = FindNearbyBlock(dirB, sideBName);

        bool nowConnected = (blockA != null && blockB != null);

        if (nowConnected && !isCurrentlyConnected)
        {
            Debug.Log("指定したブロックがくっつきました！");

            if (targetScript != null)
            {
                targetScript.enabled = true;
                Debug.Log($"'{targetObject.name}' の '{targetScript.GetType().Name}' をONにしました。");
            }

            // 🔽 タグを "Ivent" に変更（追加部分）
            if (targetObject != null)
            {
                targetObject.tag = "Ivent";
                Debug.Log($"'{targetObject.name}' のタグを 'Ivent' に変更しました。");
            }

            // 鎖音
            if (chainAudio != null)
            {
                chainAudio.Play();
            }

            if (targetRenderer != null && onMaterial != null)
            {
                targetRenderer.material = onMaterial;
            }
        }

        if (!nowConnected && isCurrentlyConnected)
        {
            Debug.Log("ブロックが離れました。");
        }

        isCurrentlyConnected = nowConnected;
    }

    Transform FindNearbyBlock(Vector3 direction, string targetName)
    {
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, contactDistance))
        {
            if (hit.collider.name == targetName)
                return hit.transform;
        }
        return null;
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
