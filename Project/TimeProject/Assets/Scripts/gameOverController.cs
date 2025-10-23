using UnityEngine;

public class GameOverUIController : MonoBehaviour
{
    // UIが最終的に停止する画面中央のY座標
    // Canvas UIを使っている場合は、0に近い値で画面中央になります。
    public float targetYPosition = 0f;

    // UIが滑らかに動く速さ（値が大きいほど目標位置に早く近づきます）
    public float dropSpeed = 5f;

    // 落下アニメーションが始まったかどうかのフラグ
    private bool isDropping = false;

    // ゲームオーバー時に外部（OxygenGaugeController）から呼び出す関数
    public void StartDropAnimation()
    {
        // 1. UIゲームオブジェクトを有効化する
        gameObject.SetActive(true);

        // 2. アニメーションを開始するフラグを立てる
        isDropping = true;

        // 3. 最初の位置を設定（必要に応じて）
    }

    void Update()
    {
        if (isDropping)
        {
            // 目標位置 (targetPosition) を設定
            Vector3 targetPosition = new Vector3(transform.position.x, targetYPosition, transform.position.z);

            // 線形補間（Lerp）を使って目標位置に向かって滑らかに移動させる
            // Time.deltaTime * dropSpeed によって、フレームレートに依存しない一定の速度で滑らかに動きます
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * dropSpeed);

            // ほぼ目標位置に到達したら停止する判定 (Lerpは完全に止まらないため、閾値で判定)
            if (Mathf.Abs(transform.position.y - targetYPosition) < 0.01f)
            {
                // 完全に目標位置に固定
                transform.position = targetPosition;
                // アニメーション終了
                isDropping = false;
            }
        }
    }
}
