using UnityEngine;
using UnityEngine.UI;

public class ObjectController : MonoBehaviour
{
    public LayerMask selectableLayer, groundLayer;
    public float gridSize = 1f;
    public float maxStepHeight = 2f;
    public float minDistance = 1f;
    public float UP = 0f;

    [SerializeField] private Sprite[] handGauge;
    [SerializeField] private Image[] Gauge;

    [Header("Tako追従設定")]
    [SerializeField] private MonoBehaviour takoControllerScript; // TakoController スクリプト
    [SerializeField] private Transform takoTransform;            // TakoのTransform
    [SerializeField] private float takoFollowSpeed = 5f;         // 追従速度
    [SerializeField] private float takoFollowYOffset = 0.8f;       // Y方向オフセット
    [SerializeField] private float takoFollowZOffset = 0f;       // Z方向オフセット
    [SerializeField] private Animator takoAnimator;              // アニメーター

    private GameObject selectedObject;
    private int count = 0;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) TrySelectObject();      // 左クリックで選択
        if (selectedObject && Input.GetMouseButton(0)) TryMoveSelectedObject(); // 移動中
        if (Input.GetMouseButtonDown(1)) ConfirmPlacement();     // 右クリックで確定

        Vector3 pos1 = transform.position;
        if (pos1.y < 0.5f)
        {
            pos1.y = 0.5f;
            transform.position = pos1;
        }
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

            // 自分のColliderがあればトリガーにする（無効化）
            Collider col = GetComponent<Collider>();
            if (col != null)
                col.isTrigger = true;

            // Tako追従スクリプトOFF
            if (takoControllerScript != null)
                takoControllerScript.enabled = false;

            // Takoアニメーションを力む状態に変更
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

            // 他のオブジェクトとの距離チェック
            GameObject[] allObjects = GameObject.FindGameObjectsWithTag("Selectable");
            foreach (GameObject obj in allObjects)
            {
                if (obj == selectedObject) continue;
                BoxCollider col = obj.GetComponent<BoxCollider>();
                if (col == null) continue;
                Vector3 otherCenter = col.bounds.center;
                float distance = Vector3.Distance(newCenter, otherCenter);
                if (distance < minDistance)
                {
                    Debug.Log("他のオブジェクトと近すぎるため移動できません");
                    return;
                }
            }

            Vector3 pos = new Vector3(gridPos.x, Mathf.Max(groundHit.point.y, UP), gridPos.z);
            selectedObject.transform.position = pos;

            // Takoを追従させる
            if (takoTransform != null && selectedObject != null)
            {
                Vector3 targetPos = selectedObject.transform.position + new Vector3(0, takoFollowYOffset, takoFollowZOffset);
                takoTransform.position = Vector3.Lerp(takoTransform.position, targetPos, Time.deltaTime * takoFollowSpeed);
            }
        }
        else
        {
            Debug.Log("段差が高すぎて登れません");
        }
    }

    void ConfirmPlacement()
    {
        if (selectedObject != null)
        {
            selectedObject.tag = "Selectable"; // 再び選択可能に
            selectedObject = null;

            Collider col = GetComponent<Collider>();
            if (col != null)
                col.isTrigger = false;

            count++;
            Gauge[count - 1].sprite = handGauge[1];

            // Takoスクリプト再ON
            if (takoControllerScript != null)
                takoControllerScript.enabled = true;

            // アニメーション解除
            if (takoAnimator != null)
                takoAnimator.SetBool("isLifting", false);
        }
    }
}
