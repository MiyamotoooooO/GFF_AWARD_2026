using System.Collections;
using UnityEngine;

public class hasigo : MonoBehaviour
{
    [Header("はしごの両端ポイント（2点を設定）")]
    public Transform startPoint; // 開始位置（登る側）
    public Transform endPoint;   // 終了位置（登り切った側）

    [Header("はしごに乗るためのキー")]
    public KeyCode grabKey = KeyCode.Space;

    [Header("はしご上での移動速度")]
    public float moveSpeed = 5f;

    [Header("はしごを登りきった後に前に進む距離")]
    public float forwardDistance = 1f;

    [Header("登りきった後に前進する速度")]
    public float forwardSpeed = 3f;

    private bool playerOnLadder = false;
    private bool isGrabbing = false;
    private bool isAutoMoving = false;
    private bool isMovingForward = false;
    private GameObject player;
    private Rigidbody playerRb;
    private int direction = 1; // 1: start→end (登る方向), -1: end→start (降りる方向)

    private Vector3 forwardTarget; // 前進の目標位置

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerOnLadder = true;
            player = other.gameObject;
            playerRb = player.GetComponent<Rigidbody>();

            Debug.Log("プレイヤーがはしご範囲に入りました。");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerOnLadder = false;
            ReleasePlayer();
            Debug.Log("プレイヤーがはしご範囲から出ました。");
        }
    }

    void Update()
    {
        if (playerOnLadder && !isAutoMoving && Input.GetKeyDown(grabKey))
        {
            GrabPlayer();
        }
    }

    void FixedUpdate()
    {
        if (isAutoMoving && player != null)
        {
            MovePlayerToTop();
        }
        else if (isMovingForward && player != null)
        {
            MovePlayerForward();
        }
    }

    void GrabPlayer()
    {
        // プレイヤーからstart/endの距離を比較
        float distStart = Vector3.Distance(player.transform.position, startPoint.position);
        float distEnd = Vector3.Distance(player.transform.position, endPoint.position);
        direction = (distStart < distEnd) ? 1 : -1;

        if (direction == 1)
        {
            // 登る方向の時だけ掴む
            isGrabbing = true;
            isAutoMoving = true;

            playerRb.useGravity = false;
            playerRb.velocity = Vector3.zero;

            Debug.Log("はしごを掴みました！（登る方向）");
        }
        else
        {
            // 降りる方向の場合は使わない
            Debug.Log("降りる方向なので、はしごは使えません。");
        }
    }

    void ReleasePlayer()
    {
        if (playerRb != null)
            playerRb.useGravity = true;

        isGrabbing = false;
        isAutoMoving = false;
        isMovingForward = false;
    }

    void MovePlayerToTop()
    {
        Transform target = (direction == 1) ? endPoint : startPoint;
        Vector3 dir = (target.position - player.transform.position).normalized;

        playerRb.MovePosition(player.transform.position + dir * moveSpeed * Time.fixedDeltaTime);

        // 終点に到達したら自動で前進動作へ
        if (Vector3.Distance(player.transform.position, target.position) < 0.1f)
        {
            isAutoMoving = false;
            isMovingForward = true;

            // はしごの「前方向」を算出（endPoint の forward 方向）
            Vector3 forwardDir = endPoint.forward;
            forwardTarget = player.transform.position + forwardDir * forwardDistance;

            Debug.Log("はしごの端に到達 → 前に進みます");
        }
    }

    void MovePlayerForward()
    {
        Vector3 dir = (forwardTarget - player.transform.position).normalized;
        playerRb.MovePosition(player.transform.position + dir * forwardSpeed * Time.fixedDeltaTime);

        if (Vector3.Distance(player.transform.position, forwardTarget) < 0.05f)
        {
            isMovingForward = false;
            ReleasePlayer();
            Debug.Log("前進完了（はしご動作終了）");
        }
    }
}

