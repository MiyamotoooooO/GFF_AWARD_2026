using UnityEngine;

public class DamageZone : MonoBehaviour
{
    // プレイヤーに与えるダメージ量(酸素の減る量)
    [SerializeField] private int damageAmount = 10;
    private AudioSource damageAudio;
    private void Start()
    {
        damageAudio = GetComponent<AudioSource>();
    }
    // 衝突したときに呼ばれる
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("衝突が発生しました！相手: " + collision.gameObject.name);
        if (collision.gameObject.CompareTag("Player"))
        {
            if (OxygenGaugeController.Instance != null)
            {
                OxygenGaugeController.Instance.TakeDamage(damageAmount);

                if (damageAudio != null)
                {
                    damageAudio.Play();
                }
            }

            else
            {
                Debug.LogError("OxygenGaugeControllerのInstanceが見つかりません。UIにスプリクトがアタッチされていますか？");
            }

        }
    }
    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (damageAudio != null)
            {
                damageAudio.Stop();
            }
        }
    }
}


