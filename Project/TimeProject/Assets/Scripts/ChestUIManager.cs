using UnityEngine;
using UnityEngine.UI;

public class ChestUIManager : MonoBehaviour
{
    // 宝箱UI関連
    public GameObject closedChestUI; // 閉じた宝箱のUI
    public GameObject openedChestUI; // 空いた宝箱のUI

    public Image ChestImage; // チェストのUIを表示する

    // インベントリUI関連
    public GameObject inventoryPanel; // インベントリのPanel

    private bool isChestOpen = false; // 宝箱が開いているかどうかの状態
    // Start関数で初期状態を設定
    void Start()
    {
        // 閉じたUIを表示、開いたUIを非表示
        closedChestUI.SetActive(true);
        openedChestUI.SetActive(false);
        inventoryPanel.SetActive(false);

        isChestOpen = false;
    }

    // UIのクリックイベントから呼び出されるメソッド
    public void OnChestUIClick()
    {
        // 宝箱がまだ空いていない場合のみ処理を実行
        if (!isChestOpen)
        {
            // UIを切り替える
            closedChestUI.SetActive(false); // 閉じたUIを非表示
            openedChestUI.SetActive(true); // 空いたUiを表示
            inventoryPanel.SetActive(true); // インベントリの表示

            // 状態を更新
            isChestOpen = true;

            // 宝箱を開けた時のログ
            Debug.Log("宝箱を開けました！");
        }
        else
        {
            // 宝箱を閉じる処理
            closedChestUI.SetActive(true); // 閉じた宝箱を表示
            openedChestUI.SetActive(false); // 空いた宝箱を表示
            inventoryPanel.SetActive(false); // インベントリの非表示

            // 状態を更新
            isChestOpen = false;
            Debug.Log("宝箱を閉じました！");
        }
    }
}

