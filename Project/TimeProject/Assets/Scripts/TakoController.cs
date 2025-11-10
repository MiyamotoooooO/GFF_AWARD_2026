using UnityEngine;

public class OperatorFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float followDistance = 1f;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;  // ← 追加！

    private void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>(); // ← 追加！
        }
    }

    private void Update()
    {
        if (target == null) return;

        Vector3 direction = target.position - transform.position;
        float distance = direction.magnitude;

        if (distance > followDistance)
        {
            Vector3 moveDir = direction.normalized;

            // ★ flipXで左右を反転（Z軸じゃなくて視覚的に向きを変える）
            if (spriteRenderer != null)
            {
                if (moveDir.x > 0) spriteRenderer.flipX = false;
                else if (moveDir.x < 0) spriteRenderer.flipX = true;
            }

            // 移動
            transform.position += moveDir * moveSpeed * Time.deltaTime;

            // アニメーション切り替え
            if (animator != null)
            {
                animator.SetBool("isMoving", true);
            }
        }
        else
        {
            if (animator != null)
            {
                animator.SetBool("isMoving", false);
            }
        }
    }
}
