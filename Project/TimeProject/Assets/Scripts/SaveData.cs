using UnityEngine;

[System.Serializable]
public class SaveData
{
    public float[] respawnPosition = new float[3]; // 現在のリスポーン位置
    public bool[] checkpointReached; // 各チェックポイントの到達状況

    public bool[] bottleStates = new bool[4] { true, true, true, true }; // 黄色ボトルの状況
}
