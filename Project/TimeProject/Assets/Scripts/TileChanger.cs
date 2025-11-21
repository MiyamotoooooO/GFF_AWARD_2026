using UnityEngine;

public class TileChanger : MonoBehaviour
{
    [Header("プレイヤーのタグ")]
    public string playerTag = "Player";

    [Header("渡り切ったら置き換わるプレハブ")]
    public GameObject newTilePrefab;

    [Header("生成位置オフセット（必要なら調整）")]
    public Vector3 spawnOffset = Vector3.zero;

    [Header("古いマスを消すかどうか")]
    public bool destroyOldTile = true;

    [Header("一度だけ反応するか")]
    public bool triggerOnce = true;

    private bool triggered = false;
    private AudioSource keikokuAudio;
    // プレイヤーがこのマスから離れたときに反応（isTriggerがOFFでも可）
    private void Start()
    {
        keikokuAudio = GetComponent<AudioSource>();
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(playerTag))
            if (keikokuAudio != null)
            {
                keikokuAudio.Play();
            }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (triggerOnce && triggered) return;
        if (!collision.gameObject.CompareTag(playerTag)) return;

        triggered = true;
        ChangeTile();

    }

    void ChangeTile()
    {
        if (newTilePrefab == null)
        {
            Debug.LogWarning($"{name}：newTilePrefab が設定されていません。");
            return;
        }

        Vector3 spawnPos = transform.position + spawnOffset;
        Quaternion spawnRot = transform.rotation;

        Instantiate(newTilePrefab, spawnPos, spawnRot);

        if (destroyOldTile)
        {
            Destroy(gameObject);
        }
    }
}



