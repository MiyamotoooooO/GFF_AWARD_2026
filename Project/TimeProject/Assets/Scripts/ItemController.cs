using UnityEngine;
using System.Collections.Generic;

public class ItemController : MonoBehaviour
{
    [Header("Layers")]
    [SerializeField] private LayerMask itemLayer;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask craftLayer;

    [Header("Settings")]
    [SerializeField] private float DOWN = 1.25f;
    [SerializeField] private float setti = 0f;

    private GameObject selectedObject;
    private Vector3 offset;
    private float objectZ;

    [Header("ä«óùëŒè€")]
    [SerializeField] private List<GameObject> managedObjects = new List<GameObject>();

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) TrySelectObject();
        if (selectedObject != null && Input.GetMouseButton(0)) TryMoveSelectedObject();
        if (Input.GetMouseButtonUp(0)) ConfirmPlacement();
    }

    void TrySelectObject()
    {
        if (selectedObject != null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, itemLayer))
        {
            selectedObject = hit.collider.gameObject;
            objectZ = Camera.main.WorldToScreenPoint(selectedObject.transform.position).z;
            offset = selectedObject.transform.position -
                     Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, objectZ));
        }
    }

    void TryMoveSelectedObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            Vector3 targetPos = hit.point + offset;
            targetPos.y = Mathf.Max(targetPos.y, DOWN);
            selectedObject.transform.position = targetPos;
        }

        if (Physics.Raycast(ray, out RaycastHit craftHit, 100f, craftLayer))
        {
            Vector3 aboveBlock = craftHit.point + new Vector3(0, setti, 0);
            selectedObject.transform.position = aboveBlock;
        }
    }

    void ConfirmPlacement()
    {
        if (selectedObject != null)
        {
            selectedObject.tag = "after";
            selectedObject = null;
        }
    }
}
