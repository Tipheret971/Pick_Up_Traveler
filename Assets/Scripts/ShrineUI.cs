using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShrineUI : MonoBehaviour
{
    // VN 진행을 위한 내부 상태 정의
    private enum ShrineState { InitialText, Choice, ResultWait, FinalReturn }
    private ShrineState currentState = ShrineState.InitialText;

    [Header("VN References")]
    [SerializeField] private TextMeshProUGUI vnTextBox; // 스토리/대사가 출력될 메인 텍스트 박스
    [SerializeField] private GameObject choicePanel;      // '예/아니오' 버튼을 담을 패널
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    [Header("UI References")]
    [SerializeField] private Button enterShrineButton; // 참배/구경 선택 화면의 버튼 (참배 버튼)
    [SerializeField] private string mapSceneName = "Map";

    // VN 텍스트 내용
    private const string TEXT_INIT = "도심 한복판에 신사가 있다...";
    private const string TEXT_CHOICE = "참배하시겠습니까?";
    private const string TEXT_PRAYING = "......";
    private const string TEXT_DONE = "오늘의 참배를 끝냈다.";

    void Start()
    {
        // bgm 재생
        SoundManager.Instance.PlayBGM(SoundManager.Instance.shrineBGM, 0.3f);
        // 맵 복귀 버튼 리스너 연결
        // if (closeButton != null)
        // {
        //     closeButton.onClick.AddListener(ReturnToMap);
        // }
        // 
        // // 참배 버튼 리스너 연결
        // if (enterShrineButton != null)
        // {
        //     enterShrineButton.onClick.AddListener(OnPrayButtonClicked);
        // }
        // VN 버튼 리스너 연결
        if (yesButton != null)
            yesButton.onClick.AddListener(() => OnChoiceMade(true));
        if (noButton != null)
            noButton.onClick.AddListener(() => OnChoiceMade(false));

        // 초기에 선택 버튼들은 비활성화
        if (choicePanel != null)
            choicePanel.SetActive(false);

        // 텍스트 박스 클릭 감지 컴포넌트 추가 (VN 진행을 위해 필수)
        // 메인 텍스트 박스에 Collider나 EventTrigger를 추가하여 마우스 클릭을 감지해야 합니다.
        // 여기서는 간단하게 씬 내에서 마우스 왼쪽 클릭을 감지하는 로직을 사용합니다.

        // 초기 텍스트 출력
        UpdateVNState();
    }

    private void Update()
    {
        // 마우스 클릭 시 VN 상태를 진행 (Choice 상태일 때는 클릭 무시)
        if (Input.GetMouseButtonDown(0) && currentState != ShrineState.Choice)
        {
            AdvanceVNState();
        }
    }

    // 맵 복귀 버튼에 연결할 함수
    public void ReturnToMap()
    {
        // SceneLoader를 찾아 맵 씬으로 복귀
        SceneLoader sceneLoader = UnityEngine.Object.FindFirstObjectByType<SceneLoader>();
        if (sceneLoader != null)
        {
            sceneLoader.ReturnToMapScene(mapSceneName);
        }
    }

    // VN 상태를 다음 단계로 진행하는 함수
    private void AdvanceVNState()
    {
        if (GameManager.Instance == null) return;

        switch (currentState)
        {
            case ShrineState.InitialText:
                // 1단계: '참배하시겠습니까?' 질문으로 전환
                currentState = ShrineState.Choice;
                UpdateVNState();
                break;

            case ShrineState.ResultWait:
                // 3단계: 참배 결과 대기 후 문구 출력 및 최종 복귀 상태로 전환
                currentState = ShrineState.FinalReturn;
                UpdateVNState();
                break;

            case ShrineState.FinalReturn:
                // 4단계: '오늘의 참배를 끝냈다' 문구 출력 후 맵 복귀
                ReturnToMap();
                break;
        }
    }

    private void UpdateVNState()
    {
        if (vnTextBox == null || choicePanel == null) return;

        // 모든 UI 초기 상태 설정
        choicePanel.SetActive(false);

        switch (currentState)
        {
            case ShrineState.InitialText:
                vnTextBox.text = TEXT_INIT;
                break;

            case ShrineState.Choice:
                // 2단계: 질문 출력 및 버튼 활성화
                if (GameManager.Instance.hasPrayedToday)
                {
                    // 이미 참배했으면 바로 최종 복귀 텍스트로
                    vnTextBox.text = TEXT_DONE;
                    currentState = ShrineState.FinalReturn;
                }
                else
                {
                    vnTextBox.text = TEXT_CHOICE;
                    choicePanel.SetActive(true);
                }
                break;

            case ShrineState.ResultWait:
                // 참배 진행 중 문구 출력
                vnTextBox.text = TEXT_PRAYING;
                break;

            case ShrineState.FinalReturn:
                // 참배 끝 문구 출력
                vnTextBox.text = TEXT_DONE;
                break;
        }
    }

    private void OnChoiceMade(bool pray)
    {
        // 참배했으므로 버튼 비활성화
        if (choicePanel != null)
            choicePanel.SetActive(false);

        if (pray)
        {
            // '예' 선택 시:
            // 1. GameManager 참배 로직 호출 (돈 차감 및 버프 획득)
            GameManager.Instance.PrayAtShrine();

            // 2. 텍스트를 '.....'로 설정 (버프 획득 여부 알리지 않음)
            vnTextBox.text = TEXT_PRAYING;

            // 3. 다음 상태는 ResultWait으로 전환 (클릭 대기 상태)
            currentState = ShrineState.ResultWait;

            // UI가 바로 맵으로 복귀하는 것을 막고 클릭을 기다립니다.
        }
        else
        {
            // '아니오' 선택 시: 바로 맵 복귀
            ReturnToMap();
        }
    }
}