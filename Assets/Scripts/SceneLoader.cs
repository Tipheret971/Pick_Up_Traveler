using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System; // Action 사용을 위해 추가

public class SceneLoader : MonoBehaviour
{
    [Header("UI Notifiers")]
    // 알림 UI 참조 필드 추가 (인스펙터에서 VN_NotificationUI 컴포넌트 연결 필요)
    [SerializeField] private VN_NotificationUI vnNotifier;
    // 이 컴포넌트는 Map 씬 로드 시 Start() 등에서 찾아와 할당해야 합니다.
    [Header("VN Sprites")]
    // 코인 무더기 이미지를 인스펙터에 연결할 필드
    [SerializeField] private Sprite coinPileSprite;
    [SerializeField] private Sprite shrineImageSprite;
    // [SerializeField] private Sprite shrineImageSprite; // 신사 버프용은 나중에 사용

    // 순서대로 체크할 크레인 씬 목록
    private static readonly List<string> CRANE_SCENES_ORDER = new List<string> { "Crane01", "Crane02", "Crane03" };
    // 오므라이스 VN 이벤트 씬 이름 (가정)
    private const string OMURICE_VN_SCENE = "OmuriceEncounterScene";
    private const string TRAVEL_VN_SCENE = "TravelEventScene";

    // --- 새로운 유틸리티 함수: 페이드 아웃을 시작하고 씬을 로드 ---
    private void StartFadeLoad(string sceneName)
    {
        // 1. 현재 씬의 FadeController를 찾음
        FadeController fc = UnityEngine.Object.FindFirstObjectByType<FadeController>();

        // 2. 씬 로드를 실행할 액션 정의
        Action loadAction = () =>
        {
            SceneManager.LoadScene(sceneName);
        };

        // 3. FadeController가 있다면 페이드 연출을 통해 로드
        if (fc != null)
        {
            // FadeInAndExecute: 검은색으로 변한 후(Fade In), 콜백(loadAction) 실행
            fc.FadeInAndExecute(loadAction);
        }
        else
        {
            // FadeController가 없다면 즉시 로드 (비상용)
            SceneManager.LoadScene(sceneName);
        }
    }

    void Start()
    {
        // Awake/Start 시점에 씬에서 VN_NotificationUI 컴포넌트를 찾습니다.
        vnNotifier = UnityEngine.Object.FindAnyObjectByType<VN_NotificationUI>(FindObjectsInactive.Include);

        if (vnNotifier == null)
        {
            Debug.LogWarning("VN_NotificationUI를 씬에서 찾을 수 없습니다. 알림 기능은 비활성화됩니다.");
        }
    }

    // 2D 맵의 게임센터 버튼에 연결될 함수
    public void LoadFirstAvailableCraneScene()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager가 로드되지 않았습니다. 씬 로드를 중단합니다.");
            return;
        }

        // 변경점 1: 진입 가능 여부(다음 씬 이름)를 '확인'만 먼저 해둡니다.
        string nextSceneName = GetNextSceneName();

        // 변경점 2: 조건 없이 바로 버프 알림 체인을 시작합니다.
        // (갈 곳이 없더라도 소바 돈을 줍거나 신사 선물을 받는 이벤트는 정상 작동하게 됩니다.)
        StartBuffNotificationChain(nextSceneName);
    }

    // 씬 이름(string)을 받아 해당 씬으로 바로 이동합니다.
    public void LoadScene(string sceneName)
    {
        // 핵심 수정 1: 크레인 씬 간 이동인지 확인
        if (CRANE_SCENES_ORDER.Contains(sceneName))
        {
            // 다른 크레인 씬으로 이동하기 전, 현재 코인 상태를 초기화합니다.
            // (이 로직은 ResetButtonHandler.cs의 PrepareForReset() 호출과 충돌하지 않도록
            // 씬 로드 전에 CraneCoinManager의 static 코인 값을 0으로 만들어야 합니다.)

            // CraneCoinManager의 static 코인 변수를 0으로 설정합니다.
            // (CraneCoinManager에 ResetPendingCoins() 함수가 있다고 가정)
            CraneCoinManager.ResetPendingCoins();
        }
        // 수정: 페이드 연출을 통해 씬 로드
        StartFadeLoad(sceneName);
    }

    // 새로 추가하는 ReloadScene 함수: 현재 씬을 재시작할 때 (코인 초기화 로직 제외)
    public void ReloadScene(string sceneName)
    {
        // ResetButtonHandler에서 이미 PrepareForReset()을 호출했으므로, 
        // 코인 초기화 로직 없이 바로 씬 로드를 시작합니다.
        Debug.Log($"현재 씬 리로드: 코인 상태 유지 ({sceneName})");
        StartFadeLoad(sceneName);
    }

    // 2D 맵으로 복귀하는 함수 (3D 씬의 복귀 버튼에 연결됨)
    public void ReturnToMapScene(string mapSceneName)
    {
        // 추가: 맵으로 복귀하기 전에 코인 상태를 0으로 초기화합니다.
        CraneCoinManager.ResetPendingCoins();
        SoundManager.Instance.PlayLoopSFX(null);
        SoundManager.Instance.StopLoopSFX();
        // SoundManager.Instance.PlayBGM(SoundManager.Instance.mapBGM);
        // 수정: 페이드 연출을 통해 씬 로드
        StartFadeLoad(mapSceneName);
    }

    private string GetNextSceneName()
    {
        string nextSceneName = "";
        foreach (string sceneName in CRANE_SCENES_ORDER)
        {
            if (GameManager.Instance.IsMachineAccessible(sceneName))
            {
                nextSceneName = sceneName;
                break;
            }
        }
        return nextSceneName;
    }

    // 버프 알림 및 씬 로드 체인을 시작합니다.
    private void StartBuffNotificationChain(string nextSceneName)
    {
        // 콜백 체인의 시작: 소바 버프 체크
        CheckAndShowSobaBuff(() =>
        {
            // 소바 알림이 끝나면 신사 버프 체크 및 실행
            CheckAndShowShrineBuff(nextSceneName);
        });
    }

    // ----------------------------------------------------
    // Step 1: 소바 버프 (골드 획득) 체크 및 알림
    // ----------------------------------------------------
    private void CheckAndShowSobaBuff(Action onComplete)
    {
        if (GameManager.Instance.hasSobaBuff && vnNotifier != null)
        {
            // 1. 보상 즉시 지급 및 버프 소멸 (알림 UI 표시 전 지급)
            // ExecuteSobaBuff는 돈을 획득하고 hasSobaBuff를 false로 만듭니다.
            // GameManager.Instance.ExecuteSobaBuff();
            // 수정: 획득 금액을 받아옵니다.
            int acquiredMoney = GameManager.Instance.ExecuteSobaBuff();

            // 2. 알림 텍스트 정의 (획득 금액을 표시하려면 ExecuteSobaBuff의 반환 값이 필요함. 
            // 여기서는 임시 텍스트 사용)
            string[] texts = new string[]
            {
            $"게임센터 입구에 {acquiredMoney:N0}엔이 떨어져있다!",
            };

            // 3. 알림 UI 표시
            vnNotifier.ShowNotification(texts, coinPileSprite, null, onComplete); // null은 GoodsData?에 해당
        }
        else
        {
            // 버프가 없으면 다음 단계 즉시 실행
            onComplete?.Invoke();
        }
    }

    // ----------------------------------------------------
    // Step 2: 신사 버프 (굿즈 획득) 체크 및 알림
    // ----------------------------------------------------
    private void CheckAndShowShrineBuff(string nextSceneName)
    {
        if (GameManager.Instance.hasShrineLuckyItemBuff && vnNotifier != null)
        {
            var gm = GameManager.Instance;
            GoodsData acquiredGoods = gm.ExecuteShrineLuckyItemBuff();

            string[] texts = new string[]
            {
            "모르는 누군가가 나에게 굿즈를 선물해주고 도망갔다...?",
            };

            vnNotifier.ShowNotification(texts, shrineImageSprite, acquiredGoods, () =>
            {
                // 알림이 끝난 시점에 최종 이동 판단
                HandleFinalSceneLoad(nextSceneName);
            });
        }
        else
        {
            // 버프가 없으면 즉시 최종 이동 판단
            HandleFinalSceneLoad(nextSceneName);
        }
    }

    // 변경점 3: 안전한 씬 로드를 보장하는 예외 처리 함수 추가
    private void HandleFinalSceneLoad(string nextSceneName)
    {
        // 만약 오늘 모든 기계를 클리어해서 갈 수 있는 크레인 씬이 없다면 ("")
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log("오늘 모든 크레인 기계를 클리어했습니다. 맵에 머무릅니다.");

            // 화면이 멈추지 않도록 현재 맵 BGM을 다시 확실히 틀어주고 로직을 안전하게 종료합니다.
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayBGM(SoundManager.Instance.mapBGM, 1.0f);
            }
            return;
        }

        // 갈 곳이 확실히 있을 때만 게임센터 BGM으로 바꾸고 페이드 로드 실행
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBGM(SoundManager.Instance.gamecenterBGM);
        }
        StartFadeLoad(nextSceneName);
    }

    // 오므라이스 VN 씬으로 이동하는 함수
    public void LoadOmuriceVNScene()
    {
        // 코인 초기화는 MapScene으로 돌아갈 때만 필요하므로 여기서는 생략
        StartFadeLoad(OMURICE_VN_SCENE);
    }

    // 여행 이벤트 전용 로드 함수
    public void LoadTravelVNScene()
    {
        StartFadeLoad(TRAVEL_VN_SCENE);
    }
}