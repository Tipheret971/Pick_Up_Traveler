using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class TravelVNManager : MonoBehaviour
{
    [Header("Data Source")]
    public TravelEventData eventData;

    [Header("UI References")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public Image heroineStanding;
    public GameObject blackOverlay;
    public Image cgImage;
    public GameObject choicePanel;
    public Button normalRouteBtn;
    public Button specialRouteBtn;
    public Image backgroundDisplay;
    public Button cancelBtn; // 추가: 여행 취소(휴식) 버튼

    private List<DialogueSection> activeDialogueList;
    private int currentIndex = 0;
    private bool isDialogueActive = false;

    private int currentNormalCost;
    private int currentSpecialCost;

    [Header("Branch UI")]
    public GameObject branchPanel;     // 24일차 전용 선택지 패널
    public TextMeshProUGUI branchAText;
    public TextMeshProUGUI branchBText;

    [Header("Common SFX")]
    public AudioClip clickSFX; // 대사 넘길 때 기본 클릭음

    void Start()
    {
        // 1. 현재 날짜 가져오기
        int currentDay = GameManager.Instance.gameDay;

        // 2. 날짜에 맞는 데이터 경로 설정
        string dataPath = GetTravelDataPath(currentDay);

        // 3. 리소스 폴더에서 데이터 로드
        eventData = Resources.Load<TravelEventData>(dataPath);

        if (eventData != null)
        {
            Debug.Log($"{currentDay}일차 데이터를 로드했습니다: {dataPath}");
            SoundManager.Instance.PlayBGM(SoundManager.Instance.travelBGM, 0.5f);
            CalculateCosts();
            ShowTravelChoice();
        }
        else
        {
            Debug.LogError($"데이터를 찾을 수 없습니다! 경로를 확인하세요: {dataPath}");
        }
    }

    // 날짜와 선택 결과에 따라 파일 경로를 반환하는 함수
    private string GetTravelDataPath(int day)
    {
        // 28일차는 24일차 선택 결과(day24Choice)를 확인
        if (day == 28)
        {
            if (GameManager.Instance.day24Choice == 1) // 신사 방문 시
                return "TravelData/TravelDay_28_True";
            else // 신사 미방문 시
                return "TravelData/TravelDay_28_Normal";
        }

        // 그 외의 날짜는 규칙대로 반환
        return $"TravelData/TravelDay_{day}";
    }

    private void CalculateCosts()
    {
        currentNormalCost = eventData.transportCost;
        currentSpecialCost = eventData.transportCost + eventData.specialOptionCost;

        if (GameManager.Instance.hasHormoneBuff)
        {
            currentNormalCost /= 2;
            currentSpecialCost /= 2;
            Debug.Log($"호르몬동 버프 감지: 할인된 비용 적용 (일반:{currentNormalCost}, 데이트:{currentSpecialCost})");
        }
    }

    // --- 1. 선택지 관리 ---
    private void ShowTravelChoice()
    {
        choicePanel.SetActive(true);

        string buffText = GameManager.Instance.hasHormoneBuff ? "\n<color=yellow>(버프 할인 50%)</color>" : "";

        // 버튼 텍스트 설정
        normalRouteBtn.GetComponentInChildren<TextMeshProUGUI>().text =
            $"일반 여행\n(¥{currentNormalCost:N0}){buffText}";

        specialRouteBtn.GetComponentInChildren<TextMeshProUGUI>().text =
            $"데이트 신청\n(¥{currentSpecialCost:N0}){buffText}";

        // 취소 버튼 텍스트 (예: 그냥 쉰다)
        if (cancelBtn != null)
        {
            cancelBtn.GetComponentInChildren<TextMeshProUGUI>().text = "오늘은 그냥 쉰다\n(다음 날로)";
        }

        // 조건 체크
        bool canAffordNormal = GameManager.Instance.money >= currentNormalCost;
        bool canAffordSpecial = GameManager.Instance.money >= currentSpecialCost;
        bool enoughAffection = GameManager.Instance.heroineAffection >= eventData.requiredAffection;

        normalRouteBtn.interactable = canAffordNormal;
        specialRouteBtn.interactable = canAffordSpecial && enoughAffection;

        // 리스너 초기화 및 연결
        normalRouteBtn.onClick.RemoveAllListeners();
        specialRouteBtn.onClick.RemoveAllListeners();
        if (cancelBtn != null) cancelBtn.onClick.RemoveAllListeners();

        normalRouteBtn.onClick.AddListener(() => StartDialogue(false));
        specialRouteBtn.onClick.AddListener(() => {
            // 현재 날짜가 28일차라면 엔딩용 플래그를 True로 설정
            if (GameManager.Instance.gameDay == 28)
            {
                GameManager.Instance.isTsukinagaSpecialEnded = true;
                Debug.Log("<color=cyan>28일차 스페셜 데이트 선택: 엔딩 플래그 활성화!</color>");
            }

            StartDialogue(true);
        });

        // 취소 버튼 클릭 시: 돈 차감 없이 바로 종료 로직으로
        if (cancelBtn != null)
        {
            cancelBtn.onClick.AddListener(() => {
                choicePanel.SetActive(false);
                SkipToNextDay();
            });
        }
    }

    // 여행을 하지 않고 다음 날로 넘기는 별도 함수
    private void SkipToNextDay()
    {
        Debug.Log("여행을 취소하고 다음 날로 진행합니다.");

        // 추가: 여행을 가지 않고 쉬더라도 보유한 호르몬동 버프는 제거
        if (GameManager.Instance.hasHormoneBuff)
        {
            GameManager.Instance.hasHormoneBuff = false;
            Debug.Log("휴식을 선택하여 호르몬동 버프가 소멸되었습니다.");
        }

        // 여행을 가지 않았으므로 호감도 상승을 막기 위해 activeDialogueList를 null로 설정
        activeDialogueList = null;
        SoundManager.Instance.PlayBGM(SoundManager.Instance.mapBGM);
        EndTravelEvent();
    }

    public void StartDialogue(bool isSpecial)
    {
        choicePanel.SetActive(false);

        int finalCost = isSpecial ? currentSpecialCost : currentNormalCost;
        GameManager.Instance.money -= finalCost;

        if (GameManager.Instance.hasHormoneBuff)
        {
            GameManager.Instance.hasHormoneBuff = false;
            Debug.Log("호르몬동 버프가 소모되었습니다.");
        }

        activeDialogueList = isSpecial ? eventData.specialRoute : eventData.normalRoute;
        currentIndex = 0;
        isDialogueActive = true;

        if (heroineStanding != null) heroineStanding.sprite = eventData.heroineCostume;

        DisplayCurrentLine();
    }

    void Update()
    {
        if (isDialogueActive && Input.GetMouseButtonDown(0))
        {
            if (choicePanel.activeSelf) return;
            NextLine();
        }
    }

    private void NextLine()
    {
        DialogueSection currentSection = activeDialogueList[currentIndex];

        // 24일차 스페셜 루트 전용 분기 체크
        if (currentSection.checkAffectionBranch && GameManager.Instance.gameDay == 24)
        {
            // 현재 루트가 스페셜 루트인지 확인 (이미 StartDialogue에서 설정됨)
            bool isSpecialRoute = (activeDialogueList == eventData.specialRoute);

            if (isSpecialRoute && GameManager.Instance.heroineAffection >= 160)
            {
                ShowSugaShrineChoice(); // 선택지 팝업!
                return; // 선택 전까지 대기
            }
            else
            {
                EndTravelEvent();
            }
        }

        currentIndex++;
        if (currentIndex < activeDialogueList.Count)
        {
            DisplayCurrentLine();
        }
        else
        {
            EndTravelEvent();
        }
    }

    // 대사 출력 제어 (페이드 여부 확인)
    private void DisplayCurrentLine()
    {
        DialogueSection section = activeDialogueList[currentIndex];

        if (section.useFade)
        {
            StartCoroutine(FadeAndDisplayRoutine(section));
        }
        else
        {
            ApplyLineData(section);
        }
    }

    // 실제 데이터를 UI에 적용하는 로직 (분리)
    private void ApplyLineData(DialogueSection section)
    {
        nameText.text = section.speaker;
        dialogueText.text = section.line;

        // 1. 기본 클릭음 재생
        if (clickSFX != null) SoundManager.Instance.PlaySFX(clickSFX, 1.0f);

        // 2. 해당 대사에 설정된 특정 효과음(발소리 등) 재생
        if (section.sfxClip != null)
        {
            SoundManager.Instance.PlaySFX(section.sfxClip, section.sfxVolume);
        }

        // --- 추가된 BGM 제어 로직 ---
        if (section.bgmClip != null)
        {
            // 대사 데이터에 사운드 클립이 할당되어 있다면 해당 곡으로 교체
            SoundManager.Instance.PlayBGM(section.bgmClip, section.bgmVolume);
        }

        if (section.backgroundImage != null)
        {
            backgroundDisplay.sprite = section.backgroundImage;
        }

        if (section.characterSprite != null)
        {
            heroineStanding.sprite = section.characterSprite;
            heroineStanding.gameObject.SetActive(true);
        }

        if (section.isSpecialMoment && section.cgIllustration != null)
        {
            blackOverlay.SetActive(true);
            cgImage.sprite = section.cgIllustration;
            cgImage.gameObject.SetActive(true);
        }
        else
        {
            blackOverlay.SetActive(false);
            cgImage.gameObject.SetActive(false);
        }
    }

    // 페이드 연출 코루틴
    private IEnumerator FadeAndDisplayRoutine(DialogueSection section)
    {
        isDialogueActive = false; // 연출 중 클릭 방지

        FadeController fc = UnityEngine.Object.FindFirstObjectByType<FadeController>();

        if (fc != null)
        {
            // 1. 화면 어둡게 (Fade In)
            yield return StartCoroutine(fc.FadeInCoroutine());

            // 2. 데이터 교체
            ApplyLineData(section);

            // 3. 연출을 위한 짧은 대기
            yield return new WaitForSeconds(0.3f);

            // 4. 화면 밝게 (Fade Out)
            yield return StartCoroutine(fc.FadeOutCoroutine());
        }
        else
        {
            ApplyLineData(section);
        }

        isDialogueActive = true;
    }

    // --- 4. 이벤트 종료 및 결과 반영 ---
    private void EndTravelEvent()
    {
        isDialogueActive = false;

        // activeDialogueList가 있을 때만(여행을 갔을 때만) 호감도 계산
        if (activeDialogueList != null)
        {
            bool isSpecial = (activeDialogueList == eventData.specialRoute);
            int gain = isSpecial ? eventData.specialAffectionGain : eventData.normalAffectionGain;
            GameManager.Instance.heroineAffection += gain;

            if (eventData.eventDay == 24 && isSpecial && GameManager.Instance.day24Choice == 1)
            {
                GameManager.Instance.visitedSugaShrine = true;
            }
        }

        // 배고픔 보정 및 다음 날 진행은 동일하게 처리
        GameManager.Instance.RestoreHunger(6);

        bool isAlive = GameManager.Instance.AdvanceDay();

        if (isAlive)
        {
            SceneLoader sceneLoader = UnityEngine.Object.FindFirstObjectByType<SceneLoader>();
            if (sceneLoader != null)
            {
                sceneLoader.ReturnToMapScene("Map");
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Map");
            }
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameOverScene");
        }
    }
    private void ShowSugaShrineChoice()
    {
        isDialogueActive = false; // 클릭 방지
        branchPanel.SetActive(true);

        branchAText.text = "신경쓰지 않는다";
        branchBText.text = "츠키나가씨에게 다가간다";
    }

    // 버튼 클릭 시 호출 (choiceNumber 1: 신사행, 2: 귀가)
    public void OnSugaChoiceSelected(int choiceNumber)
    {
        branchPanel.SetActive(false);
        isDialogueActive = true;

        if (choiceNumber == 1)
        {
            GameManager.Instance.day24Choice = 1; // 신사 방문 플래그 저장
            Debug.Log("스가 신사 방문 결정! 28일차 운명이 바뀝니다.");

            // 추가 연출: 바로 대사를 이어가거나, 신사 전용 짧은 대사 리스트로 교체 가능
            currentIndex++;
            DisplayCurrentLine();
        }
        else
        {
            GameManager.Instance.day24Choice = 2; // 방문 안 함
            EndTravelEvent(); // 바로 집으로
        }
    }
}