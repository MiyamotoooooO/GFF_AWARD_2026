using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class ObjectController1 : MonoBehaviour
{
    public static ObjectController1 Instance { get; private set; }
    [Header("Layers")]
    [SerializeField] private LayerMask selectableLayer; //idou
    [SerializeField] private LayerMask itemLayer;       //item
    [SerializeField] private LayerMask waterLayer;      //water
    [SerializeField] private LayerMask raycastLayer;    //rayCastMask
    [SerializeField] private LayerMask notRaycastLayer; //rayCastMask以外
    [SerializeField] private LayerMask defaultLayer;    //Default
    [SerializeField] private LayerMask craftLayer;      //craft

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

    private GameObject selectedObject; //オブジェクト格納
    private GameObject selectedItem;   //アイテム格納
    private Vector3 offset;
    private int count = 0;
    private float objectZ;

    //[Header("管理対象")]
    //[SerializeField] private List<GameObject> managedObjects = new List<GameObject>();

    [Header("Tako追従設定")]
    [SerializeField] private MonoBehaviour takoControllerScript;
    [SerializeField] private Transform takoTransform;
    [SerializeField] private float takoFollowSpeed = 5f;
    [SerializeField] private float takoFollowYOffset = 0.8f;
    [SerializeField] private float takoFollowZOffset = 0f;
    [SerializeField] private Animator takoAnimator;

    [Header("カーソル表示設定")]
    [SerializeField] private Texture cursorSpriteDefault; //red
    [SerializeField] private Texture cursorSpriteActive;  //green
    [SerializeField] private MeshRenderer cursorRenderer; //カーソル表示mesh
    [SerializeField] private float cursorWaterHeight;     //カーソル表示高さ
    [Header("オブジェクト選択中設定")]
    [SerializeField] private Color selectedColor;  //選んでいる最中の色
    [SerializeField] private Color savedColor;     //選んでいるときの色の保存object

    [Header("その他")]
    [SerializeField] private GameObject deathCauseParent;
    [SerializeField] private GameObject gameOverRootPanel;
    private BottleUIManager bottleUIManager;

    [Header("Debug")]
    //[SerializeField] private Vector3 checkPosition;
    //[SerializeField] private Transform checkObjectTransform;

    //オブジェクトの押し出し
    private Vector3[] positionOffsets =
    {
        Vector3.zero,
        Vector3.back,
        Vector3.left,
        Vector3.forward,
        Vector3.right,
        Vector3.back + Vector3.left,
        Vector3.forward + Vector3.left,
        Vector3.back + Vector3.right,
        Vector3.forward + Vector3.right,
    };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // ゲーム開始時は非表示
        if (gameOverRootPanel != null)
        {
            gameOverRootPanel.SetActive(false);
        }
    }

    void Update()
    {
        if(Time.timeScale == 1)
        {
            // アイテム選択・移動・設置
            if (Input.GetMouseButtonDown(0) && selectedObject == null) TrySelectItem();
            if (Input.GetMouseButton(0) && selectedItem != null) TryMoveSelectedItem();
            if (Input.GetMouseButtonUp(0) && selectedItem != null) ConfirmItemPlacement();

            // オブジェクト選択・移動・設置
            if (Input.GetMouseButtonDown(0) && selectedItem == null) TrySelectObject();
            if (Input.GetMouseButton(0) && selectedObject != null) TryMoveSelectedObject();
            if (Input.GetMouseButtonDown(1) && selectedObject != null) ConfirmObjectPlacement();
        }

        UpdateCursorSprite();
    }

    //----------------- カーソル表示関連 ---------------
    void ChangeMaterialColor(Renderer renderer, Color color)
    {
        MaterialPropertyBlock mpb = new();
        mpb.SetColor("_Color", color);
        renderer.SetPropertyBlock(mpb);
    }

    void ChangeMaterialTexture(Renderer renderer, Texture texture)
    {
        MaterialPropertyBlock mpb = new();
        mpb.SetTexture("_MainTex", texture);
        renderer.SetPropertyBlock(mpb);
    }

    void UpdateCursorSprite()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, notRaycastLayer))
        {
            Vector3 cursorPos;
            LayerMask selectedLayer = new();

            //checkObjectTransform.position = hit.point;
            // マウス先にオブジェクトがある場合
            if ((hit.collider.gameObject.layer == selectableLayer) ||
                (hit.collider.gameObject.layer == itemLayer))
            {
                cursorPos = hit.collider.bounds.center;
                cursorPos.y = hit.collider.bounds.max.y; // オブジェクトの上にカーソル
            }
            else
            {
                // 通常のグリッド位置
                Vector3 gridPos = new Vector3(
                    Mathf.Round((hit.point.x - hit.normal.x * 0.1f) / gridSize) * gridSize,
                    hit.point.y,
                    Mathf.Round((hit.point.z - hit.normal.z * 0.1f) / gridSize) * gridSize
                );


                //Renderer renderer = checkObjectTransform.GetComponent<Renderer>();

                if (selectedObject != null)
                {
                    cursorPos = selectedObject.transform.position + Vector3.up * 0.5f;
                    selectedLayer = selectableLayer;
                }
                else if (Physics.Raycast(gridPos + Vector3.up * 5f, Vector3.down, out hit, 10f, defaultLayer))
                {
                    //ChangeMaterialColor(renderer, Color.green);
                    cursorPos = new Vector3(gridPos.x, Mathf.Max(hit.point.y, 1f), gridPos.z);
                    selectedLayer = defaultLayer;
                }
                else if (Physics.Raycast(gridPos + Vector3.up * 5f, Vector3.down, out hit, 10f, selectableLayer))
                {
                    //ChangeMaterialColor(renderer, Color.green);
                    cursorPos = new Vector3(gridPos.x, Mathf.Max(hit.point.y, 1f), gridPos.z);
                    selectedLayer = selectableLayer;
                }
                else if (Physics.Raycast(gridPos + Vector3.up * 4, Vector3.down, out hit, 10f, waterLayer))
                {
                    //ChangeMaterialColor(renderer, Color.blue);
                    Physics.Raycast(ray, out hit, 100f, waterLayer);
                    gridPos = new Vector3(
                        Mathf.Round(hit.point.x / gridSize) * gridSize,
                        cursorWaterHeight,
                        Mathf.Round(hit.point.z / gridSize) * gridSize
                    );
                    cursorPos = gridPos;
                    selectedLayer = waterLayer;
                }
                else
                {
                    //ChangeMaterialColor(renderer, Color.white);
                    cursorPos = gridPos;
                }
            }

            // カーソル位置更新
            cursorRenderer.transform.position = cursorPos;

            // スプライト切り替え
            if ((selectedLayer == selectableLayer) ||
                (selectedLayer == itemLayer))
            {
                ChangeMaterialTexture(cursorRenderer, cursorSpriteActive);
                //cursorRenderer.sprite = cursorSpriteActive; // 移動可能
            }
            else
            {
                ChangeMaterialTexture(cursorRenderer, cursorSpriteDefault);
                //cursorRenderer.sprite = cursorSpriteDefault; // 通常
            }
        }
        else
        {
            ChangeMaterialTexture(cursorRenderer, cursorSpriteDefault);
            //cursorRenderer.sprite = cursorSpriteDefault; // Rayが何も当たらない場合
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

            Renderer renderer = selectedObject.GetComponent<Renderer>();
            MaterialPropertyBlock mpb = new();
            if (renderer.material.HasColor("_Color"))
            {
                savedColor = renderer.material.GetColor("_Color");
                mpb.SetColor("_Color", selectedColor);
                renderer.SetPropertyBlock(mpb);
            }
            
            //pastSelectedObject = selectedObject;
            Collider col = selectedObject.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            if (takoControllerScript != null) takoControllerScript.enabled = false;
            if (takoAnimator != null) takoAnimator.SetBool("isLifting", true);
        }
    }


    bool TryCollisionCheck(Vector3 position)
    {
        GameObject[] allObjects = GameObject.FindGameObjectsWithTag("Selectable");
        GameObject[] allObjectsIve = GameObject.FindGameObjectsWithTag("Ivent");
        foreach (GameObject obj in allObjects)
        {
            if (obj == selectedObject) continue;
            BoxCollider col = obj.GetComponent<BoxCollider>();
            if (col == null) continue;
            float distance = Vector3.Distance(position, col.bounds.center);
            if (distance < minDistance)
            {
                Debug.Log("他のオブジェクトと近すぎるため移動できません");
                return false;
            }
        }

        foreach (GameObject objIve in allObjectsIve)
        {
            if (objIve == selectedObject) continue;
            BoxCollider col = objIve.GetComponent<BoxCollider>();
            if (col == null) continue;
            float distance = Vector3.Distance(position, col.bounds.center);
            if (distance < minDistance)
            {
                Debug.Log("他のオブジェクトと近すぎるため移動できません");
                return false;
            }
        }

        return true;
    }

    void TryMoveSelectedObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //object管理

        if (!Physics.Raycast(ray, out RaycastHit hit, 100f, raycastLayer)) return;
        Vector3 gridPos = new Vector3(
            Mathf.Round(hit.point.x / gridSize) * gridSize,
            0,
            Mathf.Round(hit.point.z / gridSize) * gridSize
        );

        //if (Physics.Raycast(gridPos + Vector3.up * 5f, Vector3.down, out  RaycastHit groundHit, 10f, defaultLayer))



        //float heightDiff = hit.point.y - selectedObject.transform.position.y;
        //if (heightDiff > maxStepHeight) return;

        //Vector3 newCenter = groundHit.point + Vector3.up * (selectedObject.transform.localScale.y / 2f);
        Vector3 newCenter = new Vector3(
            Mathf.Round(hit.point.x / gridSize) * gridSize,
            setti,
            Mathf.Round(hit.point.z / gridSize) * gridSize
        );
        //checkPosition = newCenter;
        //checkPosition.y = hit.point.y;
        //checkObjectTransform.position = checkPosition;

        // 他オブジェクトとの距離チェック


        foreach (var offset in positionOffsets)
        {

            if (!TryCollisionCheck(newCenter + offset))
            {
                continue;
            }
            //if (!Physics.Raycast(ray, out hit, 100f, waterLayer)) return;
            //Vector3 gridPos = new Vector3(
            //    Mathf.Round(hit.point.x / gridSize) * gridSize,
            //    0,
            //    Mathf.Round(hit.point.z / gridSize) * gridSize
            //);
            selectedObject.transform.position = newCenter + offset;

            // Tako追従
            if (takoTransform != null)
            {
                Vector3 targetPos = selectedObject.transform.position + new Vector3(0, takoFollowYOffset, takoFollowZOffset);
                takoTransform.position = Vector3.Lerp(takoTransform.position, targetPos, Time.deltaTime * takoFollowSpeed);
            }
            break;
        }
    }
    public void ConfirmObjectPlacement()
    {
        if (selectedObject != null)
        {
            int currentBottleCount = GetCurrentBottleCount();

            Collider col = selectedObject.GetComponent<Collider>();
            if (col != null) col.isTrigger = false;

            if (takoControllerScript != null)
                takoControllerScript.enabled = true;
            if (takoAnimator != null)
                takoAnimator.SetBool("isLifting", false);

            Renderer renderer = selectedObject.GetComponent<Renderer>();
            if (renderer.material.HasColor("_Color"))
            {
                MaterialPropertyBlock mpb = new();
                mpb.SetColor("_Color", savedColor);
                renderer.SetPropertyBlock(mpb);
            }

            selectedObject.tag = "Selectable";
            selectedObject = null;

            //count++;
            Gauge[count].sprite = handGauge[1];

            if (SaveManager.Instance != null && SaveManager.Instance.currentData != null)
            {
                Debug.Log("【タコ】ボトル消費の開始");

                int bottleIndex = count;
                if (bottleIndex >= 0 && bottleIndex < SaveManager.Instance.currentData.bottleStates.Length)
                {
                    // ボトルを取得済みとしてマーク
                    SaveManager.Instance.currentData.bottleStates[bottleIndex] = false;
                    Debug.Log($"【タコ消費】ボトル[{bottleIndex}]をfalseに設定しました。");
                }
            }

            count++;
            SaveManager.Instance.SaveGame();
        }
    }

    //--------------- その他 ------------------
    public void ShowGameOverScreen()
    {
        if (gameOverRootPanel != null)
        {
            gameOverRootPanel.SetActive(true);
            Debug.Log("ゲームオーバー画面全体を表示しました。");
        }
        else
        {
            Debug.LogError("【設定ミス】GameOverRootPanelがインスペクターで設定されていません。");
            return;
        }

        if (deathCauseParent == null)
        {
            Debug.LogError("【設定ミス】DeathCauseParentがインスペクターで設定されていません。");
            return;
        }

        // 記録された死因名を取得
        string targetName = PlayerController.LastTouchedObjectName;
        Debug.Log($"UI検索を開始: '{targetName}' という名前のテキストを探し中");

        bool found = false;

        foreach (Transform child in deathCauseParent.transform)
        {
            child.gameObject.SetActive(false);

            if (child.gameObject.name.Trim() == targetName.Trim())
            {
                child.gameObject.SetActive(true);
                found = true;
                Debug.Log($"死因テキストを発見！表示します: {child.gameObject.name}");
            }
        }

        if (!found)
        {
            string allChildrenNames = "";
            foreach (Transform child in deathCauseParent.transform)
            {
                allChildrenNames += $"[{child.gameObject.name}], ";
            }
            Debug.LogError($"表示失敗！{targetName}と一致するテキストが見つかりませんでした。");
        }
    }

    public void RecoverCountAndGauge()
    {
        if (count <= 0)
        {
            return;
        }
        count--;

        //int gaugeIndexToReset = count;

        if (count >= 0 && count < Gauge.Length)
        {
            Gauge[count].sprite = handGauge[0];

            Debug.Log($"ボトルの回収に伴い、countが {count + 1} から {count} に回復しました。");
        }

        //if (count >= 0 && count < Gauge.Length)
        //{
        //    Gauge[count].sprite = handGauge[0];

        //    Debug.Log($"ボトルを1つ回復しました。countは {count} → {count + 1} です。");

        //    count++;
        //}
        //else Debug.Log("すでに全て回復しています。");

        SaveManager.Instance.SaveGame();
    }


    public void ResetTakoCountAndGauge()
    {
        // ゲージUIの見た目をすべて満タンにもどす
        for (int i = 0; i < Gauge.Length; i++)
        {
            if (Gauge[i] != null)
            {
                Gauge[i].sprite = handGauge[0];
            }
        }
        count = 0;
        Debug.Log($"【タコ制御】チェックポイントによりタコのcountを{count}に完全にリセットした。");
    }


    private int GetCurrentBottleCount()
    {
        if (bottleUIManager != null && SaveManager.Instance.currentData != null)
        {
            SaveManager.Instance.currentData.bottleStates.Count(state => state);
        }
        return 0;
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
}

