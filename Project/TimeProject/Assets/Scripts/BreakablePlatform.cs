using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakablePlatform : MonoBehaviour
{
    [Header("Playerタグを持つオブジェクトだけ反応")]
    public string playerTag = "Player";


    [Header("何回踏まれたら壊れるか")]
    public int breakCount = 3;


    [Header("壊れるまでの遅延(秒)")]
    public float breakDelay = 0.5f;


    [Header("踏むごとに変化させるスプライト(踏んだ順)")]
    public Material[] damageMaterials;

    private int currentCount = 0;
    private bool isBreaking = false;

    private MeshRenderer meshRenderer;

    private AudioSource breakAudio; //確定音



    void Start()
    {
        breakAudio = GetComponent<AudioSource>();

        meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer == null)
        {
            Debug.Log("gameobjectにMeshRendererがありません。");
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        //Playerタグ以外は無視
        if (!collision.gameObject.CompareTag(playerTag)) return;

        //壊れかけてたら無視 
        if (isBreaking) return;

        currentCount++;

        Debug.Log("gameObjectが踏まれた");

        // 崩壊音（このオブジェクトから鳴らす(踏むたび)）
        if (breakAudio != null)
        {
            breakAudio.Play();
        }
        //スプライトを変更
        UpdateMaterial();

        //指定回数を超えたら崩壊
        if (currentCount >= breakCount)
        {
            StartCoroutine(BreakPlatform());
            // 壊れる瞬間の音
            Debug.Log("壊れた音再生");
            if (breakAudio != null)
                breakAudio.Play();
        }


    }
    private void UpdateMaterial()
    {
        if (meshRenderer == null || damageMaterials == null || damageMaterials.Length == 0)
            return;

        //配列の範囲内ならスプライトを変更
        int index = Mathf.Min(currentCount - 1, damageMaterials.Length - 1);
        meshRenderer.material = damageMaterials[index];

        Debug.Log("マテリアル変更");


    }

    private System.Collections.IEnumerator BreakPlatform()
    {
        isBreaking = true;


        //崩れる演出を入れたい場合はここ(例えば点滅など)
        yield return new WaitForSeconds(breakDelay);



        //消える処理
        Debug.Log("gameObjectは削除されました");
        Destroy(gameObject);

    }




}


