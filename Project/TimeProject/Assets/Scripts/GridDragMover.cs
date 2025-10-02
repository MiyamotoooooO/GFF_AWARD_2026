using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]

public class GridDragMover : MonoBehaviour
{
    static public float gridSize = 1f;     //グリッド幅
    public float gravityScale = 8f; //重力倍率（1 = 通常重力）
    private bool isDragging = false;//ドラッグしてるかどうか

    private Camera mainCamera;
    private Rigidbody _rb;
    private Vector3 lastGridPosition;
  
    void Start()
    {
        mainCamera = Camera.main;
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
    }

    private void Update()
    {
        Vector3 pos = transform.position;
        if (pos.y < 0.5f)
        {
            pos.y = 0.5f;
            transform.position = pos;
        }
        //transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }

    private void FixedUpdate()
    {
        Vector3 gravity = Physics.gravity * gravityScale;
        _rb.AddForce(gravity, ForceMode.Acceleration);
    }

    void OnMouseDown()
    {
        isDragging = true;
        lastGridPosition = GetSnappedPosition();
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector3 currentGridPosition = GetSnappedPosition();

        // グリッド位置が変わったら移動
        if (currentGridPosition != lastGridPosition)
        {
            transform.position = new Vector3(currentGridPosition.x, transform.position.y, currentGridPosition.z);
            lastGridPosition = currentGridPosition;


        }
    }

    void OnMouseUp()
    {
        isDragging = false;
    }

    Vector3 GetSnappedPosition()
    {

        if (isDragging && Input.GetMouseButton(0))  //ドラッグ且つ右クリックしている時にグリッド状に動かす
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane ground = new Plane(Vector3.up, Vector3.zero); // XZ平面（y=0）

            if (ground.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);

                float x = Mathf.Round(hitPoint.x / gridSize) * gridSize;
                float z = Mathf.Round(hitPoint.z / gridSize) * gridSize;

                // Yは固定（高さを変えない）
                transform.position = new Vector3(x, -2.001647f, z);
                return new Vector3(x, -2.001647f, z);
            }
        }
        return transform.position;

        /*
        // マウス位置をワールド座標に変換
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(mainCamera.transform.position.y);// - transform.position.y); // 高さを考慮
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);

        // X/Zをグリッドにスナップ
        float snappedX = Mathf.Round(worldPos.x / gridSize) * gridSize;
        float snappedZ = Mathf.Round(worldPos.z / gridSize) * gridSize;

        return new Vector3(snappedX, transform.position.y, snappedZ);
        */
    }
}