using UnityEngine;
using UnityEngine.UI;

public class ObjectController : MonoBehaviour
{
    public LayerMask selectableLayer, groundLayer;

    public float gridSize = 1f;
    public float maxStepHeight = 2f;
    public float minDistance = 1f;
    [SerializeField] float UP = 0.5f;

    [SerializeField] Sprite[] handGauge;
    [SerializeField] Image[] Gauge;

    private GameObject selectedObject;
    private int count = 0;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) TrySelectObject(); // オブジェクト選択
        if (selectedObject && Input.GetMouseButton(0)) TryMoveSelectedObject(); // 移動
        if (Input.GetMouseButtonDown(1)) ConfirmPlacement(); // 右クリックで確定

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

            Collider col = GetComponent<Collider>();
            if (col != null)
                col.isTrigger = true;
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
            Collider col = GetComponent<Collider>();
            if (col != null)
                col.isTrigger = false;

            count++;
            Gauge[count - 1].sprite = handGauge[1];

            selectedObject.tag = "Selectable"; // 再び選択可能に
            selectedObject = null;
        }
    }
}
