using UnityEngine;
using System.Collections;
public class ItemTransformer : MonoBehaviour
{
    [Header("プレイヤーのタグ")]
    public string playerTag = "Player"; // プレイヤー判定用

    [Header("判定するアイテムのタグ")]
    public string targetTag = "Selectable"; // このタグのオブジェクトを検出

    [Header("生成されるアイテム（プレハブ）")]
    public GameObject resultPrefab;   // 生成するオブジェクト

    [Header("クラフト範囲設定")]
    public float craftRadius = 2.0f; // プレイヤーが範囲内にいるかの判定
    public Color gizmoColor = new Color(0f, 1f, 1f, 0.3f); // Gizmoの色（シアン）

    [Header("アイテム検出範囲（半径）")]
    public float detectRadius = 1.0f;

    [Header("生成位置（未設定ならこのオブジェクト位置）")]
    public Transform spawnPoint;

    [Header("生成高さオフセット（Y方向）")]
    public float spawnHeightOffset = 0f;

    [Header("エフェクト設定")]
    public GameObject effectPrefab;          // エフェクトのプレハブ
    public float effectHeightOffset = 0f;    // エフェクトの高さオフセット
    public float effectLifetime = 2f;        // 自動削除までの時間（秒）

    [Header("変換後、元のアイテムを削除する")]
    public bool consumeOriginal = true;

    [Header("変換可能間隔（秒）")]
    public float cooldown = 1.0f;

    [Header("クラフト操作キー")]
    public KeyCode craftKey = KeyCode.Space; // スペースキーでクラフト

    private float lastTransformTime = -10f;
    private bool playerInRange = false; // プレイヤーが範囲内にいるか
    [Header("サウンド時間")]
    public float craftSoundDuration = 0f; // ← 再生したい時間（秒）
    private AudioSource craftAudio;
    void Start()
    {
        craftAudio = GetComponent<AudioSource>();
        if (spawnPoint == null) spawnPoint = transform;
    }

    void Update()
    {
        // プレイヤーが範囲内にいるかをチェック
        CheckPlayerInRange();

        // プレイヤーが範囲内にいるときのみクラフト可能
        if (playerInRange && Input.GetKeyDown(craftKey))
        {
            TryTransform();
            if (craftAudio != null)
            {
                StartCoroutine(PlayCraftSoundLimited());
            }
        }
    }
    private IEnumerator PlayCraftSoundLimited()
    {
        craftAudio.time = 0f;   // 先頭から再生
        craftAudio.Play();

        yield return new WaitForSeconds(craftSoundDuration);

        craftAudio.Stop();
    }
    void CheckPlayerInRange()
    {
        playerInRange = false;
        Collider[] hits = Physics.OverlapSphere(transform.position, craftRadius);

        foreach (var hit in hits)
        {
            if (hit.CompareTag(playerTag))
            {
                playerInRange = true;
                break;
            }

        }
    }

    void TryTransform()
    {
        if (Time.time - lastTransformTime < cooldown) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, detectRadius);

        foreach (var hit in hits)
        {
            if (hit.CompareTag(targetTag))
            {
                if (consumeOriginal)
                {
                    Destroy(hit.gameObject);
                }

                // 生成位置
                Vector3 spawnPos = spawnPoint.position + new Vector3(0, spawnHeightOffset, 0);

                // アイテム生成
                Instantiate(resultPrefab, spawnPos, spawnPoint.rotation);
                if (craftAudio != null)
                {
                    craftAudio.Stop();
                }
                // エフェクト生成
                if (effectPrefab != null)
                {

                    Vector3 effectPos = spawnPoint.position + new Vector3(0, effectHeightOffset, 0);
                    GameObject fx = Instantiate(effectPrefab, effectPos, Quaternion.identity);
                    Debug.Log($"{gameObject.name} がエフェクト {effectPrefab.name} を生成しました ({effectPos})");

                    if (effectLifetime > 0f)
                        Destroy(fx, effectLifetime);
                }

                Debug.Log($"{gameObject.name} が {targetTag} を {resultPrefab.name} に変換しました！");

                lastTransformTime = Time.time;
                break;
            }

        }
    }

    // Sceneビューで範囲を可視化
    void OnDrawGizmosSelected()
    {
        // クラフト範囲
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, craftRadius);

        // アイテム検出範囲（線）
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }
}




