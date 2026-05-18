using UnityEngine;
using System.Linq; // Linq 사용을 위해 추가 (씬 이름 검색)

public class PrizeSceneInitializer : MonoBehaviour
{
    void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager가 로드되지 않았습니다. 굿즈 초기화를 중단합니다.");
            return;
        }

        // 1. 현재 씬 이름 파악 (예: "Crane01")
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // 2. GameManager에서 현재 씬에 해당하는 굿즈 데이터를 찾음
        PrizeMachineState state = GameManager.Instance.currentMachinePrizes
            .FirstOrDefault(s => s.SceneName == currentSceneName);

        // 3. 씬 내의 PrizeItem 오브젝트를 찾음
        PrizeItem prizeItem = FindFirstObjectByType<PrizeItem>(); 
        
        if (prizeItem == null)
        {
            Debug.LogError($"씬 [{currentSceneName}]에서 PrizeItem 오브젝트를 찾을 수 없습니다. 굿즈를 배치해 주세요.");
            return;
        }

        // 4. 굿즈 데이터 할당
        if (state != null)
        {
            prizeItem.SetItemData(state.CurrentGoods);
            Debug.Log($"[{currentSceneName}] PrizeItem 초기화 완료: {state.CurrentGoods.Series} / {state.CurrentGoods.Rarity}");
            
            // TODO: (다음 단계) 여기서 굿즈의 시각적 요소를 업데이트하는 함수를 호출해야 합니다.
            // prizeItem.UpdateVisuals();
        }
        else
        {
            Debug.LogError($"GameManager에 씬 [{currentSceneName}]에 대한 굿즈 데이터가 없습니다.");
        }
        // PrizeSceneInitializer.cs의 Start() 하단에 추가
        CranePrizeDisplayUI displayUI = FindFirstObjectByType<CranePrizeDisplayUI>();
        if (displayUI != null)
        {
            displayUI.RefreshDisplay();
        }
    }
}