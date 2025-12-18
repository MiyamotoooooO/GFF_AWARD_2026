using System.Collections;
using UnityEngine;

public class GoalCamera : MonoBehaviour
{
    [Header("参照オブジェクト")]
    public Camera mainCamera;          // メインカメラ
    public Transform player;           // プレイヤー
    public Transform nextStagePoint;   // 次ステージ方向の目印

    [Header("カメラ移動設定")]
    public float moveDistance = 0f;
    public float moveDuration = 0f;
    public float showTime = 0f;
    public float backXOffset = 0f;
    public float backZOffset = 0f;

    private bool hasCleared = false;
    private bool isPlaying = false;            // 今演出中かどうか
    private Coroutine playingRoutine = null;

    private Vector3 cameraOffset;

    private MonoBehaviour followCameraScript;
    private MonoBehaviour ControllerScript;

    // ★追加：Player の Animator / AudioSource
    private Animator playerAnimator;
    private AudioSource playerAudio;

    void Start()
    {
        if (mainCamera != null && player != null)
        {
            cameraOffset = mainCamera.transform.position - player.position;
        }

        followCameraScript = mainCamera.GetComponent<MonoBehaviour>();
        ControllerScript = player.GetComponent<MonoBehaviour>();

        // ★追加：Player の Animator と AudioSource を取得
        playerAnimator = player.GetComponent<Animator>();
        playerAudio = player.GetComponent<AudioSource>();
    }

    void Update()
    {
        // 🔸 Spaceキーでスキップ
        if (isPlaying && Input.GetKeyDown(KeyCode.Space))
        {
            SkipCameraRoutine();
        }
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.gameObject.CompareTag("Player"))
    //    {
    //        OnStageClear();
    //    }
    //}

    public void OnStageClear()
    {
        if (hasCleared) return;  // 一度だけ
        hasCleared = true;

        isPlaying = true;
        playingRoutine = StartCoroutine(CameraMoveRoutine());
    }

    private IEnumerator CameraMoveRoutine()
    {
        // ▼ Player操作 & カメラスクリプト停止
        if (followCameraScript != null) followCameraScript.enabled = false;
        if (ControllerScript != null) ControllerScript.enabled = false;

        // ★追加：Animator 停止
        if (playerAnimator != null) playerAnimator.enabled = false;

        // ★追加：Audio 停止
        if (playerAudio != null) playerAudio.Pause();

        Vector3 startPos = mainCamera.transform.position;
        Vector3 dir = (nextStagePoint.position - startPos).normalized;
        Vector3 targetPos = startPos + dir * moveDistance + new Vector3(backXOffset, 0, backZOffset);

        // ① スライド
        yield return StartCoroutine(SmoothMoveCamera(startPos, targetPos, moveDuration));

        // ② 停止中もスキップ可能
        float timer = 0;
        while (timer < showTime)
        {
            if (!isPlaying) yield break;
            timer += Time.deltaTime;
            yield return null;
        }

        // ③ 戻る
        yield return StartCoroutine(SmoothMoveCamera(targetPos, startPos, moveDuration));

        EndCameraRoutine();
    }

    /// <summary>
    /// スキップ処理
    /// </summary>
    private void SkipCameraRoutine()
    {
        if (!isPlaying) return;

        isPlaying = false;

        if (playingRoutine != null)
        {
            StopCoroutine(playingRoutine);
        }

        // カメラの位置リセット
        mainCamera.transform.position = player.position + cameraOffset;

        EndCameraRoutine();
    }

    /// <summary>
    /// 終了処理まとめ
    /// </summary>
    private void EndCameraRoutine()
    {
        // カメラ & Player操作復帰
        if (followCameraScript != null) followCameraScript.enabled = true;
        if (ControllerScript != null) ControllerScript.enabled = true;

        // ★追加：Animator を元に戻す
        if (playerAnimator != null) playerAnimator.enabled = true;

        // ★追加：Audio を元に戻す
        if (playerAudio != null) playerAudio.UnPause();

        isPlaying = false;
    }

    private IEnumerator SmoothMoveCamera(Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (!isPlaying) yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = t * t * (3f - 2f * t);
            mainCamera.transform.position = Vector3.Lerp(from, to, t);

            yield return null;
        }
        mainCamera.transform.position = to;
    }
}

