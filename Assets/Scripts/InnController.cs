using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class InnController : MonoBehaviour
{
    // Map 씬 이름과 Game Over 씬 이름을 인스펙터에서 설정할 수 있도록 추가
    [SerializeField] private string mapSceneName = "Map";
    [SerializeField] private string gameOverSceneName = "EndingScene"; // Game Over 씬 이름 추가

    public void RestAndAdvanceDay()
    {
        if (GameManager.Instance == null) return;

        // 버튼 중복 클릭 방지
        Button button = GetComponent<Button>();
        if (button != null) button.interactable = false;

        // 1. 날짜 진행 체크
        bool dayAdvancedSuccessfully = GameManager.Instance.AdvanceDay();

        // [핵심 추가] 오늘이 여행 이벤트 날인지 체크
        // GameManager에 현재 날짜가 여행 이벤트 날인지 알려주는 변수나 로직이 필요합니다.
        int[] travelDays = { 4, 8, 12, 16, 20, 24, 28 };
        bool isTravelDay = System.Array.Exists(travelDays, day => day == GameManager.Instance.gameDay);

        if (dayAdvancedSuccessfully && isTravelDay)
        {
            // 여행 이벤트 날이라면 InnController에서 별도로 씬을 로드하지 않습니다.
            // 왜냐하면 GameManager.AdvanceDay() 내부에서 이미 SceneLoader를 호출했기 때문입니다.
            Debug.Log("여행 이벤트 날이므로 InnController의 추가 씬 로드를 중단합니다.");
            return;
        }

        // --- 이하 일반적인 날이나 게임 오버일 때의 로직 ---
        FadeController fc = UnityEngine.Object.FindFirstObjectByType<FadeController>();
        string targetScene = dayAdvancedSuccessfully ? mapSceneName : gameOverSceneName;

        System.Action loadAction = () => { SceneManager.LoadScene(targetScene); };

        if (fc != null) fc.FadeInAndExecute(loadAction);
        else loadAction.Invoke();
    }
}