using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointManager : MonoBehaviour
{
    public int CheckpointIndex
    {
        get
        {
            return currentCheckpointIndex;
        }
    }

    private List<Checkpoint> checkpoints;
    private int currentCheckpointIndex = 0;
    private bool isCannonPlaying = false;
    private int queuedCheckpointID = -1;



    // cannon実行の場合、処理が終わるまで待機
    // ジョブを実行



    private void Start()
    {
        foreach(var checkpoint in SaveManager.Instance.currentData.checkpointReached)
        {
            if (checkpoint)
            {
                currentCheckpointIndex++;
            }
        }
        Debug.Log($"{name}: Current checkpoint is at no.{currentCheckpointIndex}");

        checkpoints = new(FindObjectsByType<Checkpoint>(FindObjectsSortMode.None));
        checkpoints.Sort((a, b) => a.checkpointID - b.checkpointID);
        foreach (var checkpoint in checkpoints)
        {
            checkpoint.InjectCheckPointManager(this);
        }

        List<PlayerCannonLauncher> cannons = new(FindObjectsByType<PlayerCannonLauncher>(FindObjectsSortMode.None));
        foreach(var cannon in cannons)
        {
            cannon.InjectCheckPointManager(this);
        }
    }

    public void SetCannonState(bool state)
    {
        isCannonPlaying = state;
        if (!state && queuedCheckpointID != -1)
        {
            Debug.LogWarning($"{name}: from cannon");
            UpdateCheckpoint();
        }
    }

    public void OnCheckpointReached(int id)
    {
        Debug.Log($"{name}: Checkpoint no.{id} reached!");
        if(!isCannonPlaying)
        {
            Debug.LogWarning($"{name}: WRONG");
            queuedCheckpointID = id;
            UpdateCheckpoint();
        }
        else
        {
            queuedCheckpointID = id;
        }
    }

    private void UpdateCheckpoint()
    {
        checkpoints[queuedCheckpointID].UpdateSpriteVisibility(false);
        if (queuedCheckpointID + 1 < checkpoints.Count)
        {
            checkpoints[queuedCheckpointID + 1].UpdateSpriteVisibility(true);
        }
        SaveManager save = SaveManager.Instance;

        // ★ セーブデータ更新（初めて踏んだ時だけ）
        if (!save.currentData.checkpointReached[queuedCheckpointID])
        {
            save.currentData.checkpointReached[queuedCheckpointID] = true;

            Vector3 pos = checkpoints[queuedCheckpointID].transform.position;
            save.currentData.respawnPosition[0] = pos.x;
            save.currentData.respawnPosition[1] = pos.y;
            save.currentData.respawnPosition[2] = pos.z;

            // ★ 酸素・ボトル・タコ回復
            OxygenGaugeController.Instance?.RecoverFullOxygen();
            BottleUIManager.Instance?.ResetBottlesToFull();
            //ObjectController1.Instance?.ResetTakoCountAndGauge();

            save.SaveGame();
            Debug.Log($"{name}: チェックポイント {queuedCheckpointID} を更新！");
        }

        // ★ チェックポイントインデックスの更新（スプライト制御）
        if (queuedCheckpointID == currentCheckpointIndex)
        {
            //if (spawnedSprite != null)
            //    spawnedSprite.SetActive(false);

            if (checkpoints[currentCheckpointIndex].GetComponent<GoalCamera>() != null)
            {
                checkpoints[currentCheckpointIndex].GetComponent<GoalCamera>().OnStageClear();
            }
            currentCheckpointIndex++;

            //// 全チェックポイントに「表示更新」させる
            //Checkpoint[] cps = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
            //foreach (var cp in cps)
            //{
            //    cp.UpdateSpriteVisibility();
            //}
        }

        queuedCheckpointID = -1;
    }
}
