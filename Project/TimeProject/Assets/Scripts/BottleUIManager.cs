using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using System.IO;

public class BottleUIManager : MonoBehaviour
{
    public Image[] bottleUIImages;

    // 黄色のボトルのスプライト
    public Sprite fullBottleSprite;
    // 空のボトルのスプライト
    public Sprite emptyBottleSprite;

    public static BottleUIManager Instance { get; private set; }

    private BottleUIManager bottleUIManager;
    private SaveData currentSaveData; // セーブデータの参照
    public ObjectController1 takoController;

    private int BottleCount => currentSaveData.bottleStates;//.Count(state => state); // 現在の黄色ボトルの本数

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
    }

    void Start()
    {
        UpdateBottleUI();
        //CheckTakoInteraction();
    }

    private bool GetSaveData()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogError($"{this.name} : SaveManagerが存在しません。");
            return false;
        }

        currentSaveData = SaveManager.Instance.currentData;
        return true;
    }

    // UIの表示を現在のSaveDataに基づいてUIを更新する
    public void UpdateBottleUI()
    {
        if (!GetSaveData())
        {
            return;
        }

        SaveData bottleSaveData = currentSaveData;

        // UIが設定されていない場合はエラーを出す
        if (bottleUIImages.Length != currentSaveData.maxBottleCount)//.Length)
        {
            Debug.LogError("UI画像の数とセーブデータの配列サイズが一致しません。");
            return;
        }
        
        // 一度全てのボトル表示を空に
        foreach (var image in bottleUIImages)
        {
            image.sprite = emptyBottleSprite;
        }


        ////SaveDataのboolの配列に基づいてUIを更新
        // SaveDataのbottleStatesの数に応じてボトルの表示を満タンに
        Debug.Log($"{currentSaveData.bottleStates}個のボトルが満タンです");
        for (int i = 0; i < currentSaveData.bottleStates/*.Length*/; i++)
        {
            //bottleUIImages[i].sprite = currentSaveData.bottleStates[i] ? fullBottleSprite : emptyBottleSprite;
            bottleUIImages[i].sprite = fullBottleSprite;
        }



        // セーブ処理を呼び出す
        SaveManager.Instance.SaveGame();
        Debug.Log($"{this.name} : セーブデータの保存を呼びました。");



        //// タコへの信号
        //CheckTakoInteraction();
    }



    //private void CheckTakoInteraction()
    //{
    //    //bool canInteract = currentSaveData.bottleStates.Count(state => state) > 0;
    //    bool canInteract = currentSaveData.bottleStates > 0;

    //    if (takoController != null)
    //    {
    //        takoController.ConfirmObjectPlacement();
    //    }
    //}



    public void SignalBottleRecovered()
    {
        if (takoController != null)
        {
            takoController.RecoverCountAndGauge();
        }
    }





    // アイテム拾得時: ボトルを追加。右から優先
    public int RecoverBottles(int count)
    {
        if (!GetSaveData() ) { return 0; }
        // 右から順に探す
        //for (int i = currentSaveData.bottleStates.Length - 1; i >= 0; i--)
        //{
        //    // 空ボトルが見つかった場合
        //    if (!currentSaveData.bottleStates[i])
        //    {
        //        currentSaveData.bottleStates[i] = true; // 黄色ボトルに切り替え
        //        UpdateBottleUI();

        //        return true; // 追加成功
        //    }
        //}

        int recoveredCount = 0;
        for (int i = 0; i < count; i++)
        {
            if (SaveManager.Instance.currentData.bottleStates >= SaveManager.Instance.currentData.maxBottleCount)
            {
                break;
            }
            SaveManager.Instance.currentData.bottleStates++;
            recoveredCount++;
        }
        if (recoveredCount > 0)
        {
            Debug.Log($"{this.name} ボトルを{recoveredCount}個回復しました");
            UpdateBottleUI();
            return recoveredCount;
        }
        Debug.Log($"{this.name} ボトルの回復に失敗しました");
        return recoveredCount;


        //if (currentSaveData.bottleStates < 4)
        //{
        //    Debug.Log("ボトルを１つ回復しました");
        //    currentSaveData.bottleStates++;
        //    UpdateBottleUI();
        //    return true;
        //}
        // 空ボトルが見つからなかった場合
        //return false; // 追加失敗
    }

    public void ResetBottlesToFull()
    {
        //if (SaveManager.Instance == null || SaveManager.Instance.currentData == null)
        //{
        //    Debug.LogError("SaveManagerが見つからない。ボトルをリセットできないよ");
        //    return;
        //}
        ////bool[] states = SaveManager.Instance.currentData.bottleStates;
        //Debug.Log($"リセット前のボトルの数は{SaveManager.Instance.currentData.bottleStates}個です。");
        //SaveManager.Instance.currentData.bottleStates = SaveManager.Instance.currentData.maxBottleCount;
        //Debug.Log($"リセット後のボトルの数は{SaveManager.Instance.currentData.bottleStates}個です。");
        //// 全てのボトルを満タンに設定
        ////for (int i = 0; i < states.Length; i++)
        ////{
        ////    states[i] = true;
        ////}
        //UpdateBottleUI();

        RecoverBottles(SaveManager.Instance.currentData.maxBottleCount);
    }

    public bool TryConsumeBottle()
    {
        if (!GetSaveData())
        {
            return false;
        }

        if (currentSaveData.bottleStates > 0)
        {
            return true;
        }

        return false;
    }

    public bool IsBottleMax()
    {
        if (!GetSaveData())
        {
            return false;
        }

        if (currentSaveData.bottleStates == currentSaveData.maxBottleCount)
        {
            return true;
        }
        return false;
    }



    public bool ConsumeBottles(int count)
    {
        if (count > SaveManager.Instance.currentData.bottleStates)
        {
            Debug.Log($"{this.name} : 消費数({count})は現在のボトル({SaveManager.Instance.currentData.bottleStates})を上回ります。");
            return false;
        }
        //SaveData data = SaveManager.Instance.currentData;
        //bool[] states = data.bottleStates;

        //int indexToEmpty = -1;
        //for (int i = 0; i < states.Length; i++)
        //{
        //    if (states[i] == true)
        //    {
        //        indexToEmpty = i;
        //        break;
        //    }
        //}

        // 満タンのボトルが見つかった場合(消費可能)
        SaveManager.Instance.currentData.bottleStates -= count;
        Debug.Log($"ボトルを{count}個消費しました。");
        // UIを更新
        UpdateBottleUI();
        return true;    // 消費成功

        // 満タンのボトルが見つかった場合(消費可能)
        //if (indexToEmpty != -1)
        //{
        //    SaveManager.Instance.currentData.bottleStates[indexToEmpty] = false;
        //    // 状態を空に設定
        //    //states[indexToEmpty] = false;

        //    // UIを更新
        //    UpdateBottleUI();

        //    // データを保存
        //    SaveManager.Instance.SaveGame();

        //    //SaveManager.Instance.NotifyDataChanged();

        //    Debug.Log($"ボトル[{indexToEmpty}]を空にしました。");

        //    //OnBottleConsumed?.Invoke();

        //    return true; // 消費成功
        //}
        //SaveManager.Instance.SaveGame();

        //return false; // 消費失敗
    }
}

