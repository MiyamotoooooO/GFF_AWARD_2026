using System.Collections;
using UnityEngine;

public class GoalCamera : MonoBehaviour
{
    [Header("参照オブジェクト")]
    public Camera mainCamera;
    public Transform player;
    public Transform nextStagePoint;

    [Header("カメラ移動設定")]
    public float moveDistance = 0f;
    public float moveDuration = 0f;
    public float showTime = 0f;
    public float backXOffset = 0f;
    public float backZOffset = 0f;

    private bool hasCleared = false;
    private Vector3 cameraOffset;

    private MonoBehaviour followCameraScript;
    private MonoBehaviour ControllerScript;

    // ★追加：プレイヤーの Animator
    private Animator playerAnimator;

    // ★追加：プレイヤーの AudioSource
    private AudioSource playerAudio;

    void Start()
    {
        if (mainCamera != null && player != null)
        {
            cameraOffset = mainCamera.transform.position - player.position;
        }

        followCameraScript = mainCamera.GetComponent<MonoBehaviour>();
        ControllerScript = player.GetComponent<MonoBehaviour>();

        // ★追加：プレイヤーの Animator と AudioSource を取得
        playerAnimator = player.GetComponent<Animator>();
        playerAudio = player.GetComponent<AudioSource>();
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
        if (followCameraScript != null) followCameraScript.enabled = false;
        if (ControllerScript != null) ControllerScript.enabled = false;

        // ★追加：プレイヤーの Animator を停止
        if (playerAnimator != null) playerAnimator.enabled = false;

        // ★追加：プレイヤーの AudioSource を停止
        if (playerAudio != null) playerAudio.Pause();

        Vector3 startPos = mainCamera.transform.position;
        Vector3 dir = (nextStagePoint.position - startPos).normalized;

        Vector3 targetPos = startPos + dir * moveDistance + new Vector3(backXOffset, 0, backZOffset);

        yield return StartCoroutine(SmoothMoveCamera(startPos, targetPos, moveDuration));
        yield return new WaitForSeconds(showTime);
        yield return StartCoroutine(SmoothMoveCamera(targetPos, startPos, moveDuration));

        if (followCameraScript != null) followCameraScript.enabled = true;
        if (ControllerScript != null) ControllerScript.enabled = true;

        // ★追加：Animator を元に戻す
        if (playerAnimator != null) playerAnimator.enabled = true;

        // ★追加：AudioSource を再開
        if (playerAudio != null) playerAudio.UnPause();
    }

    private IEnumerator SmoothMoveCamera(Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = t * t * (3f - 2f * t);
            mainCamera.transform.position = Vector3.Lerp(from, to, t);
            yield return null;
        }
        mainCamera.transform.position = to;
    }
}

