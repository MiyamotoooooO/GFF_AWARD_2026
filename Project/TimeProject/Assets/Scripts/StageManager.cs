using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    [Header("参照オブジェクト")]
    public Camera mainCamera;        //メインカメラ
    public Transform player;         //プレイヤー（戻り先）
    public Transform nextStagePoint; //次のステージを見せる位置

    [Header("カメラ移動設定")]
    public float moveDuration = 2f;  // カメラが移動にかける秒数
    public float showTime = 2f;      // 次のステージを見せる時間

    private bool hasCleared = false;  //追加　クリア済みフラグ

    //カメラとプレイヤーの相対位置を保持
    private Vector3 cameraOffset;

    //private void Start()
    //{
    //    //ゲーム開始時に「カメラがプレイヤーとどのくらい離れているか」を記録
    //    cameraOffset = mainCamera.transform.position - player.position;
    //}
    // ステージクリア時に呼び出す
    public void OnStageClear()
    {
        if (hasCleared) return;　　　//すでに実行していたら何もしない
        hasCleared = true;　　　　　// 実行済みにする
        StartCoroutine(CameraMoveRoutinee());
    }

    private IEnumerator CameraMoveRoutinee()
    {
       　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　 //コルーチン(Coroutine)は時間を分けて処理する
      　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　  // yield return はカメラの移動が終わるまでまつ
        // ①カメラをスライドして次のステージへ移動
        yield return StartCoroutine(MoveCamera(nextStagePoint.position));

        // ②次のステージを一定時間表示
        yield return new WaitForSeconds(showTime);

        // ③カメラをスライドしてプレイヤー位置へ戻す 
    　　//プレイヤーを中心に戻す
        Vector3 playerCenterPos = player.position + cameraOffset;
        yield return StartCoroutine(MoveCamera(playerCenterPos));
    }

    private IEnumerator MoveCamera(Vector3 targetPos)
    {
        Vector3 start = mainCamera.transform.position;

        Vector3 end = new Vector3(targetPos.x, start.y, targetPos.z);
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            // 進行度 (0 → 1)
            float t = elapsed / moveDuration;

            // 一定速度で位置を補間
            mainCamera.transform.position = Vector3.Lerp(start, end, t);

            // 経過時間を加算
            elapsed += Time.deltaTime;

            // 次のフレームまで待つ
            yield return null;
        }

        // 最終位置に固定
        mainCamera.transform.position = end;
    }


// Start is called before the first frame update


    // Update is called once per frame
    void Update()
    {
        
    }
}
