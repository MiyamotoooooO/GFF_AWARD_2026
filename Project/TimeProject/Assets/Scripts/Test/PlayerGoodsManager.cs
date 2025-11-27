using UnityEngine;
using UnityEngine.UI;

public class PlayerGoodsManager : MonoBehaviour
{
    [Header("現在の所持グッズ")]
    public GoodsType currentGoods = GoodsType.None;

    [Header("UI表示用のImage")]
    public Image goodsUIImage;

    [Header("グッズごとのスプライト")]
    public Sprite noneSprite;
    public Sprite speedShoesSprite;
    public Sprite axeSprite;
    public Sprite magicArmSprite;

    [Header("ビジュアル用オブジェクト（例: 靴, アームなど）")]
    public GameObject speedVisual;
    public GameObject axeVisual;
    public GameObject armVisual;

    void Start()
    {
        UpdateGoodsVisual();
    }

    private void OnTriggerEnter(Collider other)
    {
        GoodsItem item = other.GetComponent<GoodsItem>();
        if (item != null)
        {
            PickupGoods(item.goodsType);
            Destroy(other.gameObject);
        }
    }

    void PickupGoods(GoodsType newGoods)
    {
        currentGoods = newGoods;
        UpdateGoodsVisual();
    }

    void UpdateGoodsVisual()
    {
        // UI更新
        switch (currentGoods)
        {
            case GoodsType.SpeedShoes:
                goodsUIImage.sprite = speedShoesSprite;
                break;
            case GoodsType.Axe:
                goodsUIImage.sprite = axeSprite;
                break;
            case GoodsType.MagicArm:
                goodsUIImage.sprite = magicArmSprite;
                break;
            default:
                goodsUIImage.sprite = noneSprite;
                break;
        }

        // プレイヤー見た目更新
        if (speedVisual != null) speedVisual.SetActive(currentGoods == GoodsType.SpeedShoes);
        if (axeVisual != null) axeVisual.SetActive(currentGoods == GoodsType.Axe);
        if (armVisual != null) armVisual.SetActive(currentGoods == GoodsType.MagicArm);
    }

    // 所持しているグッズの種類を外部から取得したい場合
    public GoodsType GetCurrentGoods()
    {
        return currentGoods;
    }
}
