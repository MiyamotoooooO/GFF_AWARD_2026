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
    public DirectionType directionType = DirectionType.Forward;
    public Vector3 customDirection = Vector3.forward;
    public float windForce = 10f;
    public float windRange = 5f;
    public float windWidth = 3f;
    public float windHeight = 2f;
    public float windDuration = 3f;

    [Header("再使用設定")]
    public bool singleUse = true;
    public float cooldown = 2f;

    [Header("風のエフェクトプレハブ（ParticleSystem入り）")]
    public GameObject windEffectPrefab;

    [Header("生成したエフェクトの大きさ")]
    public Vector3 effectScaleMultiplier = Vector3.one;

    // 内部変数
    private GameObject windEffectObj;
    private ParticleSystem windEffect;
    private BoxCollider effectTriggerBox;

    private bool canActivate = true;
    private bool windActive = false;
    private Rigidbody lastRbInside;

    void Start()
    {
        if (windEffectPrefab != null)
        {
            windEffectObj = Instantiate(windEffectPrefab, transform);
            windEffectObj.name = "WindEffect";

            windEffect = windEffectObj.GetComponentInChildren<ParticleSystem>();
            if (windEffect == null)
            {
                Debug.LogError("windEffectPrefab に ParticleSystem が入っていません。");
            }
        }

        effectTriggerBox = new GameObject("WindEffectTrigger").AddComponent<BoxCollider>();
        effectTriggerBox.isTrigger = true;
        effectTriggerBox.transform.SetParent(transform);
    }

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

        AdjustWindEffectTransform();
        if (windEffect != null) windEffect.Play();

        float timer = 0f;
        while (timer < windDuration)
        {
            BlowWind();
            timer += Time.deltaTime;
            yield return null;
        }

        windActive = false;
        if (windEffect != null) windEffect.Stop();
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
        Vector3 center = transform.position + dir * (windRange / 2f);

        Collider[] hit = Physics.OverlapBox(
            center,
            new Vector3(windWidth / 2, windHeight / 2, windRange / 2),
            Quaternion.LookRotation(dir)
        );

        bool playerInside = false;

        foreach (Collider col in hit)
        {
            Rigidbody rb = col.attachedRigidbody;

            if (rb != null)
            {
                rb.AddForce(dir * windForce * Time.deltaTime, ForceMode.VelocityChange);

                if (col.CompareTag(playerTag))
                {
                    lastRbInside = rb;
                    playerInside = true;
                }
            }
        }

        if (!playerInside && lastRbInside != null)
        {
            StopPlayerWindVelocity(dir);
        }
    }

    void StopPlayerWindVelocity(Vector3 dir)
    {
        if (lastRbInside == null) return;

        Vector3 v = lastRbInside.velocity;
        float alongWind = Vector3.Dot(v, dir);

        float ease = 5f;

        float newAlong = Mathf.Lerp(alongWind, 0, Time.deltaTime * ease);

        lastRbInside.velocity = v + dir * (newAlong - alongWind);

        if (Mathf.Abs(newAlong) < 0.01f)
        {
            lastRbInside = null;
        }
    }

    Vector3 GetWindDirection()
    {
        switch (directionType)
        {
            case DirectionType.Forward: return transform.forward;
            case DirectionType.Backward: return -transform.forward;
            case DirectionType.Left: return -transform.right;
            case DirectionType.Right: return transform.right;
            case DirectionType.Up: return Vector3.up;
            case DirectionType.Down: return Vector3.down;
            case DirectionType.Custom: return customDirection.normalized;
            default: return transform.forward;
        }
    }

    void AdjustWindEffectTransform()
    {
        if (windEffectObj == null || windEffect == null) return;

        Vector3 dir = GetWindDirection().normalized;
        Vector3 center = transform.position + dir * (windRange / 2);

        windEffectObj.transform.position = center;
        windEffectObj.transform.rotation = Quaternion.LookRotation(dir);
        windEffectObj.transform.localScale = effectScaleMultiplier;

        var shape = windEffect.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(windWidth, windHeight, windRange);

        effectTriggerBox.transform.position = center;
        effectTriggerBox.transform.rotation = Quaternion.LookRotation(dir);
        effectTriggerBox.size = new Vector3(windWidth, windHeight, windRange);

        var trigger = windEffect.trigger;
        trigger.enabled = true;
        trigger.SetCollider(0, effectTriggerBox);
        trigger.inside = ParticleSystemOverlapAction.Ignore;
        trigger.outside = ParticleSystemOverlapAction.Kill;
        trigger.exit = ParticleSystemOverlapAction.Kill;
    }

    void OnDrawGizmosSelected()
    {
        Vector3 dir = GetWindDirection().normalized;
        Vector3 center = transform.position + dir * (windRange / 2f);

        Gizmos.color = Color.cyan;
        Gizmos.matrix = Matrix4x4.TRS(center, Quaternion.LookRotation(dir), Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(windWidth, windHeight, windRange));

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + dir * windRange);
    }
}


