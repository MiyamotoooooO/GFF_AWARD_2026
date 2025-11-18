using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class BottleUIManager : MonoBehaviour
{
    public Image[] bottleUIImages;

    // 黄色のボトルのスプライト
    public Sprite fullBottleSprite;
    // 空のボトルのスプライト
    public Sprite emptyBottleSprite;

    private BottleUIManager bottleUIManager;
    private SaveData currentSaveData; // セーブデータの参照
    public ObjectManager takoController;
    private int BottleCount => currentSaveData.bottleStates.Count(state => state); // 現在の黄色ボトルの本数

    void Start()
    {
        if (currentSaveData == null)
        {
            currentSaveData = new SaveData();
        }

        UpdateBottleUI();
        //CheckTakoInteraction();
    }

    // UIの表示を現在のSaveDataに基づいてUIを更新する
    public void UpdateBottleUI()
    {
        SaveData bottleSaveData = currentSaveData;

        // UIが設定されていない場合はエラーを出す
        if (bottleUIImages.Length != currentSaveData.bottleStates.Length)
        {
            Debug.LogError("UI画像の数とセーブデータの配列サイズが一致しません。");
            return;
        }

        // SaveDataのboolの配列に基づいてUIを更新
        for (int i = 0; i < currentSaveData.bottleStates.Length; i++)
        {
            bottleUIImages[i].sprite = currentSaveData.bottleStates[i] ? fullBottleSprite : emptyBottleSprite;
        }

        // セーブ処理を呼び出す
        SaveManager.Instance.SaveGame();

        // タコへの信号
        CheckTakoInteraction();

    }

    private void CheckTakoInteraction()
    {
        bool canInteract = currentSaveData.bottleStates.Count(state => state) > 0;

        if (takoController != null)
        {
            takoController.ConfirmPlacement();
        }
    }

    public void SignalBottleRecovered()
    {
        if (takoController != null)
        {
            takoController.RecoverCountAndGauge();
        }
    }



    // アイテム拾得時: ボトルを追加。右から優先
    public bool TryAddBottle()
    {
        // 右から順に探す
        for (int i = currentSaveData.bottleStates.Length - 1; i >= 0; i--)
        {
            // 空ボトルが見つかった場合
            if (!currentSaveData.bottleStates[i])
            {
                currentSaveData.bottleStates[i] = true; // 黄色ボトルに切り替え
                UpdateBottleUI();

                return true; // 追加成功
            }
        }
        // 空ボトルが見つからなかった場合
        return false; // 追加失敗
    }

    public bool ConsumeOneBottle()
    {
        SaveData data = SaveManager.Instance.currentData;
        bool[] states = data.bottleStates;

        int indexToEmpty = -1;
        for (int i = 0; i < states.Length; i++)
        {
            if (states[i] == true)
            {
                indexToEmpty = i;
                break;
            }
        }

        // 満タンのボトルが見つかった場合(消費可能)
        if (indexToEmpty != -1)
        {
            SaveManager.Instance.currentData.bottleStates[indexToEmpty] = false;
            // 状態を空に設定
            //states[indexToEmpty] = false;

            // UIを更新
            UpdateBottleUI();

            // データを保存
            SaveManager.Instance.SaveGame();

            //SaveManager.Instance.NotifyDataChanged();

            Debug.Log($"ボトル[{indexToEmpty}]を空にしました。");

            //OnBottleConsumed?.Invoke();

            return true; // 消費成功
        }
        SaveManager.Instance.SaveGame();

        return false; // 消費失敗
    }
}

