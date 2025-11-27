using System.Diagnostics.CodeAnalysis;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Rigidbody), typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }
    public static bool IsDead { get; private set; } = false;

    [Tooltip("移動速度")]
    [SerializeField] private float moveSpeed = 5f;       // 移動速度

    [Tooltip("アニメーション用")]
    [SerializeField] private Animator animator;          // Animator コンポーネント

    [Header("UI設定")]
    [SerializeField] private GameObject spaceUI;
    private GameObject spawonUI;

    private Rigidbody rb;             // 3D物理用 Rigidbody
    private SpriteRenderer sr;        // キャラの見た目（左右反転用）
    private AudioSource footstepAudio;   // ← 足音用

    public OxygenGaugeController oxygenGaugeController;
    private Vector3 lastCheckpointPosition;
    public Collider planeCollider;
    public static string LastTouchedObjectName { get; private set; } = "UnknownCause";

    private bool hasTouchedfalling = false;
    private bool isOnfalling = false;
    private bool isGameEnded = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        sr = GetComponent<SpriteRenderer>(); // SpriteRendererを取得
        Collider playerCollider = GetComponent<Collider>(); // プレイヤーのcolliderを取得

        animator = GetComponent<Animator>();


        // 足音用 AudioSource（Playerについているやつ）
        footstepAudio = GetComponent<AudioSource>();
        footstepAudio.loop = true;       // 足音をループ再生
        footstepAudio.playOnAwake = false;

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

        // 足音制御（歩いてるときだけ再生）
        HandleFootsteps(speed);
        // 向き変更（左右だけ反転）
        if (moveX != 0)
        {
            sr.flipX = moveX < 0; // 左に進んでいるときだけ反転
        }
    }
    private void HandleFootsteps(float speed)
    {
        // 歩いている & 足音が再生されていない → 再生
        if (speed > 0.1f)
        {
            if (!footstepAudio.isPlaying)
                footstepAudio.Play();
        }
        else
        {
            // 止まっている → 足音停止
            if (footstepAudio.isPlaying)
                footstepAudio.Stop();
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

        if (isGameEnded == true) return;
        string hitName = collision.gameObject.name;

        FallingItem DeathCauser = collision.gameObject.GetComponent<FallingItem>();
        if (DeathCauser != null)
        {
            string prefabName = DeathCauser.GetDeathCauseName();

            string finalCauseName = prefabName.Replace("(clone)", "").Trim();

            hasTouchedfalling = false;
            //HandleDeath(finalCauseName);

            return;
        }
        else if (hitName == "stone")
        {
            if (OxygenGaugeController.Instance != null)
            {
                SetNonContactDeathCause("stone");
                Debug.Log("stoneを取得しました。");
                OxygenGaugeController.Instance.GameOverUI();
            }
        }
        else if (hitName == "falling")
        {
            Debug.Log("fallingに乗りました。");
            isOnfalling = true;
        }
        else if (hitName == "Water")
        {
            if (isOnfalling == true)
            {
                isGameEnded = true;
                SetNonContactDeathCause("falling");
                Debug.Log("fallingに乗ってから落ちました。");
                OxygenGaugeController.Instance.GameOverUI();
                return;
            }
            else if (oxygenGaugeController != null)
            {
                isGameEnded = true;
                if (LastTouchedObjectName == "falling") return;
                Debug.Log("Waterが呼ばれました。");
                SetNonContactDeathCause("Water");
                oxygenGaugeController.GameOverUI();
            }
        }
        else
        {
            LastTouchedObjectName = hitName;
        }

        if (collision.gameObject.tag == "Ivent")
        {
            spawonUI = Instantiate(spaceUI);
            Vector3 spawonPos = spawonUI.transform.position;
            spawonPos.x = collision.transform.position.x;
            spawonPos.y = collision.transform.position.y + 1.5f;
            spawonPos.z = collision.transform.position.z;
            spawonUI.transform.localPosition = spawonPos;
        }

        LastTouchedObjectName = collision.gameObject.name;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Ivent")
        {
            Destroy(spawonUI);
        }

        //if(collision.gameObject.name == "falling")
        //{
        //  isOnfalling = false;
        //}
    }

    private void OnTriggerEnter(Collider other)
    {
        LastTouchedObjectName = other.gameObject.name;

    }

    public static void SetNonContactDeathCause(string causeName)
    {
        LastTouchedObjectName = causeName;
    }

}


