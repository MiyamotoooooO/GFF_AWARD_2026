using UnityEngine;
using System.Collections;

public class CollisionTeleporter : MonoBehaviour
{
    [Tooltip("このテレポートゾーンとペアになるテレポートゾーンを設定してください")]
    public CollisionTeleporter partnerTeleporter;

    [Tooltip("テレポート後に再テレポートを無効化する時間（秒）")]
    public float coolDownTime = 2f;

    private bool isCoolingDown = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isCoolingDown && partnerTeleporter != null)
        {
            // プレイヤーをペアの位置に移動
            collision.transform.position = partnerTeleporter.transform.position + Vector3.up * 1.0f;

            // 相手側のクールダウン開始
            StartCoroutine(partnerTeleporter.StartCoolDownInternal());

            // 自身のクールダウン開始
            isCoolingDown = true;
            StartCoroutine(StopCoolDownFlag());
        }
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
