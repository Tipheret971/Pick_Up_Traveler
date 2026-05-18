using UnityEngine;

// Rigidbody와 Collider가 있어야 물리적으로 작동합니다.
[RequireComponent(typeof(Rigidbody))]
public class PrizeItem : MonoBehaviour
{
    // 이 오브젝트가 가진 굿즈의 속성 데이터 (GameManager에서 할당됨)
    [Header("Goods Attributes")]
    [SerializeField]
    private GoodsData itemData;

    // Rigidbody가 이 스크립트와 함께 부착되도록 강제합니다.
    private Rigidbody rb;
    // 추가: 굿즈가 이미 획득 처리 중인지 확인하는 플래그
    private bool isBeingAcquired = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // 필요 시 여기서 Rigidbody의 Mass나 Drag(마찰력)를 조절할 수 있습니다.
        // 예: rb.mass = (int)itemData.Rarity + 1; 
    }

    // 외부에서 획득 처리 상태를 확인하고 설정하는 함수
    public bool TryStartAcquisition()
    {
        // 획득 처리 중이 아니라면 (false)
        if (!isBeingAcquired)
        {
            isBeingAcquired = true; // 즉시 true로 설정하여 다음 호출을 막음
            return true; // 획득 처리를 시작해도 좋음
        }
        return false; // 이미 획득 처리 중이므로 무시
    }

    // 외부에서 굿즈 데이터를 가져갈 수 있도록 Public 메서드를 제공합니다.
    public GoodsData GetItemData()
    {
        return itemData;
    }

    // GameManager가 굿즈를 할당할 때 호출할 함수 (현재는 테스트용)
    public void SetItemData(GoodsData data)
    {
        itemData = data;
        // 굿즈의 시각적 요소를 업데이트하는 함수를 여기서 호출합니다.
        // UpdateVisuals(); 
    }

    // 나중에 시각적 요소를 업데이트할 때 사용할 함수 (지금은 내용 없음)
    // private void UpdateVisuals()
    // {
    //    // TODO: 굿즈 오브젝트의 이미지/테두리를 itemData에 맞게 변경하는 로직
    // }
}