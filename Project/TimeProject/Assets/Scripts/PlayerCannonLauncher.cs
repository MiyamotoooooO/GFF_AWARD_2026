using System.Collections;
using UnityEngine;

public class PlayerCannonLauncher : MonoBehaviour
{
    [Header("砲台を設置した足場の位置（Transform）")]
    public Transform cannonBase; // 足場を指定

    [Header("打ち上げ速度（上方向）")]
    public float launchUpSpeed = 15f;

    [Header("落下開始の高さ")]
    public float reappearHeight = 20f;

    [Header("クリックで狙うレイヤー")]
    public LayerMask defaultLayer;

    [Header("カメラ移動スクリプト（無効化対象）")]
    public MonoBehaviour cameraMoveScript;

    [Header("プレイヤーの移動スクリプト（無効化対象）")]
    public MonoBehaviour playerMoveScript; // ← ★ここにプレイヤー移動スクリプトを入れる

    [Header("TAKOのスクリプトオフ(無効化対象)")]
    public MonoBehaviour takoMoveScript;

    private Rigidbody rb;
    private bool isInCannon = false;
    private bool isOnCannon = false;
    private bool isFlyingUp = false;
    private bool isFalling = false;
    private Vector3 targetPosition;
    private float standOffsetY = 1.0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // 砲台の上でスペースキーを押すと砲台に入る
        if (isOnCannon && Input.GetKeyDown(KeyCode.Space) && !isInCannon)
        {
            EnterCannon();
        }

        // 砲台に入った状態でクリックしたら打ち上げ
        if (isInCannon && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 300f, defaultLayer))
            {
                targetPosition = hit.point;
                LaunchUpward();
                Debug.Log(ray);
            }
        }

        // 上昇中に一定高さを超えたら再出現
        if (isFlyingUp && transform.position.y > reappearHeight)
        {
            StartCoroutine(ReappearAndFall());
            isFlyingUp = false;
        }
    }

    // 砲台に入る
    void EnterCannon()
    {
        Vector3 fixedPosition = cannonBase.position;
        fixedPosition.y += standOffsetY; // 足場の上に配置

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        transform.position = fixedPosition;

        isInCannon = true;

        // 🎮 プレイヤー移動スクリプトをオフ
        if (playerMoveScript != null)
        {
            playerMoveScript.enabled = false;
            Debug.Log("プレイヤー移動スクリプト停止（砲台に入った）");
        }
        // 🐾 ペット追従スクリプトをオフ
        if (takoMoveScript != null)
        {
            takoMoveScript.enabled = false;
            Debug.Log("ペット追従スクリプトを停止");
        }

        Debug.Log("砲台に入りました（足場基準）");
    }

    // 上方向に打ち上げ
    void LaunchUpward()
    {
        rb.isKinematic = false;
        isInCannon = false;
        isFlyingUp = true;

        rb.velocity = Vector3.up * launchUpSpeed;

        // 🎥 カメラスクリプトを停止
        if (cameraMoveScript != null)
        {
            cameraMoveScript.enabled = false;
            Debug.Log("カメラ移動スクリプト停止");
        }

        Debug.Log("上方向に打ち上げ！");
    }

    // 上空から再出現して落下
    IEnumerator ReappearAndFall()
    {
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;

        // クリック地点の真上から落下開始
        Vector3 fallStart = targetPosition + Vector3.up * reappearHeight;
        transform.position = fallStart;

        yield return new WaitForSeconds(0.5f);

        rb.isKinematic = false;
        rb.velocity = Vector3.zero;
        isFalling = true;

        Debug.Log("上から落下開始");   
    }

    void OnCollisionEnter(Collision collision)
    {
        // 足場(砲台)判定
        if (collision.gameObject.name == "Cannon")
        {
            isOnCannon = true;
            Debug.Log("砲台の上にいます");
        }

        // 落下→着地検知
        if (isFalling)
        {
            float distanceToTarget = Vector3.Distance(
                new Vector3(transform.position.x, 0, transform.position.z),
                new Vector3(targetPosition.x, 0, targetPosition.z)
            );

            if (distanceToTarget < 1.5f)
            {
                if (cameraMoveScript != null)
                {
                    cameraMoveScript.enabled = true;
                    Debug.Log("着地！カメラ再開");
                }

                // 🎮 プレイヤー移動スクリプトを再開
                if (playerMoveScript != null)
                {
                    playerMoveScript.enabled = true;
                    Debug.Log("プレイヤー移動スクリプト再開");
                }
                if (takoMoveScript != null)
                {
                    takoMoveScript.enabled = true;
                    Debug.Log("ペット追従スクリプトを再開");
                }
                isFalling = false;
            }
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Cannon"))
        {
            isOnCannon = false;
            Debug.Log("砲台から離れました");
        }
    }
}


