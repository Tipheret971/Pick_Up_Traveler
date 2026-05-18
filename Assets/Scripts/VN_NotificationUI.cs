using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System; // Action 델리게이트 사용을 위해 필요

public class VN_NotificationUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI textBox;
    [SerializeField] private Image imageDisplay;
    [SerializeField] private Button clickListenerButton; // 화면 전체를 덮는 투명 버튼 (클릭 감지용)

    [Header("Goods Display")]
    [SerializeField] private Transform goodsDisplayParent; // 굿즈 프리팹이 위치할 부모 Transform
    [SerializeField] private GameObject inventoryItemPrefab; // InventoryItemTemplate 프리팹

    // 클릭 완료 시 호출될 콜백 함수 (델리게이트)
    private Action onFinishCallback;

    // 현재 텍스트 목록의 인덱스
    private int currentTextIndex = 0;
    // 현재 표시해야 할 텍스트 목록
    private string[] currentTexts;

    // 이 UI를 싱글톤처럼 사용하거나, SceneLoader에서 FindObjectOfType으로 찾습니다.

    void Start()
    {
        // 클릭 리스너 연결: 클릭 시 텍스트를 진행하거나 종료합니다.
        if (clickListenerButton != null)
        {
            clickListenerButton.onClick.AddListener(OnScreenClicked);
        }
        // 초기에는 비활성화 상태여야 합니다.
        gameObject.SetActive(false);
    }

    /// <summary>
    /// VN 알림을 시작하고 데이터를 설정합니다.
    /// </summary>
    /// <param name="texts">순차적으로 표시할 텍스트 배열</param>
    /// <param name="image">표시할 이미지 (null일 수 있음)</param>
    /// <param name="goodsData">표시할 굿즈 데이터 (null일 수 있음)</param> // GoodsData 추가
    /// <param name="callback">모든 텍스트가 끝났을 때 호출할 함수</param>
    public void ShowNotification(string[] texts, Sprite image, GoodsData? goodsData, Action callback) // GoodsData?로 변경
    {
        currentTexts = texts;
        onFinishCallback = callback;
        currentTextIndex = 0;

        // 1. 이미지 설정
        if (imageDisplay != null)
        {
            imageDisplay.sprite = image; // 전달된 이미지를 설정 (null이면 이미지가 사라짐)
            imageDisplay.gameObject.SetActive(image != null && goodsData == null); // 수정: 굿즈 데이터가 없으면 활성화
                                                                                   // 이 이미지 오브젝트는 굿즈 프리팹이 없을 때만 활성화됩니다.
        }

        // 2. 굿즈 프리팹 표시 로직 호출
        InstantiateGoodsPrefab(goodsData); // 이제 goodsData에 따라 프리팹을 표시하거나 숨깁니다.

        // 3. UI 활성화 및 첫 텍스트 표시
        gameObject.SetActive(true);
        DisplayCurrentText();
    }

    private void InstantiateGoodsPrefab(GoodsData? goodsData)
    {
        // 이전 굿즈가 있다면 제거 (기존 로직 유지)
        foreach (Transform child in goodsDisplayParent)
        {
            Destroy(child.gameObject);
        }

        // 굿즈 데이터가 유효하고 프리팹이 연결되어 있는지 확인
        if (goodsData != null && goodsData.HasValue && inventoryItemPrefab != null)
        {
            // 1. 프리팹 인스턴스 생성 및 데이터 추출
            GameObject goodsUI = Instantiate(inventoryItemPrefab, goodsDisplayParent);
            GoodsData goods = goodsData.Value;

            // 핵심 수정 1: 생성된 오브젝트의 위치를 부모 기준 (0, 0, 0)으로 설정하여 중앙 정렬
            RectTransform goodsRect = goodsUI.GetComponent<RectTransform>();
            if (goodsRect != null)
            {
                goodsRect.localPosition = Vector3.zero;

                // 앵커 및 피벗을 중앙(Center)으로 설정 (안전성 강화)
                goodsRect.anchorMin = new Vector2(0.5f, 0.5f);
                goodsRect.anchorMax = new Vector2(0.5f, 0.5f);
                goodsRect.pivot = new Vector2(0.5f, 0.5f);
            }

            // 2. 컴포넌트 참조 (프리팹 구조에 따라 순서대로 가져옴)
            // Image 컴포넌트 획득: [0] = Button Image, [1] = ItemImage, [2] = RarityBorder (예상)
            Image[] images = goodsUI.GetComponentsInChildren<Image>(true); // 비활성화된 자식도 찾도록 설정 (안전성 강화)
            TextMeshProUGUI[] tmps = goodsUI.GetComponentsInChildren<TextMeshProUGUI>(true);

            if (VisualsManager.Instance != null)
            {
                // 3. 이미지 리소스 가져오기
                Sprite seriesSprite = VisualsManager.Instance.GetSeriesSprite(goods.Series);
                Sprite borderSprite = VisualsManager.Instance.GetRarityBorder(goods.Rarity);

                // 4. 이미지 설정: 인덱스 1과 2가 ItemImage와 RarityBorder라고 가정합니다.

                // 시리즈 이미지 설정 (인덱스 1 또는 2일 가능성 높음)
                if (images.Length > 1 && seriesSprite != null)
                {
                    // [1] 또는 [2]가 굿즈 이미지라고 가정하고 설정
                    images[1].sprite = seriesSprite;
                    images[1].color = Color.white; // 투명도 확실히 1로 설정
                }

                // 희귀도 테두리 설정 (시리즈 이미지와 다른 인덱스)
                if (images.Length > 2 && borderSprite != null)
                {
                    // [2] 또는 [1]이 테두리라고 가정하고 설정
                    images[2].sprite = borderSprite;
                    images[2].color = Color.white; // 투명도 확실히 1로 설정
                }
            }

            // 5. 텍스트 업데이트 (이름과 가격 TMP가 순서대로 있다고 가정)
            if (tmps.Length > 0)
            {
                string seriesName = GameManager.Instance.GetSeriesName(goods.Series);

                // tmps[0]이 굿즈 이름/희귀도 텍스트라고 가정
                tmps[0].text = $"{seriesName}\n({goods.Rarity})";
                tmps[0].color = Color.black; // 텍스트 색상 확실히 설정
            }

            // 6. 불필요한 요소 비활성화/조정 (옵션)
            // 획득 알림에서는 버튼 기능이 필요 없으므로 비활성화
            // Button btn = goodsUI.GetComponent<Button>();
            // if (btn != null)
            // {
            //     btn.interactable = false;
            // }

            // 굿즈 부모 오브젝트 활성화 (혹시 모를 상황 대비)
            goodsDisplayParent.gameObject.SetActive(true);
        }
        else
        {
            // 굿즈 데이터가 없으면 굿즈 표시 영역 비활성화
            goodsDisplayParent.gameObject.SetActive(false);
        }
    }

    // 텍스트 박스에 현재 인덱스의 텍스트를 표시
    private void DisplayCurrentText()
    {
        if (textBox != null && currentTexts != null && currentTextIndex < currentTexts.Length)
        {
            textBox.text = currentTexts[currentTextIndex];
        }
    }

    // 마우스 클릭(버튼 클릭) 시 호출
    private void OnScreenClicked()
    {
        currentTextIndex++;

        if (currentTexts != null && currentTextIndex < currentTexts.Length)
        {
            // 텍스트 목록이 남아 있으면 다음 텍스트 표시
            DisplayCurrentText();
        }
        else
        {
            // 모든 텍스트가 끝났으므로 UI를 숨기고 콜백 호출
            gameObject.SetActive(false);
            onFinishCallback?.Invoke();
        }
    }
}