using UnityEngine;
using UnityEngine.SceneManagement;

public class PrizeExitController : MonoBehaviour
{
    [SerializeField] private string prizeTag = "Goods";
    public AudioClip clap;

    private void OnTriggerEnter(Collider other)
    {
        // 1. 들어온 오브젝트의 태그가 상품 태그와 일치하는지 확인
        if (other.CompareTag(prizeTag))
        {
            HandlePrizeAcquisition(other.gameObject);
        }
    }

    // 상품 획득 시 실제로 진행될 게임 로직
    private void HandlePrizeAcquisition(GameObject acquiredPrize)
    {
        // 1. PrizeItem 스크립트에서 굿즈 데이터를 가져오기
        PrizeItem prizeItem = acquiredPrize.GetComponent<PrizeItem>();

        if (prizeItem == null)
        {
            Debug.LogError("획득된 굿즈에 PrizeItem 스크립트가 없습니다. 획득 실패.");
            return;
        }

        // 핵심 수정: PrizeItem에게 획득 처리를 시작해도 되는지 묻습니다.
        if (!prizeItem.TryStartAcquisition())
        {
            // 이미 획득 처리 중이라면 함수를 즉시 종료하고 중복 실행 방지
            return;
        }

        // 2. GameManager 연동 및 클리어 기록 (핵심)
        if (GameManager.Instance != null)
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            GoodsData acquiredData = prizeItem.GetItemData();

            // 굿즈를 인벤토리에 추가 및 씬 클리어 기록
            GameManager.Instance.AddGoodsToInventory(acquiredData);
            GameManager.Instance.SetMachineCleared(currentSceneName);

            Debug.Log($"🎉 획득! [{acquiredData.Series} / {acquiredData.Rarity}] 씬 클리어됨.");
            SoundManager.Instance.PlaySFX(clap, 0.8f);

            // 추가: 씬 클리어 후 Reset 버튼을 찾아 즉시 비활성화
            // 버튼의 이름이 'ResetButton'이라고 가정합니다.
            // 핵심 수정: 씬 클리어 후 Reset 버튼을 찾아 상태를 즉시 갱신
            GameObject resetButtonObject = GameObject.Find("ResetButton");

            if (resetButtonObject != null)
            {
                // ResetButton에 부착된 CraneSceneButtonManager 컴포넌트를 찾습니다.
                CraneSceneButtonManager buttonManager = resetButtonObject.GetComponent<CraneSceneButtonManager>();

                if (buttonManager != null)
                {
                    // 즉시 버튼의 접근성(interactable)을 갱신합니다.
                    buttonManager.UpdateAccessibility();
                    Debug.Log("Reset 버튼 상태를 즉시 갱신했습니다.");
                }
            }
        }
        else
        {
            Debug.LogError("GameManager 인스턴스를 찾을 수 없습니다.");
        }

        // 3. 획득된 상품 오브젝트 제거
        Destroy(acquiredPrize);

        // 4. (제외) 맵으로 자동 복귀 로직은 없습니다. 플레이어는 씬에 머뭅니다.
    }
}