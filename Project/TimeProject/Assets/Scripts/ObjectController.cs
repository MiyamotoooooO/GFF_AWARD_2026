using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ObjectManager : MonoBehaviour
{
    [SerializeField] private LayerMask selectableLayer, groundLayer, itemLayer;
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private float maxStepHeight = 2f;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float UP = 0f;

    [SerializeField] private Sprite[] handGauge;
    [SerializeField] private Image[] Gauge;

    [Header("Tako追従設定")]
    [SerializeField] private MonoBehaviour takoControllerScript;
    [SerializeField] private Transform takoTransform;
    [SerializeField] private float takoFollowSpeed = 5f;
    [SerializeField] private float takoFollowYOffset = 0.8f;
    [SerializeField] private float takoFollowZOffset = 0f;
    [SerializeField] private Animator takoAnimator;


    [Header("カーソル表示設定")]
    [SerializeField] private Sprite cursorSpriteDefault;
    [SerializeField] private Sprite cursorSpriteActive;
    [SerializeField] private SpriteRenderer cursorRenderer;


    private GameObject selectedObject;
    private int count = 0;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) TrySelectObject();
        if (selectedObject && Input.GetMouseButton(0)) TryMoveSelectedObject();
        if (Input.GetMouseButtonDown(1)) ConfirmPlacement();

        UpdateCursorSprite();
    }





    void UpdateCursorSprite()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            Vector3 cursorPos;

            // マウス先にオブジェクトがある場合
            if (((1 << hit.collider.gameObject.layer) & selectableLayer) != 0 ||
                ((1 << hit.collider.gameObject.layer) & itemLayer) != 0)
            {
                cursorPos = hit.collider.bounds.center;
                cursorPos.y = hit.collider.bounds.max.y + 0f; // オブジェクトの上にカーソル
            }
            else
            {
                // 通常のグリッド位置
                Vector3 gridPos = new Vector3(
                    Mathf.Round(hit.point.x / gridSize) * gridSize,
                    hit.point.y / gridSize,
                    Mathf.Round(hit.point.z / gridSize) * gridSize
                );

                if (Physics.Raycast(gridPos + Vector3.up * 5f, Vector3.down, out RaycastHit groundHit, 10f, groundLayer))
                    cursorPos = new Vector3(gridPos.x, Mathf.Max(groundHit.point.y, 1f), gridPos.z);
                else
                    cursorPos = hit.point;
            }

            // カーソル位置更新
            cursorRenderer.transform.position = cursorPos;

            // スプライト切り替え
            if (((1 << hit.collider.gameObject.layer) & selectableLayer) != 0 ||
                ((1 << hit.collider.gameObject.layer) & itemLayer) != 0)
                cursorRenderer.sprite = cursorSpriteActive; // 移動可能
            else
                cursorRenderer.sprite = cursorSpriteDefault; // 通常
        }
        else
            cursorRenderer.sprite = cursorSpriteDefault; // Rayが何も当たらない場合
    }






    void TrySelectObject()
    {
        if (count >= 4)
        {
            Debug.Log("これ以上オブジェクトを選択できません");
            return;
        }
        if (selectedObject != null)
        {
            Debug.Log("オブジェクトを設置するまでほかのオブジェクトは選択できません");
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, selectableLayer))
        {
            selectedObject = hit.collider.gameObject;
            Collider col = selectedObject.GetComponent<Collider>();
            if (col != null)
                col.isTrigger = true;

            // Tako追従スクリプトOFF
            if (takoControllerScript != null)
                takoControllerScript.enabled = false;

            // アニメーション変更
            if (takoAnimator != null)
                takoAnimator.SetBool("isLifting", true);
        }

    }

    void TryMoveSelectedObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer)) return;

        Vector3 gridPos = new Vector3(
            Mathf.Round(hit.point.x / gridSize) * gridSize,
            0,
            Mathf.Round(hit.point.z / gridSize) * gridSize
        );

        if (Physics.Raycast(gridPos + Vector3.up * 5f, Vector3.down, out RaycastHit groundHit, 10f, groundLayer))
        {
            float heightDiff = groundHit.point.y - selectedObject.transform.position.y;
            if (heightDiff > maxStepHeight) return;

            Vector3 newCenter = groundHit.point + Vector3.up * (selectedObject.transform.localScale.y / 2f);

            // 他オブジェクトとの距離チェック
            GameObject[] allObjects = GameObject.FindGameObjectsWithTag("Selectable");
            foreach (GameObject obj in allObjects)
            {
                if (obj == selectedObject) continue;
                BoxCollider col = obj.GetComponent<BoxCollider>();
                if (col == null) continue;
                float distance = Vector3.Distance(newCenter, col.bounds.center);
                if (distance < minDistance)
                {
                    Debug.Log("他のオブジェクトと近すぎるため移動できません");
                    return;
                }
            }

            Vector3 pos = new Vector3(gridPos.x, Mathf.Max(groundHit.point.y, UP), gridPos.z);
            selectedObject.transform.position = pos;

            // Tako追従
            if (takoTransform != null)
            {
                Vector3 targetPos = selectedObject.transform.position + new Vector3(0, takoFollowYOffset, takoFollowZOffset);
                takoTransform.position = Vector3.Lerp(takoTransform.position, targetPos, Time.deltaTime * takoFollowSpeed);
            }
        }
    }
    
    void ConfirmPlacement()
    {
        if (selectedObject != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, selectableLayer))
            {
                selectedObject = hit.collider.gameObject;
                Collider col = selectedObject.GetComponent<Collider>();
                if (col != null)
                    col.isTrigger = false;
            }

            if (takoControllerScript != null)
                takoControllerScript.enabled = true;

            if (takoAnimator != null)
                takoAnimator.SetBool("isLifting", false);

            selectedObject.tag = "Selectable";
            selectedObject = null;

            Gauge[count].sprite = handGauge[1];

            if (SaveManager.Instance != null && SaveManager.Instance.currentData != null)
            {
                int bottleIndex = count - 1;
                if (bottleIndex >= 0 && bottleIndex < SaveManager.Instance.currentData.bottleStates.Length)
                {
                    // ボトルを取得済みとしてマーク
                    SaveManager.Instance.currentData.bottleStates[bottleIndex] = false;
                }
            }

            count++;

        }

        // データを保存
        SaveManager.Instance.SaveGame();
    }
}
