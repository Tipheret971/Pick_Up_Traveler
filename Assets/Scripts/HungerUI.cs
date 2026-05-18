using UnityEngine;
using TMPro;

public class HungerUI : MonoBehaviour
{
    private TextMeshProUGUI hungerText;

    void Start()
    {
        hungerText = GetComponent<TextMeshProUGUI>();
        UpdateHungerDisplay();
    }

    void OnEnable()
    {
        UpdateHungerDisplay();
    }

    public void UpdateHungerDisplay()
    {
        if (GameManager.Instance == null || hungerText == null) return;

        int hunger = GameManager.Instance.currentHunger;
        //int debuff = GameManager.Instance.hungerDebuffStack;

        //string debuffStatus = (debuff > 0) ? $" (공복 {debuff}스택!)" : "";

        // 배고픔 텍스트 포맷: "배고픔: 10/10 (공복 1일차)"
        hungerText.text = $"배고픔: {hunger}/10";// debuffStatus}";

        // 옵션: 배고픔이 0이면 색상을 빨간색으로 변경하여 경고
        if (hunger == 0)
        {
            hungerText.color = Color.red;
        }
        else
        {
            hungerText.color = Color.white;
        }
    }
}