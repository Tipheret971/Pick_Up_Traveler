using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class CranePrizeDisplayUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject displayPanel; // 정보창 전체 패널
    [SerializeField] private Image goodsImage;        // 굿즈 이미지 (images[1] 역할)
    [SerializeField] private Image rarityBorder;     // 희귀도 테두리 (images[2] 역할)
    [SerializeField] private TextMeshProUGUI nameText; // 굿즈 이름 (tmps[0] 역할)

    void Start()
    {
        RefreshDisplay();
    }

    public void RefreshDisplay()
    {
        if (GameManager.Instance == null) return;

        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // FirstOrDefault는 클래스인 PrizeMachineState 객체를 찾으므로 null 체크가 가능합니다.
        var state = GameManager.Instance.currentMachinePrizes
            .FirstOrDefault(s => s.SceneName == currentSceneName);

        // state가 null이 아니면 내부에 GoodsData(구조체)가 존재한다고 판단합니다.
        if (state != null)
        {
            UpdateUI(state.CurrentGoods);
        }
        else
        {
            if (displayPanel != null) displayPanel.SetActive(false);
            Debug.LogWarning($"[{currentSceneName}]에 해당하는 굿즈 데이터가 없습니다.");
        }
    }

    private void UpdateUI(GoodsData goods)
    {
        displayPanel.SetActive(true);

        // 3. VisualsManager를 통해 이미지 설정 (리셀샵 로직 복사)
        if (VisualsManager.Instance != null)
        {
            // 시리즈 이미지
            Sprite seriesSprite = VisualsManager.Instance.GetSeriesSprite(goods.Series);
            if (goodsImage != null) goodsImage.sprite = seriesSprite;

            // 테두리 이미지
            Sprite borderSprite = VisualsManager.Instance.GetRarityBorder(goods.Rarity);
            if (rarityBorder != null) rarityBorder.sprite = borderSprite;
        }

        // 4. 이름 설정
        if (nameText != null)
        {
            string seriesName = GameManager.Instance.GetSeriesName(goods.Series);
            nameText.text = $"{seriesName}";
        }
    }
}