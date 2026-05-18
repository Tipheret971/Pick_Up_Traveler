using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro; // 툴팁 핸들러에 TextMeshProUGUI가 필요할 수 있으므로 추가

public class BuffUI : MonoBehaviour
{
    [Header("Icon References")]
    [SerializeField] private GameObject iconTemplatePrefab; // BuffIconTemplate 프리팹 (BuffTooltipHandler 포함)
    [SerializeField] private Transform iconParent; // BuffIconParent (자기 자신)

    // TODO: 버프 아이콘 스프라이트를 관리하는 시스템 (VisualsManager에서 가져와야 함)
    // 현재는 임시로 Sprite 배열을 사용합니다. 실제 스프라이트를 할당하세요.
    [Header("Buff Icon Sprites")]
    [SerializeField] private Sprite resaleBuffIcon;
    [SerializeField] private Sprite discountBuffIcon;
    [SerializeField] private Sprite hormoneBuffIcon;
    [SerializeField] private Sprite sobaBuffIcon;
    [SerializeField] private Sprite omuriceBuffIcon;
    [SerializeField] private Sprite shrineDiscountIcon;
    [SerializeField] private Sprite shrineResaleIcon;
    [SerializeField] private Sprite shrineLuckyItemIcon;

    // 툴팁 패널의 전역 참조 (Awake에서 한 번 찾음)
    private GameObject globalTooltipPanel;
    private TextMeshProUGUI globalTooltipText;

    void Awake()
    {
        // 1. TooltipPanel GameObject를 씬에서 찾습니다. (이름이 'TooltipPanel'이라고 가정)
        globalTooltipPanel = GameObject.Find("TooltipPanel");

        if (globalTooltipPanel != null)
        {
            // 2. 패널의 자식에서 TextMeshPro 컴포넌트를 찾습니다.
            globalTooltipText = globalTooltipPanel.GetComponentInChildren<TextMeshProUGUI>();

            // 3. 초기 상태는 비활성화
            globalTooltipPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("BuffUI: TooltipPanel 오브젝트를 씬에서 찾을 수 없습니다. 이름이 'TooltipPanel'인지 확인하세요.");
        }
    }
    void OnEnable()
    {
        // 씬이 활성화되거나 오브젝트가 활성화될 때 업데이트
        UpdateBuffDisplay();
    }

    public void UpdateBuffDisplay()
    {
        if (GameManager.Instance == null || iconParent == null || iconTemplatePrefab == null) return;

        // 1. 이전 아이콘 모두 제거 (재활용을 위해)
        foreach (Transform child in iconParent)
        {
            Destroy(child.gameObject);
        }

        var gm = GameManager.Instance;

        // --- 2. 활성 버프 목록 및 데이터 정의 ---
        List<(Sprite icon, string description)> activeBuffData = GetActiveBuffData(gm);

        // --- 3. 아이콘 생성 및 데이터 연결 ---
        foreach (var data in activeBuffData)
        {
            // 3-1. 아이콘 오브젝트 생성
            GameObject iconObj = Instantiate(iconTemplatePrefab, iconParent);

            // 3-2. 이미지 컴포넌트에 스프라이트 설정
            Image iconImage = iconObj.GetComponent<Image>();
            if (iconImage != null)
            {
                // 스프라이트가 null이 아닐 경우에만 설정
                iconImage.sprite = data.icon != null ? data.icon : resaleBuffIcon; // 임시 기본 아이콘 할당
            }

            // 3-3. 툴팁 핸들러에 설명 연결
            BuffTooltipHandler handler = iconObj.GetComponent<BuffTooltipHandler>();
            if (handler != null)
            {
                // 핵심 수정: InitTooltip을 호출하여 패널과 텍스트 정보를 즉시 주입
                handler.InitTooltip(globalTooltipPanel, globalTooltipText);
                handler.descriptionText = data.description;
            }
            // 아이콘을 활성화
            iconObj.SetActive(true);
        }
    }

    // GameManager의 상태를 읽어 활성 버프의 데이터(아이콘, 설명) 목록을 반환합니다.
    private List<(Sprite icon, string description)> GetActiveBuffData(GameManager gm)
    {
        List<(Sprite, string)> list = new List<(Sprite, string)>();

        // 1. 리셀 배율 버프 (1.0 초과일 때만 표시)
        if (gm.resaleMultiplier > 1.0f)
        {
            float bonus = (gm.resaleMultiplier - 1.0f) * 100;
            list.Add((resaleBuffIcon, $"리셀샵이 굿즈를 {bonus:F0}% 비싸게\n매입한다고 한다!\n(다음날 소멸)"));
        }

        // 2. 할인 버프 (1.0 미만일 때만 표시)
        if (gm.priceDiscountMultiplier < 1.0f)
        {
            float discount = (1.0f - gm.priceDiscountMultiplier) * 100;
            list.Add((discountBuffIcon, $"근처 가게들이\n{discount:F0}% 할인중이다!\n(다음날 소멸)"));
        }

        // 3. 호르몬 정식 버프
        if (gm.hasHormoneBuff)
        {
            list.Add((hormoneBuffIcon, "다음 관광지는 싸게\n다녀올 수 있을 것 같다!\n(다음 관광지 이벤트시 소멸)"));
        }

        // 4. 소바 버프
        if (gm.hasSobaBuff)
        {
            list.Add((sobaBuffIcon, "게임센터에 돈이 떨어져\n있을것만 같다!\n(게임센터 입장 or 다음날 소멸)"));
        }

        // 5. 오므라이스 버프
        if (gm.hasOmuriceBuff)
        {
            list.Add((omuriceBuffIcon, "식당 밖으로 나오면\n특별한 일이 생길지도...?\n(이벤트 발생 or 다음날 소멸)"));
        }

        // 6-1. 신사 할인 버프 (40% 할인)
        if (gm.hasShrineDiscountBuff)
        {
            list.Add((shrineDiscountIcon, "근처 가게들이\n40% 할인중이다!\n(다음날 소멸)"));
        }

        // 6-2. 신사 리셀 버프 (1.5배 리셀)
        if (gm.hasShrineResaleBuff)
        {
            list.Add((shrineResaleIcon, "리셀샵이 굿즈를 50% 비싸게\n매입한다고 한다!\n(다음날 소멸)"));
        }

        // 6-3. 신사 럭키 아이템 버프 (랜덤 굿즈 획득)
        if (gm.hasShrineLuckyItemBuff)
        {
            list.Add((shrineLuckyItemIcon, "게임센터 근처에서\n좋은 일이 생길것 같다!\n(게임센터 입장 or 다음날 소멸)"));
        }
        return list;
    }
}