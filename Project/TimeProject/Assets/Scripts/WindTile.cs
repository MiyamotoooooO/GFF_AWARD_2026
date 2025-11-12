using UnityEngine;
using System.Collections;

public class WindTile : MonoBehaviour
{
    public enum DirectionType
    {
        Forward,
        Backward,
        Left,
        Right,
        Up,
        Down,
        Custom
    }

    [Header("プレイヤーのタグ")]
    public string playerTag = "Player";

    [Header("風の設定")]
    public DirectionType directionType = DirectionType.Forward; // Inspectorで方向を選択
    public Vector3 customDirection = Vector3.forward;            // Custom選択時のみ使用
    public float windForce = 10f;                                // 風の強さ
    public float windRange = 5f;                                 // 風の届く距離
    public float windWidth = 3f;                                 // 風の横幅
    public float windHeight = 2f;                                // 高さ
    public float windDuration = 3f;                              // 風の持続時間

    [Header("再使用設定")]
    public bool singleUse = true;                                // 一度きりかどうか
    public float cooldown = 2f;                                  // 再使用までの時間

    bool canActivate = true;
    bool windActive = false;

    // isTriggerがOFFなのでOnTriggerEnterは使わず、OnCollisionEnterで判定
    private void OnCollisionEnter(Collision collision)
    {
        if (!canActivate || windActive) return;
        if (collision.collider.CompareTag(playerTag))
        {
            StartCoroutine(WindRoutine());
            if (singleUse)
                canActivate = false;
            else
                StartCoroutine(CooldownRoutine());
        }
    }

    IEnumerator WindRoutine()
    {
        windActive = true;
        Debug.Log(" 強風発生！");

        float timer = 0f;
        while (timer < windDuration)
        {
            BlowWind();
            timer += Time.deltaTime;
            yield return null;
        }

        windActive = false;
        Debug.Log(" 風が止んだ");
    }

    IEnumerator CooldownRoutine()
    {
        canActivate = false;
        yield return new WaitForSeconds(cooldown);
        canActivate = true;
    }

    void BlowWind()
    {
        Vector3 dir = GetWindDirection().normalized;
        Vector3 center = transform.position + dir * (windRange / 2);

        Collider[] hit = Physics.OverlapBox(
            center,
            new Vector3(windWidth / 2, windHeight / 2, windRange / 2),
            Quaternion.LookRotation(dir)
        );

        foreach (Collider col in hit)
        {
            Rigidbody rb = col.attachedRigidbody;
            if (rb != null)
            {
                rb.AddForce(dir * windForce * Time.deltaTime, ForceMode.VelocityChange);
            }
        }
    }

    Vector3 GetWindDirection()
    {
        switch (directionType)
        {
            case DirectionType.Forward:
                return transform.forward;
            case DirectionType.Backward:
                return -transform.forward;
            case DirectionType.Left:
                return -transform.right;
            case DirectionType.Right:
                return transform.right;
            case DirectionType.Up:
                return Vector3.up;
            case DirectionType.Down:
                return Vector3.down;
            case DirectionType.Custom:
                return customDirection.normalized;
            default:
                return transform.forward;
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector3 dir = GetWindDirection().normalized;
        Vector3 center = transform.position + dir * (windRange / 2);

        Gizmos.color = Color.cyan;
        Gizmos.matrix = Matrix4x4.TRS(center, Quaternion.LookRotation(dir), Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(windWidth, windHeight, windRange));

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + dir * (windRange * 0.6f));
        Gizmos.DrawSphere(transform.position + dir * (windRange * 0.6f), 0.2f);
    }
}

