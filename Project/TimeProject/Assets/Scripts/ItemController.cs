using UnityEngine;

public class ItemController : MonoBehaviour
{
    public LayerMask selectableLayer;
    public LayerMask groundLayer; // ← ドラッグ中の移動判定用
    public LayerMask craftLayer;  // ← 設置判定用
    public float DOWN = 1.25f;
    public float setti = 0f;

    private GameObject selectedObject;
    private float objectZ;
    private Vector3 offset;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) TrySelectObject();
        if (selectedObject != null && Input.GetMouseButton(0)) TryMoveSelectedObject();
        if (Input.GetMouseButtonUp(0)) ConfirmPlacement();

        Vector3 pos1 = transform.position;
        if (pos1.y < DOWN)
        {
            pos1.y = DOWN;
            transform.position = pos1;
        }
    }

    void TrySelectObject()
    {
        if (selectedObject != null) return;

        Vector3 mousePos = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, selectableLayer))
        {
            selectedObject = hit.collider.gameObject;
            objectZ = Camera.main.WorldToScreenPoint(selectedObject.transform.position).z;
            offset = selectedObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, objectZ));
        }
    }

    void TryMoveSelectedObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            Vector3 targetPos = hit.point + offset;
            selectedObject.transform.position = targetPos;
        }

        if (Physics.Raycast(ray, out RaycastHit crafthit, 100f, craftLayer))
        {
            // スケールに依存しない高さで配置
            Vector3 aboveBlock = crafthit.point + new Vector3(0, setti, 0);
            selectedObject.transform.position = aboveBlock;
        }
    }

    //void TryMoveSelectedObject()
    //{
    //    if (selectedObject == null) return;

    //    Rigidbody rb = selectedObject.GetComponent<Rigidbody>();
    //    if (rb == null) return;

    //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //    if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
    //    {
    //        Vector3 targetPos = hit.point + offset;
    //        Vector3 moveDirection = (targetPos - selectedObject.transform.position).normalized;
    //        float moveSpeed = 100f; // 移動速度（調整可能）

    //        rb.velocity = moveDirection * moveSpeed;
    //    }

    //    if (Physics.Raycast(ray, out RaycastHit crafthit, 100f, craftLayer))
    //    {
    //        Vector3 aboveBlock = crafthit.point + new Vector3(0, setti, 0);
    //        Vector3 moveDirection = (aboveBlock - selectedObject.transform.position).normalized;
    //        float moveSpeed = 100f;

    //        rb.velocity = moveDirection * moveSpeed;
    //    }
    //}

    //void TryMoveSelectedObject()
    //{
    //    if (selectedObject == null) return;

    //    Rigidbody rb = selectedObject.GetComponent<Rigidbody>();
    //    if (rb == null) return;

    //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

    //    // groundLayer に Raycast
    //    if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
    //    {
    //        Vector3 targetPos = hit.point + offset;
    //        Vector3 forceDirection = (targetPos - selectedObject.transform.position).normalized;
    //        float forceStrength = 100f; // 力の強さ（調整可能）
    //        //selectedObject.transform.position = targetPos;
    //        rb.AddForce(forceDirection * forceStrength, ForceMode.Force);
    //    }

    //    // craftLayer に Raycast（設置候補）
    //    if (Physics.Raycast(ray, out RaycastHit crafthit, 100f, craftLayer))
    //    {
    //        Vector3 aboveBlock = crafthit.point + new Vector3(0, setti, 0);
    //        Vector3 forceDirection = (aboveBlock - selectedObject.transform.position).normalized;
    //        float forceStrength = 100f;

    //        selectedObject.transform.position = aboveBlock;
    //        rb.AddForce(forceDirection * forceStrength, ForceMode.Force);
    //    }
    //}

    void ConfirmPlacement()
    {
        if (selectedObject != null)
        {
            //Rigidbody rb = selectedObject.GetComponent<Rigidbody>();
            //if (rb != null)
            //{
            //    rb.velocity = Vector3.zero; // 移動停止
            //    rb.velocity = Vector3.zero;
            //    rb.angularVelocity = Vector3.zero;
            //}

            selectedObject.tag = "after";
            selectedObject = null;
        }
    }
}
