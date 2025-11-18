using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int checkpointID; // 各チェックポイントにユニークなIDを設定
    //public OxygenGaugeController oxygenGaugeController;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Player")
        {
            SaveManager save = SaveManager.Instance;

            if (!save.currentData.checkpointReached[checkpointID])
            {
                // 初めて踏んだときのみ更新
                save.currentData.checkpointReached[checkpointID] = true;

                Vector3 pos = transform.position;
                save.currentData.respawnPosition[0] = pos.x;
                save.currentData.respawnPosition[1] = pos.y;
                save.currentData.respawnPosition[2] = pos.z;

                if (OxygenGaugeController.Instance != null)
                {
                    // 酸素の最大値をリセット
                    OxygenGaugeController.Instance.ResetToMaxOxygen();
                }

                save.SaveGame();
                Debug.Log($"チェックポイント {checkpointID} を更新！");
            }
        }
    }
}


