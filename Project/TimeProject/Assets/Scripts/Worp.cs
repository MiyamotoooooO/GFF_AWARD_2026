using UnityEngine;
using System.Collections;

public class CollisionTeleporter : MonoBehaviour
{
    [Tooltip("このテレポートゾーンとペアになるテレポートゾーンを設定してください")]
    public CollisionTeleporter partnerTeleporter;

    [Tooltip("テレポート後に再テレポートを無効化する時間（秒）")]
    public float coolDownTime = 2f;

    private bool isCoolingDown = false;
    private bool isPlayerOnTeleporter = false; // ← プレイヤーが乗っているか判定用
    private AudioSource worpaudio; //マス音

    private void Start()
    {
        worpaudio = GetComponent<AudioSource>();
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerOnTeleporter = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerOnTeleporter = false;
        }
    }

    private void Update()
    {
        // プレイヤーが乗っていて、スペースキーが押されて、クールダウン中でない場合
        if (isPlayerOnTeleporter && Input.GetKeyDown(KeyCode.Space) && !isCoolingDown)
        {
            TeleportPlayer();
        }
    }

    private void TeleportPlayer()
    {
        // マス生成音（このオブジェクトから鳴らす）
        if (worpaudio != null)
        {
            worpaudio.Play();
        }
        if (partnerTeleporter == null) return;

        // プレイヤーを取得
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null) return;

        // プレイヤーをペアの位置へ移動（少し上に位置調整）
        player.transform.position = partnerTeleporter.transform.position + Vector3.up * 1.0f;

        // クールダウン開始
        isCoolingDown = true;
        StartCoroutine(StopCoolDownFlag());

        // 相手側もクールダウン開始
        StartCoroutine(partnerTeleporter.StartCoolDownInternal());
    }

    public IEnumerator StartCoolDownInternal()
    {
        isCoolingDown = true;
        yield return new WaitForSeconds(coolDownTime);
        isCoolingDown = false;
    }

    IEnumerator StopCoolDownFlag()
    {
        yield return new WaitForSeconds(coolDownTime);
        isCoolingDown = false;
    }
}

