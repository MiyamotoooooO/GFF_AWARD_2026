using UnityEngine;

[System.Serializable]
public class SaveData
{
    public float[] respawnPosition = new float[3]; // 現在のリスポーン位置
    public bool[] checkpointReached; // 各チェックポイントの到達状況
}
