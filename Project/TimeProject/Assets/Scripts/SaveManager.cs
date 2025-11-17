using System.IO;
using UnityEngine;
using System.Linq;
using UnityEngine.Rendering; // LoadGame内で初期化する場合に必要(テスト用)

public class SaveManager : MonoBehaviour
{
    private string savePath;
    public static SaveManager Instance; // シングルトンで簡単アクセス
    public SaveData currentData;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        savePath = Path.Combine(Application.persistentDataPath, "save.json");
        LoadGame();
    }

    public void NotifyDataChanged()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
    #endif
    }


    public void SaveGame()
    {
        string json = JsonUtility.ToJson(currentData, true);
        File.WriteAllText(savePath, json);
        Debug.Log("セーブ完了: " + savePath);
    }

    public void LoadGame()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            currentData = JsonUtility.FromJson<SaveData>(json);
        }
        else
        {
            // 新規データを作成
            //currentData = new SaveData();
            //currentData.respawnPosition = new float[3] { 0.53f, 1.6f, -1.67f }; // 初期スポーン位置
            //currentData.checkpointReached = new bool[10]; // 例: チェックポイント10個分

            CreatNewSaveData();
        }

        DontDestroyOnLoad(gameObject);
    }

    private void CreatNewSaveData() // テスト用
    {
        currentData = new SaveData();
        currentData.respawnPosition = new float[3] { 0f, 0f, 0f };
        currentData.checkpointReached = Enumerable.Repeat(false, 10).ToArray();
        currentData.respawnPosition = new float[3] { 0.8f, 0.5f, -1.67f };
        currentData.checkpointReached = new bool[10];

        Debug.Log("新規データを作成しました。");
    }

    public SaveData CurrentData
    {
        get { return currentData; }
    }


    // ゲーム中にキー入力を監視してデータをリセットする
    void Update()
    {
        // 左コントロールキー
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            ResetSaveData();
        }
    }

    // セーブデータを初期化する処理
    public void ResetSaveData()
    {
        if (File.Exists(savePath))
        {
            // セーブファイルを削除
            File.Delete(savePath);

            Debug.Log($"セーブファイル {savePath} を削除しました。");
        }

        CreatNewSaveData();
    }
}

