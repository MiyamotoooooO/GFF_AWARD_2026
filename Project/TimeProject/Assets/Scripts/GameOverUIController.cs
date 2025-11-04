using UnityEngine;
using UnityEngine.UI;

public class GameOverUIController : MonoBehaviour
{
    // UIが最終的に停止する画面中央のY座標
    // ※ インスペクターで設定するか、またはスクリプトで計算することも可能
    public float startYPosition = 100f;

    public float targetYPosition = 0f;

    // 落下にかかる時間（秒）
    public float dropDuration = 1.5f;

    // UIが落ちてくる速さ
    public float dropSpeed = 5f;

    // 落下アニメーションが始まったかどうかのフラグ
    private bool isDropping = false;

    // UIを初期設定で非表示にする（Canvas UIの場合はSetActive(false)など）

    // ゲームオーバー時に外部から呼び出す関数
    public void StartDropAnimation()
    {
        // 処理の準備（例：Canvas UIならSetActive(true)にする）
        gameObject.SetActive(true);

        Vector3 startPosition = transform.position;
        startPosition.y = startYPosition;
        transform.position = startPosition;

        isDropping = true;

        Debug.Log("落下のアニメーションを開始しました！");
    }

    void Update()
    {
        if (isDropping)
        {
            //Debug.Log($"現在のy座標:{transform.position.y:F2}, 目標y座標:{targetYPosition}");
            // 現在のY座標を取得
            float currentY = transform.position.y;

            // 目標位置に向かって移動（Lerpを使うと滑らかに減速する）
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, targetYPosition, transform.position.z), dropSpeed * Time.deltaTime);

            // Lerp（線形補間）を使ってより滑らかな動きを実現
            Vector3 targetPosition = new Vector3(transform.position.x, targetYPosition, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * dropSpeed);

            // ほぼ目標位置に到達したら停止する判定 (Lerpは完全に止まらないため)
            if (Mathf.Abs(transform.position.y - targetYPosition) < 0.01f)
            {
                transform.position = targetPosition; // 完全に目標位置に固定
                isDropping = false; // アニメーション終了
            }
        }
    }
}

