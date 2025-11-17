using UnityEngine;
using System.Linq;

public class BottleItem : MonoBehaviour
{
    // 参照するUI管理スプリクト
    private BottleUIManager bottleUIManager;

    void Awake()
    {
        bottleUIManager = FindObjectOfType<BottleUIManager>();
        if (bottleUIManager == null)
        {
            Debug.LogError("シーンにボトルUIのスプリクトが見つかりません！");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("ボトルに触れた");

            PickUpBottle();
        }
    }
    private void PickUpBottle()
    {
        Debug.Log("ボトルの獲得に試みる！");

        if (SaveManager.Instance == null || SaveManager.Instance.currentData == null)
        {
            Debug.LogError("SaveManagerまたはcurrentDataがnullです。");
            return;
        }

        SaveData data = SaveManager.Instance.currentData;
        bool[] states = data.bottleStates;

        //【要件:全部黄色のボトルで満タンになったらアイテムが拾えなくなる】
        if (states.All(state => state == true))
        {
            Debug.Log("ボトルは満タンです。アイテムを獲得できません。");
            return; // 処理を終了
        }

        //【要件:複数本空だったら右から優先的に黄色のボトルに切り替わる】
        //int indexToFull = -1; // ・Fill
        bool wasEmpty = false;
        for (int i = 0; i < states.Length; i++)
        {
            if (states[i] == false)
            {
                states[i] = true; // 右から最初に空のボトルのインデックス ・Fill
                wasEmpty = true;
                //break;
            }
        }

        if (wasEmpty) // ・Fill
        {
            // 空のボトルを見つけたから満タンにする
            //states[indexToFull] = true; // ・Fill
            //Debug.Log($"ボトル[{indexToFull}]を補充しました。"); // ・Fill

            // UIを更新
            if (bottleUIManager != null)
            {
                bottleUIManager.SignalBottleRecovered();
            }

            if (bottleUIManager != null)
            {
                bottleUIManager.UpdateBottleUI();
            }

            // セーブデータに変更を保存
            SaveManager.Instance.SaveGame();

            // アイテムオブジェクトを削除
            Destroy(gameObject);
        }
    }
}




/*using UnityEngine;
using System.Linq;

public class BottleItem : MonoBehaviour
{
    // 参照するUI管理スプリクト
    private BottleUIManager bottleUIManager;

    void Awake()
    {
        bottleUIManager = FindObjectOfType<BottleUIManager>();
        if (bottleUIManager == null)
        {
            Debug.LogError("シーンにボトルUIのスプリクトが見つかりません！");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("ボトルに触れた");

            PickUpBottle();
        }
    }
    private void PickUpBottle()
    {
        Debug.Log("ボトルの獲得に試みる！");

        if (SaveManager.Instance == null || SaveManager.Instance.currentData == null)
        {
            Debug.LogError("SaveManagerまたはcurrentDataがnullです。");
            return;
        }

        SaveData data = SaveManager.Instance.currentData;
        bool[] states = data.bottleStates;

        //【要件:全部黄色のボトルで満タンになったらアイテムが拾えなくなる】
        if (states.All(state => state == true))
        {
            Debug.Log("ボトルは満タンです。アイテムを獲得できません。");
            return; // 処理を終了
        }

        //【要件:複数本空だったら右から優先的に黄色のボトルに切り替わる】
        //int indexToFull = -1; // ・Fill
        bool wasEmpty = false;
        for (int i = 0; i < states.Length; i++)
        {
            if (states[i] == false)
            {
                states[i] = true; // 右から最初に空のボトルのインデックス ・Fill
                wasEmpty = true;
                //break;
            }
        }

        if (wasEmpty) // ・Fill
        {
            // 空のボトルを見つけたから満タンにする
            //states[indexToFull] = true; // ・Fill
            //Debug.Log($"ボトル[{indexToFull}]を補充しました。"); // ・Fill

            // UIを更新
            if (bottleUIManager != null)
            {
                bottleUIManager.SignalBottleRecovered();
            }

            if (bottleUIManager != null)
            {
                bottleUIManager.UpdateBottleUI();
            }

            // セーブデータに変更を保存
            SaveManager.Instance.SaveGame();

            // アイテムオブジェクトを削除
            Destroy(gameObject);
        }

    }
}*/

