using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SaveOnMiniclick : MonoBehaviour
{
    void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            // 인스펙터에 등록된 기존 타이틀 이동 로직을 방해하지 않고, 
            // 세이브 기능만 클릭 이벤트에 '추가'로 등록합니다.
            btn.onClick.AddListener(SaveGameData);
        }
    }

    private void SaveGameData()
    {
        // 메모리에 정상적으로 살아있는 GameManager Instance를 찾아 세이브 실행
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SaveGame();
            Debug.Log("<color=green>[SaveSystem] 타이틀 복귀 전 데이터 저장 성공!</color>");
        }
        else
        {
            Debug.LogError("[SaveSystem] GameManager.Instance를 찾을 수 없어 저장에 실패했습니다.");
        }
    }
}