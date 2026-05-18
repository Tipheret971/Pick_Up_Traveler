using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement; // 씬 이름 확인을 위해 필요
using UnityEngine.Events;
using System.IO;

// 굿즈 데이터 구조는 GoodsDefinitions.cs에 정의되어 있습니다.
// 이 클래스는 각 기계에 배치될 굿즈 정보를 담습니다.
[System.Serializable]
public class PrizeMachineState
{
    public GoodsData CurrentGoods;
    public string SceneName;
}

public class GameManager : MonoBehaviour
{
    // 싱글톤 인스턴스: 씬이 바뀌어도 이 객체를 통해 데이터에 접근
    public static GameManager Instance { get; private set; }

    // --- 영속 데이터 ---
    public int gameDay = 1; // 게임 내 날짜 (1일차부터 시작)
    public int money = 5000;

    // 3개의 크레인 씬 이름을 상수로 정의 (씬 이름이 다를 경우 수정 필요)
    private static readonly List<string> allCraneScenes = new List<string> { "Crane01", "Crane02", "Crane03" };

    // 기계 접근 제한: Key=씬 이름, Value=클리어한 날짜(gameDay)
    public Dictionary<string, int> clearedMachines = new Dictionary<string, int>();

    // 3개의 크레인 씬에 배치될 굿즈 정보를 저장할 리스트
    public List<PrizeMachineState> currentMachinePrizes = new List<PrizeMachineState>();

    // 획득한 굿즈를 저장할 인벤토리
    public List<GoodsData> inventory = new List<GoodsData>();

    // 리셀샵 보너스 시리즈 (매일 2개)
    public PrizeSeries bonusSeries1;
    public PrizeSeries bonusSeries2;

    // --- 시간 & 배고픔 시스템 ---
    [Header("Time & Hunger")]
    // public int currentDay = 1; // 현재 일자 - 이미 gameDay 변수 존재
    public int currentHunger = 10; // 현재 배고픔 (최소 0, 최대 10)
    private const int MAX_HUNGER = 10;
    private const int DAILY_HUNGER_DECREASE = 6;
    public int hungerDebuffStack = 0; // 공복 디버프 스택 (2스택 시 게임 오버)
    public int dailyRestaurantCount = 3; // 식당 입장 제한 횟수 (매일 3으로 리셋)
    private const int TRAVEL_EVENT_CYCLE = 4; // 여행 이벤트 발생 주기 (4일마다)

    // --- 버프 시스템 ---
    [Header("Buffs")]
    // 리셀샵 배율 버프 (기본 1.0)
    public float resaleMultiplier = 1.0f;
    // 식당/편의점 할인 버프 (기본 1.0, 0.75=25% 할인, 0.6=40% 할인)
    public float priceDiscountMultiplier = 1.0f;
    // 호르몬 정식 버프 (다음 주기 여행 이벤트 비용 50% 감소)
    public bool hasHormoneBuff = false;
    // 오므라이스 버프 (50% 확률 랜덤 비주얼 노벨 이벤트)
    public bool hasOmuriceBuff = false;
    // 소바 버프 (다음 게임센터 입장 시 코인 획득)
    public bool hasSobaBuff = false;

    [Header("Food System")]
    public int dailyFoodCount = 0; // 식당 방문 횟수 (최대 3회)

    [Header("Soba Buff Settings")]
    // 획득 금액: 200, 600, 1200, 10000 엔
    private readonly int[] SOBA_AMOUNTS = { 200, 600, 1200, 10000 };
    // 확률: 15%, 30%, 50%, 5% (총합 100%)
    private readonly float[] SOBA_PROBABILITIES = { 0.15f, 0.30f, 0.50f, 0.05f };

    [Header("Shrine System")]
    // 신사 버프 관리 변수
    public bool hasShrineDiscountBuff = false; // 40% 할인 (신사에서만 획득)
    public bool hasShrineResaleBuff = false;   // 1.5배 리셀 (신사에서만 획득)
    public bool hasShrineLuckyItemBuff = false; // 랜덤 굿즈 획득 (신사에서만 획득)

    public float shrineDiscountMultiplier = 0.6f; // 신사 전용 할인율
    public float shrineResaleMultiplier = 1.5f; // 신사 전용 리셀 배율

    // 추가: 오늘 참배를 했는지 확인 (매일 AdvanceDay()에서 초기화됨)
    public bool hasPrayedToday = false;

    // 참배 비용
    private const int SHRINE_COST = 50;
    // 버프 획득 확률 (30%)
    private const float SHRINE_BUFF_CHANCE = 0.30f;

    [Header("Affection System")]
    public int heroineAffection = 0; // 새로 추가: 히로인 호감도 (0부터 시작)

    public bool visitedSugaShrine = false; // 24일차 특별 이벤트 확인용
    public int day24Choice = 0; // 0: 선택 안함, 1: A루트, 2: B루트

    [Header("Ending Flags")]
    // 28일차에 츠키나가와 함께하는 루트를 끝까지 마쳤을 때 true로 바꿀 변수
    public bool isTsukinagaSpecialEnded = false;

    [Header("New Game Plus")]
    // 기본값은 1.0, 5만엔 엔딩 시마다 2.0을 곱함
    public float globalPriceMultiplier = 1.0f;

    public bool isNewGameStarting = false; // 새 게임 시작 플래그

    private void OnEnable()
    {
        // 씬이 로드될 때 OnSceneLoaded 함수가 실행되도록 유니티 시스템에 등록합니다.
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // 메모리 누수 방지를 위해 객체가 사라질 때 이벤트를 해제합니다.
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitGame();
            // 처음 게임을 실행했을 때만 이 로직을 탑니다.
        }
        else
        {
            // 맵 씬을 다시 불러와서 GameManager가 또 생겼을 때!
            // 만약 뉴게임 버튼을 눌러서 '데이터 초기화'가 필요한 상태라면
            if (Instance.isNewGameStarting)
            {
                // 이미 메모리에 살아있는 기존 Instance에게 초기화 명령을 내립니다.
                Instance.InitGame();
                Instance.isNewGameStarting = false;
            }

            // 새로 중복 생성된 오브젝트는 파괴해서 구조를 유지합니다.
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        // 1. 맵 씬에 들어왔을 때
        if (scene.name == "Map")
        {
            // 2. 새 게임 버튼을 통해 들어온 경우에만 InitGame 호출
            if (isNewGameStarting)
            {
                Debug.Log("새 게임 진입 확인: 초기화 로직을 실행합니다.");
                InitGame();

                // 3. 실행 후 플래그 초기화 (다음에 맵에 다시 들어올 땐 실행 안 되도록)
                isNewGameStarting = false;
            }
            else
            {
                // '이어하기'나 '다른 씬에서 복귀'할 때의 BGM 처리
                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayBGM(SoundManager.Instance.mapBGM, 1.0f);
                }
            }
        }
    }

    public void InitGame()
    {
        Debug.Log("[GameManager] 게임 최초 시작! 굿즈 데이터 생성 및 리셀 보너스 설정.");
        GenerateAllMachinePrizes();

        // 게임 시작 시 리셀 보너스 시리즈 2개 랜덤 결정
        bonusSeries1 = GetRandomSeries();
        do
        {
            bonusSeries2 = GetRandomSeries();
        } while (bonusSeries2 == bonusSeries1); // 중복되지 않도록

        Debug.Log($"[리셀 보너스] 오늘 보너스 시리즈: {bonusSeries1} & {bonusSeries2}");

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBGM(SoundManager.Instance.mapBGM, 1.0f);
            Debug.Log("BGM재생");
        }
    }

    // --- 날짜 관리 ---

    // 숙소 상호작용 시 호출 (날짜 진행 성공 여부 반환)
    public bool AdvanceDay()
    {
        // 1. 배고픔 수치 감소 (자고 일어났을 때 한 번만)
        currentHunger = Mathf.Max(0, currentHunger - DAILY_HUNGER_DECREASE);

        // 2. 공복 디버프 체크
        if (currentHunger <= 0) hungerDebuffStack += 1;
        else hungerDebuffStack = 0;

        // 공복 엔딩 조건
        if (hungerDebuffStack >= 2)
        {
            SceneLoader sceneLoader = UnityEngine.Object.FindFirstObjectByType<SceneLoader>();
            sceneLoader.LoadScene("EndingScene");
            return false;
        }

        // 3. 날짜 증가 (여기서 4, 8, 12...일이 됩니다)
        gameDay++;
        // 30일 도달 엔딩 조건
        if (gameDay >= 30)
        {
            SceneLoader sceneLoader = UnityEngine.Object.FindFirstObjectByType<SceneLoader>();
            sceneLoader.LoadScene("EndingScene");
            return false;
        }
        
        
        // ---------------------------------------------------------
        // 4. 아침 루틴
        // ---------------------------------------------------------
        dailyFoodCount = 0;
        GenerateAllMachinePrizes(); // 새로운 굿즈 배치
        
        // 리셀 보너스 결정
        bonusSeries1 = GetRandomSeries();
        do { bonusSeries2 = GetRandomSeries(); } while (bonusSeries2 == bonusSeries1);
        
        // 버프 초기화
        resaleMultiplier = 1.0f;
        priceDiscountMultiplier = 1.0f;
        hasOmuriceBuff = false;
        hasSobaBuff = false;
        hasPrayedToday = false;
        hasShrineDiscountBuff = false;
        hasShrineResaleBuff = false;
        hasShrineLuckyItemBuff = false;

        int[] travelDays = { 4, 8, 12, 16, 20, 24, 28 };
        if (System.Array.Exists(travelDays, day => day == gameDay))
        {
            Debug.Log($"<color=yellow>{gameDay}일차는 여행 이벤트 날입니다! 즉시 씬을 전환합니다.</color>");

            // 맵 씬의 다른 요소(굿즈 생성, 식당 리셋 등)가 동작하기 전에 씬을 넘겨버립니다.
            SceneLoader sceneLoader = UnityEngine.Object.FindFirstObjectByType<SceneLoader>();
            if (sceneLoader != null)
            {
                // 이전에 만든 전용 함수 호출
                sceneLoader.LoadTravelVNScene();
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("TravelEventScene");
            }

            // 중요: 씬 전환이 시작되었으므로 여기서 함수를 즉시 종료(true 반환)합니다.
            // 이 아래에 있는 '일반적인 날'을 위한 로직들은 실행되지 않습니다.
            return true;
        }
        SaveGame();

        Debug.Log($"일반적인 {gameDay}일차가 시작되었습니다.");
        return true;
    }

    // --- 배고픔 회복 헬퍼 함수 ---
    public void RestoreHunger(int amount)
    {
        // 배고픔 회복 로직
        currentHunger = Mathf.Min(MAX_HUNGER, currentHunger + amount);
        hungerDebuffStack = 0;
        Debug.Log($"배고픔 회복: +{amount}. 현재 배고픔: {currentHunger}");

        // Null 체크 후 안전하게 UI 업데이트 함수 호출
        // FindFirstObjectByType을 사용하여 HungerUI 인스턴스를 찾습니다.
        HungerUI hungerUI = UnityEngine.Object.FindFirstObjectByType<HungerUI>();

        if (hungerUI != null)
        {
            hungerUI.UpdateHungerDisplay(); // HungerUI의 업데이트 함수 호출
        }
        // else: UI가 씬에 없으면 (예: 3D 씬) 아무것도 하지 않습니다.
    }

    // --- 아이템 획득 및 접근 제한 ---

    // 굿즈 획득 시 호출될 함수 (PrizeExitController에서 사용 예정)
    public void AddGoodsToInventory(GoodsData goods)
    {
        inventory.Add(goods);
        Debug.Log($"굿즈 획득! 인벤토리에 {goods.Series} ({goods.Rarity}) 추가됨.");
    }

    // PrizeExitController에서 호출되어 씬 클리어 상태를 기록하는 함수
    public void SetMachineCleared(string machineName)
    {
        // 현재 gameDay로 클리어 기록
        clearedMachines[machineName] = gameDay;
    }

    // --- 접근 제한 확인 (UI Button에서 사용) ---

    // 해당 기계 씬(SceneName)에 오늘 접근 가능한지 확인
    public bool IsMachineAccessible(string sceneName)
    {
        // 1. Dictionary에 기록이 있는지 확인
        if (clearedMachines.ContainsKey(sceneName))
        {
            int lastClearedDay = clearedMachines[sceneName];

            // 2. 마지막 클리어 날짜가 오늘(gameDay)과 같다면 접근 불가
            if (lastClearedDay == gameDay)
            {
                return false; // 접근 불가 (오늘 이미 클리어함)
            }
        }

        return true; // 접근 가능
    }

    // 오늘 모든 3D 크레인 기계를 클리어했는지 확인
    public bool AreAllMachinesClearedToday()
    {
        foreach (string sceneName in allCraneScenes)
        {
            // IsMachineAccessible() 함수는 클리어하지 않았으면 True를 반환합니다.
            if (IsMachineAccessible(sceneName))
            {
                // 하나라도 접근 가능하다면 (즉, 클리어하지 않았다면)
                return false;
            }
        }

        // 모든 씬이 접근 불가능하다면 (모두 클리어했다면)
        return true;
    }

    // PrizeSeries에 따른 IP 이름을 반환하는 함수 추가
    public string GetSeriesName(PrizeSeries series)
    {
        // PrizeSeries는 0부터 9까지의 값을 가집니다.
        switch (series)
        {
            case PrizeSeries.SERIES_0: return "드래곤볼";
            case PrizeSeries.SERIES_1: return "원피스";
            case PrizeSeries.SERIES_2: return "귀멸의 칼날";
            case PrizeSeries.SERIES_3: return "주술회전";
            case PrizeSeries.SERIES_4: return "하츠네 미쿠";
            case PrizeSeries.SERIES_5: return "스파이 패밀리";
            case PrizeSeries.SERIES_6: return "체인소 맨";
            case PrizeSeries.SERIES_7: return "오등분의 신부";
            case PrizeSeries.SERIES_8: return "장송의 프리렌";
            case PrizeSeries.SERIES_9: return "에반게리온";
            default: return $"알 수 없는 시리즈 {(int)series}";
        }
    }

    // 확률 기반 굿즈 생성 및 리스트에 저장
    public void GenerateAllMachinePrizes()
    {
        // 이전 데이터 초기화
        currentMachinePrizes.Clear();

        // 씬 이름 순서 (Crane01, Crane02, Crane03)에 맞춰 굿즈를 3개 생성
        for (int i = 0; i < 3; i++)
        {
            PrizeSeries series = GetRandomSeries();
            PrizeRarity rarity = GetRandomRarity();

            currentMachinePrizes.Add(new PrizeMachineState
            {
                CurrentGoods = new GoodsData { Series = series, Rarity = rarity },
                SceneName = $"Crane0{i + 1}" // "Crane01", "Crane02", "Crane03"
            });

            Debug.Log($"[굿즈 생성] {currentMachinePrizes[i].SceneName}: {series} / {rarity} (넘버링: {currentMachinePrizes[i].CurrentGoods.Numbering})");
        }
    }

    // 확률 로직 1: 굿즈 시리즈 결정 (각 10% 확률)
    private PrizeSeries GetRandomSeries()
    {
        int index = Random.Range(0, 10); // 0부터 9까지 랜덤 정수
        return (PrizeSeries)index;
    }

    // 확률 로직 2: 희귀도 결정 (일반 50%, 희귀 35%, 전설 15%)
    private PrizeRarity GetRandomRarity()
    {
        float roll = Random.value; // 0.0 ~ 1.0 사이의 실수
        if (roll < 0.5f) // 50%
        {
            return PrizeRarity.Normal;
        }
        else if (roll < 0.85f) // 35% (0.50 이상 0.85 미만)
        {
            return PrizeRarity.Rare;
        }
        else // 15% (0.85 이상 1.0 미만)
        {
            return PrizeRarity.Legendary;
        }
    }

    // 식당 메뉴에 따라 버프를 적용하고 배고픔을 회복합니다.
    public void ApplyRestaurantBuff(string menuName)
    {
        // 1. 배고픔 회복 (모든 식당 메뉴는 3 회복)
        RestoreHunger(3);

        // 2. 일일 식당 카운트 차감
        dailyFoodCount++;

        bool buffApplied = false;

        // 3. 메뉴에 따른 버프 적용 (이미 버프가 있으면 추가 적용하지 않음)
        switch (menuName)
        {
            case "카이센동": // 리셀 배율 1.1f
                if (resaleMultiplier <= 1.0f)
                {
                    resaleMultiplier = 1.1f;
                    buffApplied = true;
                }
                break;

            case "돈카츠": // 25% 할인 (0.75배)
                if (priceDiscountMultiplier >= 1.0f)
                {
                    priceDiscountMultiplier = 0.75f;
                    buffApplied = true;
                }
                break;

            case "호르몬동": // 여행 이벤트 할인
                if (!hasHormoneBuff)
                {
                    hasHormoneBuff = true;
                    buffApplied = true;
                }
                break;

            case "오므라이스": // 랜덤 VN 이벤트
                if (!hasOmuriceBuff)
                {
                    hasOmuriceBuff = true;
                    buffApplied = true;
                }
                break;

            case "소바": // 게임센터 입장 시 돈 획득
                if (!hasSobaBuff)
                {
                    hasSobaBuff = true;
                    buffApplied = true;
                }
                break;
        }
        if (buffApplied)
        {
            Debug.Log($"[식당 버프 적용] {menuName} 버프를 획득했습니다.");
        }
        else
        {
            Debug.Log($"[식사 완료] {menuName}을 먹고 HP만 회복했습니다. 버프는 이미 보유 중입니다.");
        }
        // TODO: Buff UI 업데이트 로직 호출
    }

    // 굿즈 판매 시 가격 계산 함수
    public int CalculateBaseResalePrice(GoodsData goods, int basePrice)
    {
        float multiplier = 1.0f;

        // 1. 시리즈 보너스 (1.5배)
        // 굿즈의 시리즈가 오늘 보너스 시리즈 2개 중 하나인지 확인
        if (goods.Series == bonusSeries1 || goods.Series == bonusSeries2)
        {
            multiplier *= 1.5f;
        }

        // 2. 희귀도 보너스 추가 (희귀도 * 10% 추가)
        // Normal(0): +0%, Rare(1): +50%, Legendary(2): +100%
        multiplier += (int)goods.Rarity * 0.5f;

        // 3. 기본 가격 계산
        int finalPrice = Mathf.RoundToInt(basePrice * multiplier);

        return finalPrice;
    }

    // 최종 리셀 배율을 계산합니다.
    public float GetCurrentResaleMultiplier()
    {
        // resaleMultiplier: 카이센동 버프(1.1) + 신사 버프(1.5)를 합산하여 최종 배율을 반환

        // Base Multiplier (1.0)
        float baseResale = 1.0f;

        // 카이센동 버프 (1.1배)
        if (resaleMultiplier > 1.0f) baseResale += (resaleMultiplier - 1.0f);

        // 신사 리셀 버프 (1.5배)
        if (hasShrineResaleBuff) baseResale += (shrineResaleMultiplier - 1.0f);

        return baseResale;
    }

    // 최종적으로 리셀샵 UI에서 호출될 함수입니다.
    public int CalculateFinalResalePrice(GoodsData goods, int basePrice)
    {
        // 1. 기본 배율 가격 계산 (시리즈, 희귀도 보너스 등)
        int priceBeforeBuff = CalculateBaseResalePrice(goods, basePrice);

        // 2. 현재 활성화된 버프 (카이센동, 신사 등)
        float activeBuffMultiplier = GetCurrentResaleMultiplier();

        // 3. 회차 누적 배율 (globalPriceMultiplier) 적용
        // 최종 가격 = (기본가 * 시리즈/희귀도) * 각종버프 * 회차배율
        int finalPrice = Mathf.RoundToInt(priceBeforeBuff * activeBuffMultiplier * globalPriceMultiplier);

        return finalPrice;
    }

    // 최종 할인율을 계산합니다.
    public float GetCurrentPriceDiscount()
    {
        // 돈카츠 버프(25%)와 신사 버프(40%)를 합산하여 최종 할인율을 반환
        // priceDiscountMultiplier: 돈카츠 버프가 있으면 0.75, 없으면 1.0
        // shrineDiscountMultiplier: 신사 버프가 있으면 0.6, 없으면 1.0

        float currentDiscount = (1.0f - priceDiscountMultiplier);

        if (hasShrineDiscountBuff) currentDiscount += (1.0f - shrineDiscountMultiplier);

        // 합산 할인율을 최종 곱셈 배율로 변환 (예: 25% + 40% = 65% 할인 -> 0.35배)
        return Mathf.Clamp(1.0f - currentDiscount, 0.0f, 1.0f);
    }

    // 소바 버프 효과를 실행합니다.
    public int ExecuteSobaBuff()
    {
        if (!hasSobaBuff) return 0; // 버프가 없다면 0 반환

        // 1. 확률에 따라 획득 금액 결정
        float randomRoll = Random.value; // 0.0 ~ 1.0
        float cumulativeProbability = 0f;
        int acquiredAmount = 0;

        for (int i = 0; i < SOBA_PROBABILITIES.Length; i++)
        {
            cumulativeProbability += SOBA_PROBABILITIES[i];

            if (randomRoll <= cumulativeProbability)
            {
                acquiredAmount = SOBA_AMOUNTS[i];
                break;
            }
        }

        // 2. 돈 획득
        if (acquiredAmount > 0)
        {
            money += acquiredAmount;
            Debug.Log($"[소바 버프] 발동! 랜덤 금액 ¥{acquiredAmount} 획득.");

            // TODO: (옵션) UI에 돈 획득 알림 표시 로직 필요
        }
        else
        {
            Debug.Log("[소바 버프] 발동 실패 또는 금액 획득 실패.");
        }

        // 3. 버프 소멸 (돈 획득 여부와 관계없이 소멸)
        hasSobaBuff = false;

        // TODO: Buff UI 업데이트 로직 호출
        // 획득 금액 반환
        return acquiredAmount;
    }
    // 신사 참배 로직 (UI에서 호출됨)
    // 반환: 획득한 버프의 종류 인덱스 (0:할인, 1:리셀, 2:굿즈) 또는 -1 (버프 실패)
    public int PrayAtShrine()
    {
        if (money < SHRINE_COST) return -2; // 돈 부족 (-2 반환)
        if (hasPrayedToday) return -3; // 이미 참배함 (-3 반환, UI가 막지만 혹시 모를 상황 대비)

        money -= SHRINE_COST; // 참배 비용 50엔 지불
        hasPrayedToday = true; // 참배 완료 상태 설정

        // 1. 버프 초기화 (버프는 중복 획득되므로, 획득 전에 기존 신사 버프를 제거)
        hasShrineDiscountBuff = false;
        hasShrineResaleBuff = false;
        hasShrineLuckyItemBuff = false;

        // 2. 30% 확률로 버프 획득 시도
        if (Random.value <= SHRINE_BUFF_CHANCE)
        {
            // 3. 3가지 버프 중 랜덤으로 하나 선택
            int buffIndex = Random.Range(0, 3); // 0, 1, 2 중 하나

            switch (buffIndex)
            {
                case 0: // 40% 할인 버프
                    hasShrineDiscountBuff = true;
                    break;
                case 1: // 1.5배 리셀 버프
                    hasShrineResaleBuff = true;
                    break;
                case 2: // 랜덤 굿즈 획득 버프
                    hasShrineLuckyItemBuff = true;
                    // TODO: 굿즈 획득 로직은 SceneLoader에서 처리해야 합니다.
                    break;
            }

            // TODO: Buff UI 업데이트 로직 호출
            return buffIndex; // 성공 (0, 1, 또는 2 반환)
        }

        return -1; // 참배는 했으나 버프 획득 실패
    }

    public GoodsData ExecuteShrineLuckyItemBuff()
    {
        // if (!hasShrineLuckyItemBuff) return null; // 버프가 없다면 null 반환

        // 1. 새로운 굿즈 데이터 생성 (랜덤 희귀도와 시리즈)
        PrizeSeries series = GetRandomSeries();
        PrizeRarity rarity = GetRandomRarity();

        GoodsData randomGoods = new GoodsData
        {
            Series = series,
            Rarity = rarity,
            // 기본 가격은 임의로 10000으로 가정합니다. 
            // (굿즈를 획득하는 시점에는 PrizeItem에서 가져올 수 없으므로 고정값 사용)
            // BasePrice = 10000
        };

        // 2. 인벤토리에 추가
        AddGoodsToInventory(randomGoods);

        // 3. 버프 소멸
        hasShrineLuckyItemBuff = false;

        Debug.Log($"[신사 버프] 발동! 랜덤 굿즈 '{GetSeriesName(series)} {rarity}' 획득.");
        // TODO: Buff UI 업데이트 로직 호출
        // TODO: (나중에) 굿즈 획득 알림 UI 텍스트 상자 출력 로직 추가
        // 획득한 굿즈 데이터 반환
        return randomGoods;
    }

    public void SaveGame()
    {
        SaveData data = new SaveData();
        data.day = gameDay;
        data.money = money;
        data.hunger = currentHunger;
        data.hungerDebuff = hungerDebuffStack;
        data.dailyFoodCount = dailyFoodCount;
        data.affection = heroineAffection;
        data.day24Choice = day24Choice;
        data.visitedSugaShrine = visitedSugaShrine;
        data.isTsukinagaSpecialEnded = isTsukinagaSpecialEnded;
        data.resaleMultiplier = resaleMultiplier;
        data.priceDiscountMultiplier = priceDiscountMultiplier;
        data.hasHormoneBuff = hasHormoneBuff;
        data.hasOmuriceBuff = hasOmuriceBuff;
        data.hasSobaBuff = hasSobaBuff;
        data.hasShrineDiscountBuff = hasShrineDiscountBuff;
        data.hasShrineResaleBuff = hasShrineResaleBuff;
        data.hasShrineLuckyItemBuff = hasShrineLuckyItemBuff;
        data.hasPrayedToday = hasPrayedToday;
        data.inventory = new List<GoodsData>(inventory); // 리스트 복사
        data.globalPriceMultiplier = globalPriceMultiplier;

        string json = JsonUtility.ToJson(data, true); // true는 보기 편하게 줄바꿈 포함
        File.WriteAllText(Application.persistentDataPath + "/savefile.json", json);
        Debug.Log($"저장 완료: {Application.persistentDataPath}/savefile.json");
    }

    public bool LoadGame() // 성공 여부를 반환하도록 bool로 변경
    {
        string path = Application.persistentDataPath + "/savefile.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            gameDay = data.day;
            money = data.money;
            currentHunger = data.hunger;
            hungerDebuffStack = data.hungerDebuff;
            dailyFoodCount = data.dailyFoodCount;
            heroineAffection = data.affection;
            day24Choice = data.day24Choice;
            visitedSugaShrine = data.visitedSugaShrine;
            isTsukinagaSpecialEnded = data.isTsukinagaSpecialEnded;
            resaleMultiplier = data.resaleMultiplier;
            priceDiscountMultiplier = data.priceDiscountMultiplier;
            hasHormoneBuff = data.hasHormoneBuff;
            hasOmuriceBuff = data.hasOmuriceBuff;
            hasSobaBuff = data.hasSobaBuff;
            hasShrineDiscountBuff = data.hasShrineDiscountBuff;
            hasShrineResaleBuff = data.hasShrineResaleBuff;
            hasShrineLuckyItemBuff = data.hasShrineLuckyItemBuff;
            hasPrayedToday = data.hasPrayedToday;
            inventory = new List<GoodsData>(data.inventory);
            globalPriceMultiplier = data.globalPriceMultiplier;

            Debug.Log("불러오기 완료!");
            return true;
        }
        return false;
    }

    // 세이브 파일이 있는지 확인하는 헬퍼 함수
    public bool HasSaveFile()
    {
        return File.Exists(Application.persistentDataPath + "/savefile.json");
    }

    // ⭐ 새 게임 시작 시 호출할 데이터 초기화 함수
    public void ResetDataForNewGame()
    {
        // 1. 중요: 회차 배율은 미리 백업해둡니다.
        float nGPlusMultiplier = globalPriceMultiplier;

        // 2. 모든 진행 데이터 초기화
        gameDay = 1;
        money = 5000; // 초기 자금 (기획에 따라 조정)
        currentHunger = 10; // 최대 허기치
        dailyFoodCount = 0;
        hungerDebuffStack = 0;
        heroineAffection = 0;
        day24Choice = 0;
        visitedSugaShrine = false;
        isTsukinagaSpecialEnded = false;

        // 리스트 및 버프 상태 초기화
        inventory.Clear();
        resaleMultiplier = 1.0f;
        priceDiscountMultiplier = 1.0f;
        hasHormoneBuff = false;
        hasOmuriceBuff = false;
        hasSobaBuff = false;
        hasShrineDiscountBuff = false;
        hasShrineResaleBuff = false;
        hasShrineLuckyItemBuff = false;

        hasPrayedToday = false;

        // 튜토리얼 초기화
        PlayerPrefs.DeleteKey("HasSeenCraneTutorial");
        PlayerPrefs.Save();

        // 3. 백업했던 배율만 다시 적용
        globalPriceMultiplier = nGPlusMultiplier;

        isNewGameStarting = true; // 새 게임 시작 상태임을 표시
        Debug.Log("새 게임 플래그 설정 완료.");

        Debug.Log($"현재 누적 가격 배율: x{globalPriceMultiplier}");
    }
}