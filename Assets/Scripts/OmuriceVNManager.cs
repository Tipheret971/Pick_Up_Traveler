using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq; // List<T>를 사용하기 위해 필요

public class OmuriceVNManager : MonoBehaviour
{
    [Header("VN Elements")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI vnNameTextBox;
    [SerializeField] private Image characterImage; // 등장 캐릭터 이미지 표시
    [SerializeField] private Button clickButton; // 클릭 감지용 버튼

    [Header("Character Assets")]
    public Sprite maidSprite;
    public Sprite convenienceSprite;
    public Sprite koreanSprite;

    [Header("Heroine Costumes")]
    public Sprite[] fixedCostumes = new Sprite[3]; // 고정 복장 3종
    public Sprite[] variableCostumes = new Sprite[3]; // 가변 복장 3종

    // VN 진행 데이터
    private List<DialogueEntry> currentDialogueEntries; // List<DialogueEntry> 사용
    private int dialogueIndex = 0;
    private bool isHeroine = false;
    private Character currentCharacter; // 현재 캐릭터 저장

    // 캐릭터 정의 (0:히로인, 1:메이드, 2:편의점, 3:한국인)
    private enum Character { Heroine, Maid, Convenience, Korean }

    // 대화 항목 구조체 정의
    private struct DialogueEntry
    {
        public string Speaker; // 화자 이름 ("츠키나가", "나", 기타 캐릭터 이름, ""는 관찰 텍스트)
        public string Line;    // 출력할 대사
    }

    // --- 대화 데이터 정의 ---
    private readonly DialogueEntry[] maidDialogue = new DialogueEntry[]
    {
        new DialogueEntry { Speaker = "메이드카페 직원", Line = "메이드리빙 아키하바라점이에요~ 주인님~" },
        new DialogueEntry { Speaker = "나", Line = "아...괜찮아요..." },
        new DialogueEntry { Speaker = "메이드카페 직원", Line = "진짜 안오실거에요...?" },
        new DialogueEntry { Speaker = "", Line = "(궁금하긴 하지만... 혼자서 가기는 좀...)" }
    };

    private readonly DialogueEntry[] convenienceDialogue = new DialogueEntry[]
    {
        new DialogueEntry { Speaker = "편의점 알바", Line = "음......" },
        new DialogueEntry { Speaker = "나", Line = "......?" },
        new DialogueEntry { Speaker = "편의점 알바", Line = "...아니에요." },
        new DialogueEntry { Speaker = "", Line = "(뭐야...?)" }
    };

    private readonly DialogueEntry[] koreanDialogue = new DialogueEntry[]
    {
        new DialogueEntry { Speaker = "지나가는 관광객", Line = "어...저기 혹시 이 근처에 메이드리빙이 어딘지 아세요?" },
        new DialogueEntry { Speaker = "나", Line = "예? 저도 잘 모르겠네요..." },
        new DialogueEntry { Speaker = "지나가는 관광객", Line = "아...실례했습니다." },
        new DialogueEntry { Speaker = "", Line = "(메이드카페를 혼자서... 나는 못할 것 같아.)" }
    };

    private readonly DialogueEntry[] tsukinagaFixed1Dialogue = new DialogueEntry[]
    {
        new DialogueEntry { Speaker = "츠키나가", Line = "어? 여기서 또보네? 반가워~" },
        new DialogueEntry { Speaker = "나", Line = "아, 츠키나가씨 반가워요, 어디가는 길이에요?" },
        new DialogueEntry { Speaker = "츠키나가", Line = "글쎄...너 만나러...?" },
        new DialogueEntry { Speaker = "나", Line = "...네?" },
        new DialogueEntry { Speaker = "츠키나가", Line = "아하하~ 농담이야, 그냥 마트에 좀 가려구. 다음에 봐~" },
        new DialogueEntry { Speaker = "", Line = "(흰티에 청바지가 저렇게 폭력적일 줄이야...)" }
    };
    private readonly DialogueEntry[] tsukinagaFixed2Dialogue = new DialogueEntry[]
    {
        new DialogueEntry { Speaker = "츠키나가", Line = "후우...후우...아? 안녕~ 여기서 뭐해?" },
        new DialogueEntry { Speaker = "나", Line = "음...그냥 근처 둘러보고있어요. 츠키나가씨는 운동중인가봐요?" },
        new DialogueEntry { Speaker = "츠키나가", Line = "응, 얼마전부터 다이어트 시작했거든, 갑자기 뛰려니까 힘들어..." },
        new DialogueEntry { Speaker = "나", Line = "다이어트요?" },
        new DialogueEntry { Speaker = "츠키나가", Line = "음...그럴 이유가 생겼거든, 다음에 봐~" },
        new DialogueEntry { Speaker = "", Line = "(다이어트 안하셔도 될 것 같은데...)" }
    };
    private readonly DialogueEntry[] tsukinagaFixed3Dialogue = new DialogueEntry[]
    {
        new DialogueEntry { Speaker = "츠키나가", Line = "안녕~ 또보네?" },
        new DialogueEntry { Speaker = "나", Line = "오...안녕하세요...오늘은 엄청 화려하게 입으셨네요...?" },
        new DialogueEntry { Speaker = "츠키나가", Line = "이거? 무명 스튜디오에서 모델 알바 하는중이야. 어때?" },
        new DialogueEntry { Speaker = "나", Line = "너무 잘 어울려요! 완전 유명하실 것 같은데...?" },
        new DialogueEntry { Speaker = "츠키나가", Line = "그랬으면 좋겠네, 아 늦겠다, 다음에 봐!" },
        new DialogueEntry { Speaker = "", Line = "(무명...? 어지간한 모델들은 명함도 못내밀겠는데...)" }
    };
    private readonly DialogueEntry[] tsukinagaVariableDialogue = new DialogueEntry[]
    {
        new DialogueEntry { Speaker = "츠키나가", Line = "안녕~ 오늘은 어디가는 길이야?" },
        new DialogueEntry { Speaker = "나", Line = "밥도 다먹었고, 게임센터나 가볼까 해요." },
        new DialogueEntry { Speaker = "츠키나가", Line = "게임센터 거의 매일 가는거 아냐?" },
        new DialogueEntry { Speaker = "나", Line = "게임센터에 가면 츠키나가씨를 만날 수 있을 것 같거든요" },
        new DialogueEntry { Speaker = "츠키나가", Line = "......?! 플러팅이야?" },
        new DialogueEntry { Speaker = "", Line = "(츠키나가씨가 당황하는 모습... 귀엽다...!)" }
    };
    // ------------------------------------

    void Start()
    {
        SoundManager.Instance.PlayBGM(SoundManager.Instance.dayBGM);
        DetermineCharacterAndStartVN();

        if (clickButton != null)
        {
            clickButton.onClick.AddListener(OnClicked);
        }
    }

    // --- 캐릭터 결정 및 VN 시작 로직 ---
    private void DetermineCharacterAndStartVN()
    {
        if (GameManager.Instance == null) return;

        Character selectedChar = SelectCharacter(GameManager.Instance.gameDay);

        SetupCharacter(selectedChar);

        // 수정된 변수 이름 사용: currentDialogueEntries
        if (currentDialogueEntries != null && currentDialogueEntries.Count > 0)
        {
            DisplayNextDialogue();
        }
    }

    private Character SelectCharacter(int currentDay)
    {
        // 1. 히로인 출현 조건 체크 (4일차~28일차)
        bool canHeroineAppear = currentDay >= 4 && currentDay <= 28;

        // 2. 랜덤 선택 (0:히로인, 1:메이드, 2:편의점, 3:한국인)
        int selectionPoolSize = canHeroineAppear ? 4 : 3;
        int randomIndex = Random.Range(0, selectionPoolSize);

        // 3. 히로인 출현 가능 시, 인덱스 0을 히로인으로 고정
        if (canHeroineAppear && randomIndex == 0)
        {
            isHeroine = true;
            return Character.Heroine;
        }

        // 4. 일반 캐릭터 3명 매핑
        // 29일차 이상이거나, 4명 풀에서 Heroine(0)이 아닌 경우
        if (!canHeroineAppear || randomIndex != 0)
        {
            // 4명 풀에서 Heroine(0) 제외 -> 1, 2, 3 인덱스 그대로 사용
            // 3명 풀 (0, 1, 2) -> +1 하여 1, 2, 3 인덱스로 매핑
            return (Character)(canHeroineAppear ? randomIndex : randomIndex + 1);
        }

        // 비상 시 (도달 불가)
        return Character.Maid;
    }

    private void SetupCharacter(Character character)
    {
        currentCharacter = character;

        // 1. 복장/대화 설정
        switch (character)
        {
            case Character.Heroine:
                // 수정: 통합 함수 호출
                (Sprite selectedSprite, DialogueEntry[] selectedScript) = SelectHeroineCostumeAndDialogue(GameManager.Instance.gameDay);

                characterImage.sprite = selectedSprite;
                // 대화 스크립트 할당
                currentDialogueEntries = new List<DialogueEntry>(selectedScript);
                isHeroine = true;
                break;
            case Character.Maid:
                characterImage.sprite = maidSprite;
                currentDialogueEntries = new List<DialogueEntry>(maidDialogue);
                break;
            case Character.Convenience:
                characterImage.sprite = convenienceSprite;
                currentDialogueEntries = new List<DialogueEntry>(convenienceDialogue);
                break;
            case Character.Korean:
                characterImage.sprite = koreanSprite;
                currentDialogueEntries = new List<DialogueEntry>(koreanDialogue);
                break;
        }

        // 2. VN 시작 (DisplayNextDialogue에서 이름 설정됨)
        if (currentDialogueEntries != null && currentDialogueEntries.Count > 0)
        {
            DisplayNextDialogue();
        }

        characterImage.gameObject.SetActive(true);
    }

    // 히로인 복장 선택 로직 (GameManager.gameDay 기준)
    private Sprite SelectHeroineCostume(int currentDay)
    {
        List<Sprite> availableCostumes = new List<Sprite>();
        availableCostumes.AddRange(fixedCostumes);

        int variableIndex = -1;

        if (currentDay >= 4 && currentDay <= 12 && variableCostumes.Length > 0)
            variableIndex = 0;
        else if (currentDay >= 13 && currentDay <= 20 && variableCostumes.Length > 1)
            variableIndex = 1;
        else if (currentDay >= 21 && currentDay <= 28 && variableCostumes.Length > 2)
            variableIndex = 2;

        if (variableIndex != -1)
        {
            availableCostumes.Add(variableCostumes[variableIndex]);
        }

        if (availableCostumes.Count > 0)
        {
            int randomIndex = Random.Range(0, availableCostumes.Count);
            return availableCostumes[randomIndex];
        }

        return fixedCostumes[0];
    }

    // 복장 선택 및 해당 대화 스크립트를 반환하는 통합 함수
    private (Sprite selectedSprite, DialogueEntry[] selectedScript) SelectHeroineCostumeAndDialogue(int currentDay)
    {
        List<(Sprite sprite, DialogueEntry[] script)> availableSets = new List<(Sprite, DialogueEntry[])>();

        // 1. 고정 복장 3개 연결 (Sprite와 Dialogue Script를 묶음)
        // [0] -> Fixed1, [1] -> Fixed2, [2] -> Fixed3
        for (int i = 0; i < fixedCostumes.Length; i++)
        {
            // availableSets.Add((fixedCostumes[i], tsukinagaFixed1Dialogue)); // 일단 모든 고정 복장에 Fixed1 대사를 연결

            // 실제 복장과 대사를 연결:
            if (i == 0) availableSets.Add((fixedCostumes[i], tsukinagaFixed1Dialogue));
            else if (i == 1) availableSets.Add((fixedCostumes[i], tsukinagaFixed2Dialogue));
            else if (i == 2) availableSets.Add((fixedCostumes[i], tsukinagaFixed3Dialogue));
        }

        // 2. 가변 복장 1개 연결 (날짜 주기별 체크)
        DialogueEntry[] variableScript = tsukinagaVariableDialogue;
        int variableIndex = -1;

        if (currentDay >= 4 && currentDay <= 12 && variableCostumes.Length > 0)
            variableIndex = 0;
        else if (currentDay >= 13 && currentDay <= 20 && variableCostumes.Length > 1)
            variableIndex = 1;
        else if (currentDay >= 21 && currentDay <= 28 && variableCostumes.Length > 2)
            variableIndex = 2;

        if (variableIndex != -1)
        {
            // 가변 복장 Sprite와 가변 대화 스크립트를 연결
            availableSets.Add((variableCostumes[variableIndex], variableScript));
        }

        // 3. 사용 가능한 세트 중 랜덤 선택
        if (availableSets.Count > 0)
        {
            int randomIndex = Random.Range(0, availableSets.Count);
            return availableSets[randomIndex];
        }

        // 비상 시 기본 세트 반환
        return (fixedCostumes[0], tsukinagaFixed1Dialogue);
    }

    // --- 대사 진행 및 종료 로직 ---

    private void OnClicked()
    {
        // 1. 현재 라인이 마지막 발언인지 확인 (다음 라인이 관찰 텍스트인지 체크)
        if (dialogueIndex + 1 < currentDialogueEntries.Count)
        {
            DialogueEntry nextEntry = currentDialogueEntries[dialogueIndex + 1];

            if (string.IsNullOrEmpty(nextEntry.Speaker))
            {
                // 현재 라인이 마지막 발언이므로, 클릭 시 이미지 제거 액션 실행
                characterImage.gameObject.SetActive(false);
                vnNameTextBox.text = ""; // 이름 텍스트 비우기
            }
        }

        // 2. 인덱스 증가 및 대화 진행 (이때 다음 라인(관찰 텍스트)이 출력됨)
        dialogueIndex++;

        if (dialogueIndex < currentDialogueEntries.Count)
        {
            DisplayNextDialogue();
        }
        else
        {
            // 모든 대화(관찰 텍스트 포함)가 끝났을 때 이벤트 종료
            EndEncounter();
        }
    }

    private void DisplayNextDialogue()
    {
        DialogueEntry currentEntry = currentDialogueEntries[dialogueIndex];

        // 1. 이름 텍스트 업데이트 (OnClicked에서 이미 비워졌거나 새 이름이 들어옴)
        vnNameTextBox.text = currentEntry.Speaker;

        // 2. 대화 텍스트 업데이트
        dialogueText.text = currentEntry.Line;

        // 3. 액션 처리 로직 제거 (OnClicked로 이동됨)
        // 이전 로직은 여기서 제거되었습니다.
    }

    private void EndEncounter()
    {
        if (isHeroine)
        {
            GameManager.Instance.heroineAffection += 5;
            Debug.Log($"호감도 +5! 현재 호감도: {GameManager.Instance.heroineAffection}");
        }

        GameManager.Instance.hasOmuriceBuff = false;

        SceneLoader sceneLoader = UnityEngine.Object.FindFirstObjectByType<SceneLoader>();
        if (sceneLoader != null)
        {
            sceneLoader.ReturnToMapScene("Map"); // Map 씬 이름으로 복귀
        }
    }

    // --- (사용되지 않는 임시 대사 함수 제거) ---
}