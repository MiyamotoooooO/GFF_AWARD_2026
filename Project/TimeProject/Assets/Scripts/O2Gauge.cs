using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OxygenGaugeController : MonoBehaviour
{
    public static OxygenGaugeController Instance { get; private set; }
    [Header("酸素ゲージ設定")]
    public float maxOxygen = 300f; // 最大酸素量
    public float oxygenDecreaseRate = 1f; // １秒間に減る酸素量

    [Header("UI要素")]
    public Sprite[] oxygenSprites;
    public Slider oxygenSlider; // 酸素ゲージのスライダー
    public Text oxygenText; // 酸素量のテキスト表示
    public GameObject gameOverPanel; // ゲームオーバーパネル
    public Image oxygenImage;

    [Header("ゲーム状態")]
    public bool isGameActive = true; // ゲーム進行中かどうか

    private float currentOxygen; // 現在の酸素量
    private int numberOfSprites; // スプライトの総数
    private float nextLogThreshold;

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
    }

    // Start is called before the first frame update
    void Start()
    {
        oxygenImage = GetComponent<Image>();
        // 初期化
        currentOxygen = maxOxygen;
        nextLogThreshold = maxOxygen - 50f;

        // ゲームオーバーパネルを非表示
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        UpdateOxygenUI();
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
                currentOxygen = 0;
                GameOver();
            }

            // UIを更新
            UpdateOxygenUI();
        }
    }

    // 酸素UIを更新
    void UpdateOxygenUI()
    {
        if (oxygenSlider != null)
        {
            // currentOxygenが300～0の範囲になるため、これでパーセンテージが表示される
            oxygenSlider.value = currentOxygen / maxOxygen;
        }

        if (oxygenText != null)
        {
            // 秒数を表示したい場合
            oxygenText.text = Mathf.Ceil(currentOxygen).ToString();
        }

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
    private void GameOver()
    {
        isGameActive = false;
        Debug.Log("ゲームオーバー！酸素が尽きました！");

        // ゲームオーバーパネルを表示
        if (oxygenImage != null)
        {
            if (numberOfSprites > 0)
            {
                oxygenImage.sprite = oxygenSprites[numberOfSprites - 1];
            }
            oxygenImage.enabled = false;
        }
        Debug.Log("【ゲームオーバー】酸素が尽きました！");

        // ゲームオーバーパネルを表示
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f;
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

