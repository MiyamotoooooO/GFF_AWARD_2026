using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;

public class O2Gauge : MonoBehaviour
{
    public static O2Gauge Instance { get; private set; }
    [Header("酸素ゲージ設定")]
    public float maxOxygen = 300f; // 最大酸素量
    public float oxygenDecreaseRate = 1f; // １秒間に減る酸素量

    [Header("UI要素")]
    public Sprite[] oxygenSprites;
    public GameObject gameOverPanel; // ゲームオーバーパネル
    public Image oxygenImage;

    [Header("ゲーム状態")]
    public bool isGameActive = true; // ゲーム進行中かどうか
    private bool isWarningActive = false;

    private float currentOxygen; // 現在の酸素量
    private int numberOfSprites; // スプライトの総数
    private float nextLogThreshold;
    private float nextSwitchThreshold;

    [Header("警告用エフェクト")]
    public float warningThreshold = 48f; // 警告開始の酸素量
    public float blinkSpeed = 1000f; //点滅の速さ 
    public float decreaseStepOff = 3f; // 赤から通常への切り替えステップ(3減る)
    public float decreaseStepOn = 2f; // 通常から赤への切り替えステップ(2減る)
    //public UnityEngine.Rendering.VolumeProfile warningVolumeProfile; // 警告用のポストプロファイル
    //private UnityEngine.Rendering.Volume volume; // シーンのGlobal Volumeコンポーネント
    public GameOverUIController gameOverUIController;
    public PostProcessVolume postProcessVolume;
    public PostProcessProfile warningProfile; // 警告時に使用する
    private int nothingMask;
    private int defaultMask;
    private int vignetteMask;
    private PostProcessProfile originalProfile;
    private PostProcessLayer postProcessLayer;

    [Header("ゲームオーバーViginette設定")]
    public PostProcessVolume gameOverVignetteVolume;
    public float vignetteFadeDuration = 2f; // ビネットが暗くなるまでの時間
    public float vignetteStartIntensity = 0f; // ズーム開始時のVignette強度
    public float targetVignetteIntensity = 0.8f; //ズーム完了時Vignette強度(0.5～0.8くらいで調整)
    public float vignetteSmoothness = 0.5f; // Vignetteのなめらかさ(デフォルト値)
    public float vignetteRounded = 1.0f; // Vignetteの丸み(デフォルト値)
    private Vignette vignette; // 制御するVignetteエフェクト
    private PostProcessProfile runtimeProfile; // ランタイム用のプロファイルインスタンス

    [Header("ゲームオーバー演出設定")]
    public float panelDropDuration = 0.5f; // パネルが落ちる時間
    public PlayerController playerController;
    public Rigidbody playerRigidbody;

    [Header("カメラ/エフェクト参照")]
    public Camera mainCamera;

    [Header("カメラズーム設定")]
    public Transform playerTransform;
    public float zoomDuration = 2.0f; // ズームにかける時間
    public float targetZoomDistance = 1.5f; // プレイヤーからカメラまでの目標距離
    public float zoomHeight = 0.5f; // プレイヤーの身体のどの高さにカメラを合わせるか

    private void Awake()
    {
        // シングルトンの初期化
        if (Instance == null)
        {
            Instance = this;
            numberOfSprites = oxygenSprites.Length;
        }
        else
        {
            Destroy(gameObject);
        }

        numberOfSprites = oxygenSprites.Length;

        // メインカメラとPost Process Layerの取得
        if (mainCamera == null)
        {
            mainCamera = Camera.main; // MainCameraのタグが付いているカメラを取得
        }

        if (mainCamera != null)
        {
            postProcessLayer = mainCamera.GetComponent<PostProcessLayer>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // 初期化
        currentOxygen = maxOxygen;
        //nextLogThreshold = maxOxygen - 50f;

        nextSwitchThreshold = warningThreshold;

        if (LayerMask.NameToLayer("Nothing") == -1)
        {
            Debug.LogError("UnityのLayers設定に'Nothing'レイヤーが存在しません!");
        }

        nothingMask = LayerMask.GetMask("Nothing");
        defaultMask = LayerMask.GetMask("Default");
        vignetteMask = LayerMask.GetMask("Vignette");

        if (nothingMask == 0 || defaultMask == 0)
        {
            Debug.Log("レイヤー名の取得に失敗しました。レイヤー名'Nothing'または'Default'がUnityのLayers設定と一致しているか確認してください。");
        }

        // ゲームオーバーパネルを非表示
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        UpdateOxygenUI();

        if (postProcessLayer != null)
        {
            int currentMask = postProcessLayer.volumeLayer;
            int vignetteMask = LayerMask.GetMask("Vignette");
            postProcessLayer.volumeLayer = currentMask | vignetteMask;
            //postProcessLayer.volumeLayer = LayerMask.GetMask("Nothing"); // "Nothing"レイヤーに設定
        }

        // postProcessVolumeからVignetteコンポーネメントを取得
        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            runtimeProfile = Instantiate(postProcessVolume.profile);
            postProcessVolume.profile = runtimeProfile;

            // プロファイルにVignetteエフェクトが含まれているか確認
            if (runtimeProfile.TryGetSettings(out vignette))
            {
                vignette.intensity.overrideState = true;

                // ズーム開始時は0に設定する
                vignette.intensity.value = 0f;

                // Vignetteのその他の設定もOverrideをONにする
                vignette.color.overrideState = true;
                vignette.color.value = Color.black; // 黒に設定
                Debug.Log("ランタイムプロファイル設定完了");
            }
            else
            {
                Debug.LogError("クローンプロファイルからvignetteが見つかりません。");
            }

            if (gameOverVignetteVolume != null)
            {
                gameOverVignetteVolume.weight = 0f; // 初期状態では無効
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // ゲームが進行中のみ酸素を減らす
        if (isGameActive && currentOxygen > 0)
        {
            // 時間経過で酸素を減らす
            currentOxygen -= oxygenDecreaseRate * Time.deltaTime;

            currentOxygen = Mathf.Max(0f, currentOxygen); // 0未満にならないように制限

            // 50経るごとにログ出力判定ロジック
            if (currentOxygen <= nextLogThreshold && nextLogThreshold >= 0f)
            {
                Debug.Log($"[酸素警告]酸素残量が{nextLogThreshold:F0}を下回りました。現在の残量:{currentOxygen:F2}");
                nextLogThreshold -= 50f;
            }

            // 酸素が０になった場合
            if (currentOxygen <= 0)
            {
                GameOverUI();
                return;
            }

            if (postProcessLayer == null) return;

            if (!isWarningActive && currentOxygen <= warningThreshold)
            {
                isWarningActive = true;

                // 警告開始時: 最初は赤画面(Default)からスタート
                SwitchLayer(defaultMask);
                Debug.Log("【警告開始】赤画面に切り替え。目標: " + (currentOxygen - decreaseStepOff));

                // 次の目標は3減ったら通常に戻す
                nextSwitchThreshold = currentOxygen - decreaseStepOff;
            }

            if (isWarningActive)
            {
                // 次の切り替え目標値を下回ったか判定
                if (currentOxygen <= nextSwitchThreshold)
                {
                    // 現在がDefault(赤)の場合 -> Nothing(通常)へ切り替え
                    if (postProcessLayer.volumeLayer == defaultMask)
                    {
                        SwitchLayer(nothingMask); // 通常(Nothing)へ
                        nextSwitchThreshold = currentOxygen - decreaseStepOn; // 次の目標は2減ったら赤
                        Debug.Log($"警告OFF/通常へ。02: {currentOxygen:F2}, 次目標: {nextSwitchThreshold:F2}");
                    }
                    // 現在がNothing(通常)の場合 -> Default(赤)へ
                    else if (postProcessLayer.volumeLayer == nothingMask)
                    {
                        SwitchLayer(defaultMask); // 赤(Default)へ
                        nextSwitchThreshold = currentOxygen - decreaseStepOff; // 次の目標は3減ったら通常
                        Debug.Log($"警告ON/赤へ。02: {currentOxygen:F2}, 次目標: {nextSwitchThreshold:F2}");
                    }
                }
            }

            UpdateOxygenUI();
        }
    }

    private void SwitchLayer(int targetMask)
    {
        if (postProcessLayer != null)
        {
            postProcessLayer.volumeLayer = targetMask;
        }
    }

    // MainCameraのPost Process LayerをDefaultに設定(赤色)
    private void SwitchLayerToDefault()
    {
        if (postProcessLayer != null)
        {
            postProcessLayer.volumeLayer = LayerMask.GetMask("Default");
        }
    }

    // MainCameraのPost Process LayerをNothingに設定(通常)   
    private void SwitchLayerToNothing()
    {
        if (postProcessLayer != null)
        {
            postProcessLayer.volumeLayer = LayerMask.GetMask("Nothing");
        }
    }

    private void SwitchLayerToVignette()
    {
        if (postProcessLayer != null)
        {
            postProcessLayer.volumeLayer = LayerMask.GetMask("Vignette");
        }
    }

    // 酸素UIを更新
    void UpdateOxygenUI()
    {

        // --- 画像を切り替えたい処理 ---
        if (oxygenImage != null && oxygenSprites.Length > 0)
        {
            // 1.現在の酸素量(300～0)を画像のインデックスに変換
            float normalizedOxygen = currentOxygen / maxOxygen; ; // 50.0f

            int maxIndex = numberOfSprites - 1;

            // 小数点を切り捨ててインデックス(0～5)を取得
            int spriteIndex = Mathf.FloorToInt((1f - normalizedOxygen) * maxIndex);

            // スプライトを切り替える
            oxygenImage.sprite = oxygenSprites[spriteIndex];
        }
    }

    // ゲームオーバーの処理
    public void GameOverUI()
    {

        Debug.Log("ゲームオーバー！酸素が尽きました！");

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (playerRigidbody != null)
        {
            playerRigidbody.velocity = Vector3.zero; // 速度をゼロにする
            playerRigidbody.angularVelocity = Vector3.zero; // 回転速度をゼロにする
            playerRigidbody.isKinematic = true;
        }

        // 画面を通常状態に戻す(Nothingレイヤーに)
        if (postProcessLayer != null)
        {
            SwitchLayerToVignette();
        }

        //isGameActiveをfalseにする
        isGameActive = false;

        // メインカメラの取得
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // ズーム処理を開始
        if (mainCamera != null)
        {

            Vector3 startPos = mainCamera.transform.position;
            Quaternion startRot = mainCamera.transform.rotation;

            // ズームコルーチンを開始
            StartCoroutine(ZoomToPlayer(startPos, startRot, zoomDuration));
        }

        //ShowGameOverUI_ExistingLogic();
    }

    // 点滅を停止し、画像を非表示にする

    private void ShowGameOverUI_ExistingLogic()
    {

        //ゲームオーバーパネルを表示
        if (gameOverUIController != null)
        {
            gameOverUIController.StartDropAnimation();
        }
    }

    // カメラをプレイヤーにズームするコルーチン
    IEnumerator ZoomToPlayer(Vector3 startPosition, Quaternion startRotation, float duration)
    {
        float zoomDuration = duration;

        // ズーム中のカメラはプレイヤーの子オブジェクトではないと仮定
        if (playerTransform == null)
        {
            Debug.LogError("Player Transformが設定されていません。ズームできません。");

            gameOverUIController.StartDropAnimation();
            yield break;
        }

        float timer = 0f;
        mainCamera.transform.position = startPosition;
        mainCamera.transform.rotation = startRotation;

        // 目標地点の計算
        Vector3 targetLookAt = playerTransform.position + Vector3.up * zoomHeight;
        Vector3 targetPosition = targetLookAt - playerTransform.forward * targetZoomDistance;

        // カメラを目標地点に向ける回転
        Quaternion targetRotation = Quaternion.LookRotation(targetLookAt - targetPosition);

        // ズーム実行
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            mainCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);


            //postProcessVolume.weight = Mathf.Lerp(0f, 1f, t);
            if (vignette != null)
            {
                gameOverVignetteVolume.weight = Mathf.Lerp(0f, 1f, t);
                vignette.intensity.value = Mathf.Lerp(0f, targetVignetteIntensity, t);

                Debug.Log($"【Vignette FADE】進行度 t:{t:F2}, New Intensity:{targetVignetteIntensity:F2}");
            }

            yield return null;
        }

        // 最後に正確な位置に設定
        mainCamera.transform.position = targetPosition;
        mainCamera.transform.rotation = targetRotation;
        postProcessVolume.weight = 1f;

        if (vignette != null)
        {
            //gameOverVignetteVolume.weight = 1f; // 完全にVignetteを適用
            vignette.intensity.value = targetVignetteIntensity; // 0.8に固定
        }

        // ズーム完了後、ゲームオーバーUIの落下アニメーションを開始
        if (gameOverUIController != null)
        {
            gameOverUIController.StartDropAnimation();
        }
    }

    public void TakeDamage(int damageAmount)
    {
        currentOxygen -= damageAmount;
        currentOxygen = Mathf.Max(0, currentOxygen); // 0未満にならないように制限
        Debug.Log($"ダメージを受けました！減少量:{damageAmount}、残り酸素:{currentOxygen}");

        // UIを更新
        UpdateOxygenUI();
    }

    public void RecoverOxygen(float amount)
    {
        currentOxygen = Mathf.Min(maxOxygen, currentOxygen + amount);
        UpdateOxygenUI();
    }

    // リスタート機能
    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}



