using System.Collections;
using UnityEngine;

public class GoalCamera : MonoBehaviour
{
    [Header("参照オブジェクト")]
    public Camera mainCamera;          // メインカメラ
    public Transform player;           // プレイヤー
    public Transform nextStagePoint;   // 次ステージ方向の目印

    [Header("カメラ移動設定")]
    public float moveDistance = 0f;    // スライド距離
    public float moveDuration = 0f;    // カメラ移動にかける時間（行きも戻りも）
    public float showTime = 0f;        // 次ステージを見せる停止時間
    public float backXOffset = 0f;   // カメラをZ方向に引く距離
    public float backZOffset = 0f;   // カメラをZ方向に引く距離

    private bool hasCleared = false;
    private Vector3 cameraOffset;

    // 🔸 プレイヤー追尾カメラスクリプトへの参照
    private MonoBehaviour followCameraScript;
    private MonoBehaviour ControllerScript;

    void Start()
    {
        if (mainCamera != null && player != null)
        {
            cameraOffset = mainCamera.transform.position - player.position;
        }

        // 🔹 カメラにアタッチされている追尾スクリプト（例：FollowCamera）を自動取得
        followCameraScript = mainCamera.GetComponent<MonoBehaviour>();
        ControllerScript = player.GetComponent<MonoBehaviour>();

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            OnStageClear();
        }
    }

    public void OnStageClear()
    {
        if (hasCleared) return;
        hasCleared = true;
        StartCoroutine(CameraMoveRoutine());
    }

    private IEnumerator CameraMoveRoutine()
    {
        // 🔸 追尾カメラを一時停止
        if (followCameraScript != null) followCameraScript.enabled = false;

        if (ControllerScript != null) ControllerScript.enabled = false;

        // カメラの元位置
        Vector3 startPos = mainCamera.transform.position;

        // 次ステージ方向ベクトル
        Vector3 dir = (nextStagePoint.position - startPos).normalized;

        // Z方向に少し引きながら移動
        Vector3 targetPos = startPos + dir * moveDistance + new Vector3(backXOffset, 0, backZOffset);

        // ① スライドして次ステージを見せる
        yield return StartCoroutine(SmoothMoveCamera(startPos, targetPos, moveDuration));

        // ② 一定時間停止（showTimeで制御）
        yield return new WaitForSeconds(showTime);

        // ③ 元の位置に戻す（同じスピード）
        yield return StartCoroutine(SmoothMoveCamera(targetPos, startPos, moveDuration));

        // 🔹 再び追尾カメラをオンに戻す
        if (followCameraScript != null) followCameraScript.enabled = true;

        if (ControllerScript != null) ControllerScript.enabled = true;


    }

    /// <summary>
    /// カメラを滑らかに移動（EaseInOut補間）
    /// </summary>
    private IEnumerator SmoothMoveCamera(Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = t * t * (3f - 2f * t); // EaseInOut補間
            mainCamera.transform.position = Vector3.Lerp(from, to, t);
            yield return null;
        }
        mainCamera.transform.position = to;
    }

}


