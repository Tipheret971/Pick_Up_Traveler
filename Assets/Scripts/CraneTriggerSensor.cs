using UnityEngine;

public class CraneTriggerSensor : MonoBehaviour
{
    private CraneController craneController;

    void Start()
    {
        // 부모 오브젝트에서 CraneController 컴포넌트를 찾습니다.
        craneController = GetComponentInParent<CraneController>();
        // if (craneController == null)
        // {
        //     Debug.LogError("ERROR: CraneController 스크립트를 부모에서 찾을 수 없습니다.");
        // }

        // Rigidbody가 없으면 충돌 감지가 작동하지 않을 수 있으므로 경고합니다.
        if (GetComponent<Collider>() != null && GetComponent<Rigidbody>() == null)
        {
            Debug.LogWarning("CraneTriggerSensor에 Rigidbody가 없습니다. Is Trigger 설정이 되어 있더라도 Rigidbody가 있어야 제대로 작동합니다.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 충돌한 오브젝트의 태그가 "Goods"인지 확인합니다.
        if (other.CompareTag("Goods"))
        {
            // 부모 CraneController에 굿즈에 닿았음을 알립니다.
            craneController.NotifyHitGoods();
        }
    }
}