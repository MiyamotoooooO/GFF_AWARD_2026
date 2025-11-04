using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;              // プレイヤーのTransform
    public Vector3 offset = new Vector3(0, 5, -8); // プレイヤーからの距離
    public float smoothSpeed = 0.125f;    // カメラ移動のなめらかさ
    //public Vector3 fixedRotation = new Vector3(23f, 11f, 0f); // 固定したい角度


    void LateUpdate()
    {
        // 目標位置
        Vector3 desiredPosition = player.position + offset;

        // 現在位置から目標位置へスムーズに移動
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // 実際に位置を更新
        transform.position = smoothedPosition;

        // rotation固定
        //transform.rotation = Quaternion.Euler(fixedRotation);
    }
    // ワープ時などに即座に位置を合わせる
    public void SnapToTarget()
    {
        transform.position = player.position + offset;
        //transform.rotation = Quaternion.Euler(fixedRotation);
    }
}
