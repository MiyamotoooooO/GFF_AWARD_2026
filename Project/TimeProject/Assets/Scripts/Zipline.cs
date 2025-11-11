using System.Collections;
using UnityEngine;

public class Zipline : MonoBehaviour
{
    [Header("ツルの両端ポイント（2点を設定）")]
    public Transform startPoint; // 開始位置
    public Transform endPoint;   // 終了位置

    [Header("ツルに乗るためのキー")]
    public KeyCode grabKey = KeyCode.Space;

    [Header("ツル上での移動速度")]
    public float moveSpeed = 5f;

    private bool playerOnZipline = false;
    private bool isGrabbing = false;
    private GameObject player;
    private Rigidbody playerRb;
    private int direction = 1; // 1: start→end, -1: end→start

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerOnZipline = true;
            player = other.gameObject;
            playerRb = player.GetComponent<Rigidbody>();

            Debug.Log("プレイヤーがツル範囲に入りました。");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerOnZipline = false;
            ReleasePlayer();
            Debug.Log("プレイヤーがツル範囲から出ました。");
        }
    }

    void Update()
    {
        // 掴む
        if (playerOnZipline && Input.GetKeyDown(grabKey))
        {
            GrabPlayer();
        }

        // 離す
        if (isGrabbing && Input.GetKeyUp(grabKey))
        {
            ReleasePlayer();
        }
    }

    void FixedUpdate()
    {
        if (isGrabbing && player != null)
        {
            MovePlayer();
        }
    }

    void GrabPlayer()
    {
        isGrabbing = true;
        playerRb.useGravity = false;
        playerRb.velocity = Vector3.zero;

        // 開始点・終了点の近い方を判断
        float distStart = Vector3.Distance(player.transform.position, startPoint.position);
        float distEnd = Vector3.Distance(player.transform.position, endPoint.position);
        direction = (distStart < distEnd) ? 1 : -1;

        Debug.Log("ツルを掴みました！");
    }

    void ReleasePlayer()
    {
        if (playerRb != null)
            playerRb.useGravity = true;

        isGrabbing = false;
    }

    void MovePlayer()
    {
        Transform target = (direction == 1) ? endPoint : startPoint;
        Vector3 dir = (target.position - player.transform.position).normalized;

        playerRb.MovePosition(player.transform.position + dir * moveSpeed * Time.fixedDeltaTime);

        // 終点に到達したら自動で離す
        if (Vector3.Distance(player.transform.position, target.position) < 0.1f)
        {
            ReleasePlayer();
            Debug.Log("ツルの端に到達しました。");
        }
    }
}

