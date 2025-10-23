using UnityEngine;

public class GridObjectController : MonoBehaviour
{
    public LayerMask selectableLayer;
    public LayerMask groundLayer;
    public LayerMask obstacleLayer;
    //public Material highlightMaterial;
    public Color _colorCode;
    public float gravityScale = 8f;
    public float gridSize = 1f;
    public float maxStepHeight = 1f;
    public float minDistance = 1f;

    private GameObject selectedObject;
    private Material originalMaterial;
    private Rigidbody _rb;


    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false; //Unityの標準重力を無効化（独自の重力を適応）
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) //オブジェクト選択
        {
            TrySelectObject();
        }

        if (selectedObject != null && Input.GetMouseButton(0)) //選択したオブジェクトの移動
        {
            TryMoveSelectedObject();
        }

        Vector3 pos1 = transform.position;
        if (pos1.y < 0.5f)
        {
            pos1.y = 0.5f;
            transform.position = pos1;

        }
        //Vector3 pos2 = transform.position;
        //if (pos2.y > 0.5f)
        //{
        //    pos2.y = 0.5f;
        //    transform.position = pos2;
        //}
    }

    void FixedUpdate()
    {
        Vector3 gravity = Physics.gravity * gravityScale;
        _rb.AddForce(gravity, ForceMode.VelocityChange);
    }

    void TrySelectObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); //Raycastを飛ばしてオブジェクトを選択
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, selectableLayer))
        {
            if (selectedObject != null)
            {
                ResetHighlight();
            }

            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("change color");
                selectedObject = hit.collider.gameObject;
                Renderer rend = selectedObject.GetComponent<Renderer>();
                if (rend != null)
                {
                    //originalMaterial = rend.material;
                    //rend.material.color = new Color(_colorCode.r, _colorCode.g, _colorCode.b, _colorCode.a);
                    //rend.material = highlightMaterial; //ハイライト処理も可能
                }
            }
        }
    }

    void ResetHighlight()
    {
        if (selectedObject != null)
        {
            Renderer rend = selectedObject.GetComponent<Renderer>();
            if (rend != null && originalMaterial != null)
            {
                //rend.material.color = originalMaterial.color;
            }
        }
    }

    void TryMoveSelectedObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            Vector3 targetGridPos = new Vector3(
                Mathf.Round(hit.point.x / gridSize) * gridSize,
                0,
                Mathf.Round(hit.point.z / gridSize) * gridSize
            ); //マウス位置にグリッドをスナップ

            // 地面の高さを取得
            if (Physics.Raycast(targetGridPos + Vector3.up * 5f, Vector3.down, out RaycastHit groundHit, 10f, groundLayer))
            {
                float heightDiff = groundHit.point.y - selectedObject.transform.position.y;

                if (heightDiff <= maxStepHeight) //段差対応
                {
                    Vector3 newCenter = groundHit.point + new Vector3(0, selectedObject.transform.localScale.y / 2f, 0); //マウス位置のオブジェクトの中心位置を計算

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

                    // 移動実行
                    Vector3 position = new Vector3(targetGridPos.x, groundHit.point.y, targetGridPos.z);
                    if (position.y < 0.5f)
                    {
                        position.y = 0.5f;
                    }
                    selectedObject.transform.position = position;

                }
                else
                {
                    Debug.Log("段差が高すぎて登れません");
                }
            }
        }
    }
}
