using System.Diagnostics.CodeAnalysis;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Rigidbody), typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [Tooltip("移動速度")]
    [SerializeField] private float moveSpeed = 5f;       // 移動速度

    [Tooltip("アニメーション用")]
    [SerializeField] private Animator animator;          // Animator コンポーネント

    private Rigidbody rb;             // 3D物理用 Rigidbody
    private SpriteRenderer sr;        // キャラの見た目（左右反転用）
    private bool isInputEnabled = true;
    public OxygenGaugeController oxygenGaugeController;
    private Vector3 lastCheckpointPosition;
    public Collider planeCollider;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        sr = GetComponent<SpriteRenderer>(); // SpriteRendererを取得
        Collider playerCollider = GetComponent<Collider>(); // プレイヤーのcolliderを取得

        animator = GetComponent<Animator>();
        Application.targetFrameRate = 60;

        GameObject[] planes = GameObject.FindGameObjectsWithTag("Plane");
        if (planeCollider != null)
        {
            Physics.IgnoreCollision(playerCollider, planeCollider, true);
            Debug.Log("Planeとの衝突を永続的に無視する設定にしました。");
        }
    }

    void Update()
    {
        // 入力取得（左右＋前後）
        float moveX = Input.GetAxisRaw("Horizontal"); // A/Dキー, ←/→
        float moveZ = Input.GetAxisRaw("Vertical");   // W/Sキー, ↑/↓

        // 入力方向をベクトルに
        Vector3 moveDir = new Vector3(moveX, 0, moveZ).normalized;

        // 移動処理
        rb.MovePosition(transform.position + moveDir * moveSpeed * Time.deltaTime);

        // アニメーション制御
        float speed = moveDir.magnitude;         // 入力の強さ
        animator.SetFloat("Speed", speed);       // "Speed" に渡す

        // 向き変更（左右だけ反転）
        if (moveX != 0)
        {
            sr.flipX = moveX < 0; // 左に進んでいるときだけ反転
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Plane")
        {
            Debug.Log("Planeとの衝突を検出しました。");
            // 衝突点の法線ベクトルに沿ってプレイヤーを押す力をゼロにする
            Physics.IgnoreCollision(GetComponent<Collider>(), collision.collider, true);
        }

        if (collision.gameObject.name == "Water")
        {
            Debug.Log("Waterに接触！");

            if (oxygenGaugeController != null)
            {
                oxygenGaugeController.GameOverUI();
            }
        }
    }

    public void DisableInput()
    {
        isInputEnabled = false;
    }
}
