using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI設定")]
    [Tooltip("スペースのUI")]
    [SerializeField] private GameObject spaceUI;
    [Tooltip("オブジェクトごとのエフェクト")]
    [SerializeField] private GameObject activeUI;

    private GameObject _spawonUI;
    private GameObject _newActiveUI;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            //UI生成
            _spawonUI = Instantiate(spaceUI);
            _spawonUI.transform.SetParent(this.transform); // 親をこのスクリプトのGameObjectに設定
           // Debug.Log("UI表示完了");

            //位置情報
            Vector3 spawonPos = _spawonUI.transform.position;
            spawonPos.x = gameObject.transform.position.x;
            spawonPos.y = gameObject.transform.position.y + 1.5f;
            spawonPos.z = gameObject.transform.position.z;
            _spawonUI.transform.position = spawonPos;
            _spawonUI.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            //Debug.Log(collision.gameObject.name + "が接触した");

            _newActiveUI = Instantiate(activeUI, _spawonUI.transform);
            _newActiveUI.transform.localPosition = new Vector3(0, 0.5f, 0); // 親の中心に配置


            SpriteRenderer activeRenderer = _newActiveUI.GetComponent<SpriteRenderer>();
            SpriteRenderer spaceRenderer = _spawonUI.GetComponent<SpriteRenderer>();

            if (activeRenderer != null && spaceRenderer != null)
            {
                // spaceUIと同じSorting Layerに設定
                activeRenderer.sortingLayerName = spaceRenderer.sortingLayerName;

                // spaceUIより1だけ前に出す
                activeRenderer.sortingOrder = spaceRenderer.sortingOrder + 1;
            }


        }
    }


    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Destroy(_spawonUI);
            Destroy(_newActiveUI);
            //Debug.Log(collision.gameObject.name + "が離れた");
        }
    }

}
