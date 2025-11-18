using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ObjectController1 : MonoBehaviour
{
    [Header("Layers")]
    [SerializeField] private LayerMask selectableLayer;
    [SerializeField] private LayerMask itemLayer;
    [SerializeField] private LayerMask waterLayer;
    [SerializeField] private LayerMask defaultLayer;
    [SerializeField] private LayerMask craftLayer;

    [Header("設定")]
    [Header("Object")]
    [SerializeField] private float gridSize = 1f;
    [SerializeField] private float maxStepHeight = 2f;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private Sprite[] handGauge;
    [SerializeField] private Image[] Gauge;

    [Header("Item")]
    [SerializeField] private float DOWN = 1.25f;
    [SerializeField] private float setti = 0f;

    [Header("管理対象")]
    [SerializeField] private List<GameObject> managedObjects = new List<GameObject>();

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
    private GameObject selectedItem;
    private Vector3 offset;
    private int count = 0;
    private float objectZ;

    void Update()
    {
        bool mouseLeft = Input.GetMouseButton(0);
        bool mouseRightDown = Input.GetMouseButtonDown(1);
        bool mouseLeftDown = Input.GetMouseButtonDown(0);
        bool mouseLeftUp = Input.GetMouseButtonUp(0);
        // アイテム選択・移動・設置
        if (mouseLeftDown && selectedObject == null) TrySelectItem();

        if (mouseLeft && selectedItem != null) TryMoveSelectedItem();

        if (mouseLeftUp && selectedItem != null) ConfirmItemPlacement();

        // オブジェクト選択・移動・設置
        if (mouseLeftDown && selectedItem == null) TrySelectObject();

        if (mouseLeft && selectedObject != null) TryMoveSelectedObject();

        if (mouseRightDown && selectedObject != null) ConfirmObjectPlacement();

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

                if (Physics.Raycast(gridPos + Vector3.up * 5f, Vector3.down, out RaycastHit groundHit, 10f, waterLayer))
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
        {
            cursorRenderer.sprite = cursorSpriteDefault; // Rayが何も当たらない場合
        }
    }

    // ---------------- オブジェクト関連 ----------------
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
            if (col != null) col.isTrigger = true;

            if (takoControllerScript != null) takoControllerScript.enabled = false;
            if (takoAnimator != null) takoAnimator.SetBool("isLifting", true);
        }
    }

    void TryMoveSelectedObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //object管理

        if (!Physics.Raycast(ray, out RaycastHit hit, 100f, waterLayer)) return;
        Vector3 gridPos = new Vector3(
            Mathf.Round(hit.point.x / gridSize) * gridSize,
            0,
            Mathf.Round(hit.point.z / gridSize) * gridSize
        );

        if (Physics.Raycast(gridPos + Vector3.up * 5f, Vector3.down, out RaycastHit groundHit, 10f, waterLayer))
        {
            float heightDiff = groundHit.point.y - selectedObject.transform.position.y;
            if (heightDiff > maxStepHeight) return;

            Vector3 newCenter = groundHit.point + Vector3.up * (selectedObject.transform.localScale.y / 2f);

            // 他オブジェクトとの距離チェック
            GameObject[] allObjects = GameObject.FindGameObjectsWithTag("Selectable");
            GameObject[] allObjectsIve = GameObject.FindGameObjectsWithTag("Ivent");
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

            foreach (GameObject objIve in allObjectsIve)
            {
                if (objIve == selectedObject) continue;
                BoxCollider col = objIve.GetComponent<BoxCollider>();
                if (col == null) continue;
                float distance = Vector3.Distance(newCenter, col.bounds.center);
                if (distance < minDistance)
                {
                    Debug.Log("他のオブジェクトと近すぎるため移動できません");
                    return;
                }
            }

            Vector3 pos = new Vector3(gridPos.x, Mathf.Max(groundHit.point.y, setti), gridPos.z);
            selectedObject.transform.position = pos;

            // Tako追従
            if (takoTransform != null)
            {
                Vector3 targetPos = selectedObject.transform.position + new Vector3(0, takoFollowYOffset, takoFollowZOffset);
                takoTransform.position = Vector3.Lerp(takoTransform.position, targetPos, Time.deltaTime * takoFollowSpeed);
            }
        }
    }

    void ConfirmObjectPlacement()
    {
        if (selectedObject != null)
        {
            Collider col = selectedObject.GetComponent<Collider>();
            if (col != null) col.isTrigger = false;

            if (takoControllerScript != null)
                takoControllerScript.enabled = true;
            if (takoAnimator != null)
                takoAnimator.SetBool("isLifting", false);

            selectedObject.tag = "Selectable";
            selectedObject = null;

            count++;
            Gauge[count - 1].sprite = handGauge[1];
        }
    }

    // ---------------- アイテム関連 ----------------
    void TrySelectItem()
    {
        if (selectedItem != null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, itemLayer))
        {
            selectedItem = hit.collider.gameObject;
            objectZ = Camera.main.WorldToScreenPoint(selectedItem.transform.position).z;
            offset = selectedItem.transform.position -
                     Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, objectZ));

            //if (takoControllerScript != null)
            //    takoControllerScript.enabled = false;

            //if (takoAnimator != null)
            //    takoAnimator.SetBool("isLifting", true);
        }
    }

    void TryMoveSelectedItem()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //if (!Physics.Raycast(ray, out RaycastHit hit, 100f, defaultLayer)) return;
        //Vector3 gridPos = new Vector3(
        //    Mathf.Round(hit.point.x / gridSize) * gridSize,
        //    0,
        //    Mathf.Round(hit.point.z / gridSize) * gridSize
        //);

        //if (Physics.Raycast(gridPos + Vector3.up * 5f, Vector3.down, out RaycastHit groundHit, 10f, defaultLayer))
        //{
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, defaultLayer))
        {
            Vector3 targetPos = hit.point + offset;
            targetPos.y = Mathf.Max(targetPos.y, DOWN);
            selectedItem.transform.position = targetPos;
        }

        if (Physics.Raycast(ray, out RaycastHit craftHit, 100f, craftLayer))
        {
            Vector3 aboveBlock = craftHit.point + new Vector3(0, setti, 0);
            selectedItem.transform.position = aboveBlock;

            Collider col = selectedItem.GetComponent<Collider>();
            if (col != null)
                col.isTrigger = true;

            //// マウス先にオブジェクトがある場合
            //if (((1 << craftHit.collider.gameObject.layer) & itemLayer) != 0)
            //{
            //    Vector3 cursorPos;
            //    cursorPos = craftHit.collider.bounds.center;
            //    cursorPos.y = craftHit.collider.bounds.max.y + 1f; // オブジェクトの上にカーソル
        }

    }


    void ConfirmItemPlacement()
    {
        if (selectedItem != null)
        {
            Collider col = selectedItem.GetComponent<Collider>();
            if (col != null)
                col.isTrigger = false;

            //if (takoControllerScript != null)
            //    takoControllerScript.enabled = true;

            //if (takoAnimator != null)
            //    takoAnimator.SetBool("isLifting", false);

            selectedItem.tag = "after";
            selectedItem = null;
        }
    }

    // ---------------- 共通 ----------------
    void FollowTako(Vector3 targetPos)
    {
        if (takoTransform != null)
        {
            Vector3 followPos = targetPos + new Vector3(0, takoFollowYOffset, takoFollowZOffset);
            takoTransform.position = Vector3.Lerp(takoTransform.position, followPos, Time.deltaTime * takoFollowSpeed);
        }
    }
}
