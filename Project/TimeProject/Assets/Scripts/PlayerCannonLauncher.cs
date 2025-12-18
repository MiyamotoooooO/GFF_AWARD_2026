using System.Collections;
using UnityEngine;

public class PlayerCannonLauncher : MonoBehaviour
{
    [Header("player（Transform）")]
    public Transform player_;

    [Header("打ち上げ速度（上方向）")]
    public float launchUpSpeed = 15f;

    [Header("落下開始の高さ")]
    public float reappearHeight = 10f;

    [Header("クリックで狙うレイヤー")]
    public LayerMask defaultLayer;

    [Header("カメラ移動スクリプト（無効化対象）")]
    public MonoBehaviour cameraMoveScript;

    [Header("プレイヤーの移動スクリプト（無効化対象）")]
    public MonoBehaviour playerMoveScript;

    [Header("TAKOのスクリプトオフ(無効化対象)")]
    public MonoBehaviour takoMoveScript;

    [Header("発射エフェクトのプレハブ")]
    [SerializeField] private GameObject launchEffectPrefab;

    [Header("発射エフェクトの出現位置")]
    [SerializeField] private Transform effectSpawnPoint;

    [Header("着地地点用エフェクト")]
    [SerializeField] private GameObject landingEffectPrefab;

    [Header("発射サウンド")]
    [SerializeField] public AudioClip launchSound;

    [Range(0f, 3f)]
    public float launchVolume = 1f;    // ★ 発射音の音量調整

    [Header("着地サウンド")]
    [SerializeField] public AudioClip landingSound;

    [Range(0f, 3f)]
    public float landingVolume = 1f;    // ★ 発射音の音量調整

    [Header("サウンド再生用 AudioSource")]
    public AudioSource audioSource;

    private Rigidbody playerrb;
    private bool isInCannon = false;
    private bool isOnCannon = false;
    private bool isFlyingUp = false;
    private bool isFalling = false;
    private Vector3 targetPosition;
    private CheckPointManager checkPointManager;

    public void InjectCheckPointManager(CheckPointManager checkPointManager)
    {
        this.checkPointManager = checkPointManager;
    }

    void Start()
    {
        playerrb = player_.GetComponent<Rigidbody>();
    }

    void Update()
    {
        // 砲台に乗ってスペース → 入る
        if (isOnCannon && Input.GetKeyDown(KeyCode.Space) && !isInCannon)
            EnterCannon();

        // 大砲の中でクリック → 発射
        if (isInCannon && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 300f, defaultLayer))
            {
                targetPosition = hit.point;
                LaunchUpward();
            }
        }

        // 上昇中 → 一定高さ超えたら落下開始
        if (isFlyingUp && player_.position.y > reappearHeight)
        {
            StartCoroutine(ReappearAndFall());
            isFlyingUp = false;
        }

        // 💡 砲台側で「プレイヤーの着地」を監視
        if (isFalling)
        {
            CheckLanding();
        }
    }

    void EnterCannon()
    {
        playerrb.velocity = Vector3.zero;
        playerrb.angularVelocity = Vector3.zero;
        playerrb.isKinematic = true;

        player_.position = new Vector3(transform.position.x, transform.position.y + 1.0f, transform.position.z);

        isInCannon = true;

        if (playerMoveScript) playerMoveScript.enabled = false;
        if (takoMoveScript) takoMoveScript.enabled = false;

        Debug.Log("砲台に入りました");
    }

    void LaunchUpward()
    {
        checkPointManager.SetCannonState(true);
        playerrb.isKinematic = false;
        isInCannon = false;
        isFlyingUp = true;

        playerrb.velocity = Vector3.up * launchUpSpeed;
        // 🔥 発射アニメーション（Animator付きPrefab）再生
        if (launchEffectPrefab != null && effectSpawnPoint != null)
        {
            // ここで高さを +0.5f に調整（お好みで）
            Vector3 spawnPos = effectSpawnPoint.position + new Vector3(0, 1.4f, 0);

            GameObject effect = Instantiate(launchEffectPrefab, spawnPos, Quaternion.identity);

            // アニメーションが終わる想定時間後に削除
            Destroy(effect, 0.4f);
        }

        //発射サウンド再生
        if (audioSource && launchSound)
        {
            audioSource.PlayOneShot(launchSound, launchVolume);
            Debug.Log("発射サウンド再生");
        }

        if (cameraMoveScript) cameraMoveScript.enabled = false;

        Debug.Log("上方向に打ち上げ！");
    }

    IEnumerator ReappearAndFall()
    {
        playerrb.isKinematic = true;
        playerrb.velocity = Vector3.zero;

        Vector3 fallStartPos = new Vector3(targetPosition.x, reappearHeight, targetPosition.z);
        player_.position = fallStartPos;

        yield return new WaitForSeconds(0.5f);

        playerrb.isKinematic = false;
        playerrb.velocity = Vector3.zero;
        isFalling = true;

        Debug.Log("上から落下開始");
    }

    void CheckLanding()
    {
        // プレイヤーの足元にRayを飛ばして地面をチェック
        if (Physics.Raycast(player_.position, Vector3.down, out RaycastHit hit, 1.1f, defaultLayer))
        {
            float distanceToTarget = Vector3.Distance(
                new Vector3(player_.position.x, 0, player_.position.z),
                new Vector3(targetPosition.x, 0, targetPosition.z)
            );

            //落下速度をチェック
            float fallSpeed = Mathf.Abs(playerrb.velocity.y);
            Debug.Log($"地面ヒット！距離 {distanceToTarget}");

            // 地面が近く、落下が止まり、ターゲット付近なら着地判定
            if (distanceToTarget < 3.0f && fallSpeed < 0.1f)
            {
                StartCoroutine(WaitAndResume());
            }

        }
    }
    IEnumerator WaitAndResume()
    {
        // 少し待って本当に止まったか確認
        //yield return new WaitForSeconds(0f);

        float fallSpeed = Mathf.Abs(playerrb.velocity.y);
        if (fallSpeed > 0.1f) yield break; // まだ落下中なら中断

        Debug.Log("✅ 着地完了！スクリプト再開");

        // 着地エフェクト
        if (landingEffectPrefab)
        {
            Vector3 spawnPos = new Vector3(targetPosition.x, player_.position.y + 0.2f, targetPosition.z);
            GameObject effect = Instantiate(landingEffectPrefab, spawnPos, Quaternion.identity);
            Destroy(effect, 0.6f);
        }

        //着地サウンド再生
        if (audioSource && landingSound)
        {
            audioSource.PlayOneShot(landingSound, landingVolume);
            Debug.Log("着地サウンド再生");
        }

        isFalling = false;

        if (cameraMoveScript) cameraMoveScript.enabled = true;
        if (playerMoveScript) playerMoveScript.enabled = true;
        if (takoMoveScript) takoMoveScript.enabled = true;

        yield return new WaitForSeconds(0.5f);
        checkPointManager.SetCannonState(false);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Player")
        {
            isOnCannon = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.name == "Player")
        {
            isOnCannon = false;
        }
    }
}


