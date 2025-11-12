using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    public Transform player;
    public Transform tako;

    void Start()
    {
        var data = SaveManager.Instance.currentData;
        Vector3 spawnPos = new Vector3(
        data.respawnPosition[0],
        data.respawnPosition[1] + 0.7f,
        data.respawnPosition[2]
        );

        player.position = spawnPos;
        tako.position = spawnPos;
        Debug.Log("リスポーン位置にプレイヤーを配置: " + spawnPos);
    }

    // ゲームオーバー時に再配置する関数例
    public void RespawnPlayer()
    {
        var data = SaveManager.Instance.currentData;
        Vector3 respawn = new Vector3(
        data.respawnPosition[0],
        data.respawnPosition[1] + 0.7f,
        data.respawnPosition[2]
        );
        player.position = respawn;
        tako.position = respawn;
    }
}

