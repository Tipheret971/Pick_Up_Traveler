using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject tutorialPopup;
    public Button closeButton;

    [Header("Settings")]
    public string tutorialKey = "HasSeenCraneTutorial"; // PlayerPrefs 저장용 키

    public AudioClip button;

    void Start()
    {
        // 이전에 튜토리얼을 본 적이 있는지 확인
        if (PlayerPrefs.GetInt(tutorialKey, 0) == 0)
        {
            ShowTutorial();
        }
        else
        {
            tutorialPopup.SetActive(false);
        }

        // 버튼 리스너 연결
        closeButton.onClick.AddListener(CloseTutorial);
    }

    void ShowTutorial()
    {
        tutorialPopup.SetActive(true);

        // 튜토리얼 중에는 게임이 멈추도록 설정 (필요 시)
        // Time.timeScale = 0f;
    }

    public void CloseTutorial()
    {
        tutorialPopup.SetActive(false);

        // 다시는 안 뜨도록 저장
        PlayerPrefs.SetInt(tutorialKey, 1);
        PlayerPrefs.Save();

        // 게임 재개
        Time.timeScale = 1f;

        // 확인 버튼 효과음 (기존 사운드매니저 활용)
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(button);
        }
    }
}