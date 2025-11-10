using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;
using TMPro;

public class OxygenGaugeController : MonoBehaviour
{
    public static OxygenGaugeController Instance { get; private set; }
    [Header("最大酸素量")]
    public float maxOxygen = 300f;
    [Header("1秒間に減る酸素量")]
    public float oxygenDecreaseRate = 1f;
    [Header("酸素ゲージUI")]
    public Sprite[] oxygenSprites;
    [Header("ゲームオーバー時のUI")]
    public GameObject gameOverPanel;
    [Header("UIを表示するImageコンポーネント")]
    public Image oxygenImage;
    [Header("ゲーム進行中かどうか")]
    public bool isGameActive = true;
    [Header("警告開始の酸素量")]
    public float warningThreshold = 48f;
    [Header("赤から通常への切り替えステップ")]
    public float decreaseStepOff = 3f;
    [Header("通常からから赤への切り替えステップ")]
    public float decreaseStepOn = 2f;
    [Header("ゲームオーバーUIを表示させるスプライト")]
    public GameOverUIController gameOverUIController;
    [Header("赤色のPostProcessVolume")]
    public PostProcessVolume postProcessVolume;
    [Header("黒色のPostProcessVolume")]
    public PostProcessVolume gameOverVignetteVolume;
    [Header("Vignetteが暗くなるまでの時間")]
    public float fadeDuration = 1.0f;
    [Header("ズーム開始時のVignetteの強度")]
    public float vignetteStartIntensity = 0f;
    [Header("ズーム完了時のVignetteの強度")]
    public float targetVignetteIntensity = 0.8f;
    [Header("Vignetteのなめらかさ")]
    public float vignetteSmoothness = 0.5f;
    [Header("ゲームオーバーのUIが落ちる時間")]
    public float panelDropDuration = 0.5f;
    [Header("プレイヤースプリクトを参照")]
    public PlayerController playerController;
    [Header("プレイヤーのRigidbodyを参照")]
    public Rigidbody playerRigidbody;
    [Header("プレイヤーのAnimatorを参照")]
    public Animator playerAnimator;
    [Header("Restartする際のTextMeshPro")]
    public TextMeshProUGUI restartTextMeshPro;
    [Header("メインカメラを参照")]
    public Camera mainCamera;
    [Header("Canvas内のMainUiを参照")]
    public CanvasGroup mainCanvasGroup;
    [Header("MainUiを傾ける時間")]
    public float rotationDuration = 1.0f;
    [Header("MainUiの回転角度")]
    public float rotationAngle = 6.0f;
    [Header("傾け完了から落下開始までの遅延時間")]
    public float fallDelay = 0.5f;
    [Header("画面外に落下するまでの時間")]
    public float fallDuration = 0.8f;
    [Header("落下させるY軸の距離(画面外に出るのに十分な距離)")]
    public float fallDistanceY = 1500f;
    [Header("プレイヤーのTransformを参照")]
    public Transform playerTransform;
    [Header("BGMを参照")]
    public AudioSource bgmAudioSource;

    // private参照
    private bool isWarningActive = false;
    private bool isGameOver = false;
    private bool canRestart = false; // リスタート可能になったかどうかのフラグ
    private static Vector3 lastCheckpointPosition;
    private static bool hasCheckpoint = false;
    private float currentOxygen; // 現在の酸素量
    private int numberOfSprites; // スプライトの総数
    private float nextLogThreshold;
    private float nextSwitchThreshold;
    private int nothingMask;
    private int defaultMask;
    private int vignetteMask;
    private PostProcessProfile originalProfile;
    private PostProcessLayer postProcessLayer;
    private Vignette vignette; // 制御するVignetteエフェクト
    private PostProcessProfile runtimeProfile; // ランタイム用のプロファイルインスタンス
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

        if (restartTextMeshPro != null)
        {
            restartTextMeshPro.enabled = false;
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
        vignetteMask = LayerMask.GetMask("Vignette");

        if (nothingMask == 0 || defaultMask == 0)
        {
            Debug.Log("レイヤー名の取得に失敗しました。レイヤー名'Nothing'または'Default'がUnityのLayers設定と一致しているか確認してください。");
        }

        if (PlayerPrefs.GetInt("HasRespawn", 0) == 1 && playerTransform != null)
        {
            float x = PlayerPrefs.GetFloat("RespawnX");
            float y = PlayerPrefs.GetFloat("RespawnY");
            float z = PlayerPrefs.GetFloat("RespawnZ");
            Vector3 respawnPosition = new Vector3(x, y, z);

            // プレイヤーをセーブ位置に移動
            playerTransform.position = respawnPosition;

            // 李スプーンフラグをリセットし、次回以降のロードでは適用されないようにする
            PlayerPrefs.SetInt("HasRespawn", 0);
            PlayerPrefs.Save();

            Debug.Log($"リスタートしました。移動先: {respawnPosition}");
        }

        // ゲームオーバーパネルを非表示
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        //if (restartTextObject != null)
        //{
        //restartTextObject.SetActive(false);
        //}
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

            UpdateOxygenUI();

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

            //UpdateOxygenUI();
        }
        // ゲームオーバーかつリスタート可能なら、スペースキー入力をチェック
        if (isGameOver && canRestart && Input.GetKeyDown(KeyCode.Space))
        {
            RestartGame();
        }
    }

    private void SwitchLayer(int targetMask)
    {
        if (postProcessLayer != null)
        {
            postProcessLayer.volumeLayer = targetMask;
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
        if (oxygenImage == null || oxygenSprites == null || oxygenSprites.Length == 0)
        {
            return; // 設定がなければスキップ
        }

        // 酸素が0の場合は非表示にする(12段階目)
        if (currentOxygen <= 0f)
        {
            oxygenImage.enabled = false;
            return;
        }

        // 12段階のサイズ(300 / 12 = 25)
        float rangeSize = maxOxygen / 12f; // 25.0
        int gaugeIndex = 0; float decreasedOxygen = maxOxygen - currentOxygen;

        gaugeIndex = Mathf.FloorToInt(decreasedOxygen / rangeSize);
        gaugeIndex = Mathf.Clamp(gaugeIndex, 0, oxygenSprites.Length - 1);
        oxygenImage.sprite = oxygenSprites[gaugeIndex];
    }

    // ゲームオーバーの処理
    public void GameOverUI()
    {
        if (isGameOver) return;
        isGameOver = true;

        if (mainCanvasGroup != null)
        {
            mainCanvasGroup.alpha = 0f; // 透明度を0にして非表示にする
        }

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

        if (playerAnimator != null)
        {
            if (playerAnimator.gameObject.name == "Player")
            {
                playerAnimator.Play("Idle", 0, 0f);
                playerAnimator.enabled = false;
            }
            else
            {
                Debug.LogWarning($"Animatorは設定されていますが、名前が 'Player_0' ではありません({playerAnimator.gameObject.name})。スキップします。");
            }
        }

        if(bgmAudioSource != null && bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Stop();
        }

        // 画面を通常状態に戻す(Nothingレイヤーに)
        if (postProcessLayer != null)
        {
            SwitchLayerToVignette();
        }

        //isGameActiveをfalseにする
        isGameActive = false;

        // コルーチン開始
        StartCoroutine(AnimateAndHideMainUI());
        //ShowGameOverUI_ExistingLogic();
    }

    IEnumerator AnimateAndHideMainUI()
    {
        if (mainCanvasGroup == null)
        {
            //StartCoroutine(FadeToBlackAndShowUI());
            yield break;
        }

        Transform mainUiTransform = mainCanvasGroup.transform;
        float timer = 0f;

        StartCoroutine(FadeToBlackAndShowUI());

        // z軸回転アニメーション
        Quaternion startRotation = mainUiTransform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0, 0, rotationAngle);

        while (timer < rotationDuration)
        {
            timer += Time.deltaTime;
            float t = timer / rotationDuration;

            // z軸を0度から6度へ補間
            mainUiTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            // Canvas Groupのalphaを0に補間して、アニメーションと同時に非表示にする(見た目だけを消す)
            mainCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }
        // 完全に回転と透明化を完了させる
        mainUiTransform.rotation = targetRotation;
        mainCanvasGroup.alpha = 0f;

        // 落下開始までの遅延
        yield return new WaitForSeconds(fallDelay);

        // 落下アニメーション
        timer = 0f;
        Vector3 startPosition = mainUiTransform.position;
        Vector3 targetPosition = startPosition - Vector3.up * fallDistanceY; // 下方向へ落下

        while (timer < fallDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fallDuration;

            // Y軸を落下させる
            mainUiTransform.position = Vector3.Lerp(startPosition, targetPosition, t);

            yield return null;
        }
        // 完全に画面外へ移動させる
        mainUiTransform.position = targetPosition;

        // アニメーション完了
        //StartCoroutine(FadeToBlackAndShowUI());
    }

    IEnumerator FadeToBlackAndShowUI()
    {
        Debug.Log("FadeToBlackが呼ばれました。暗転を開始します，");
        if (gameOverVignetteVolume == null)
        {
            Debug.LogError("GameOverVignetteVolumeが設定されていません。");
        }
        else
        {
            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                float t = timer / fadeDuration;

                gameOverVignetteVolume.weight = Mathf.Lerp(0f, 1f, t);

                yield return null;
            }
            // 完全に暗くする
            gameOverVignetteVolume.weight = 1f;
        }
        // 暗転完了後
        if (gameOverUIController != null)
        {
            gameOverUIController.StartDropAnimation();
        }

        yield return new WaitForSeconds(1.5f);

        if (restartTextMeshPro != null)
        {
            restartTextMeshPro.enabled = true;
        }
        // UI表示の開始後、リスタートフラグを立てる
        canRestart = true;
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

    public static void SetCheckpoint(Vector3 position)
    {
        lastCheckpointPosition = position;
        hasCheckpoint = true;
        Debug.Log("チェックポイントを記録しました。");
    }

    // リスタート機能
    void RestartGame()
    {
        // プレイヤーを最新のチェックポイントへ移動させる
        //if (playerController != null)
        //{
            //Vector3 checkpointPos = playerController.GetLastCheckpointPosition();

            //playerController.transform.position = checkpointPos;
        //}

        if (restartTextMeshPro != null)
        {
            restartTextMeshPro.enabled = false;
        }

        if(bgmAudioSource != null && !bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Play();
        }

        if (mainCanvasGroup != null)
        {
            mainCanvasGroup.alpha = 1f; // 透明度を1に戻して表示

            mainCanvasGroup.transform.localPosition = Vector3.zero; // LocalPositionを原点に戻す
            mainCanvasGroup.transform.rotation = Quaternion.identity; // 回転リセット
            mainCanvasGroup.blocksRaycasts = true; // 必要であれば入力を再度受け付ける
        }

        // 現在のロードシーンを再ロード
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}



