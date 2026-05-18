using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

// IPointerEnterHandler: 커서가 버튼에 진입했을 때
// IPointerExitHandler: 커서가 버튼을 벗어났을 때
public class ButtonHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Elements")]
    // 버튼 자체의 RectTransform만 필요합니다.
    [SerializeField] private RectTransform targetRect;

    [Header("Animation Settings")]
    [SerializeField] private Vector3 targetScale = new Vector3(1.1f, 1.1f, 1f); // 커서 올렸을 때 확대될 크기
    [SerializeField] private float animationDuration = 0.1f; // 애니메이션 속도

    private Vector3 originalScale;
    private Coroutine scaleCoroutine;

    void Start()
    {
        // 버튼의 원래 크기를 저장합니다.
        if (targetRect == null)
        {
            // 스크립트가 버튼 자체에 붙어있다고 가정
            targetRect = GetComponent<RectTransform>();
        }
        originalScale = targetRect.localScale;
    }

    // --- 커서가 버튼 위에 올라왔을 때 (확대) ---
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 기존 코루틴을 중지하고 확대 애니메이션 시작
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }
        scaleCoroutine = StartCoroutine(AnimateScale(targetScale));
    }

    // --- 커서가 버튼에서 벗어났을 때 (축소/복귀) ---
    public void OnPointerExit(PointerEventData eventData)
    {
        // 기존 코루틴을 중지하고 원래 크기로 복귀 애니메이션 시작
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }
        scaleCoroutine = StartCoroutine(AnimateScale(originalScale));
    }

    // 크기 변화 애니메이션 코루틴
    private IEnumerator AnimateScale(Vector3 target)
    {
        Vector3 start = targetRect.localScale;
        float time = 0f;

        while (time < animationDuration)
        {
            // Lerp를 사용하여 부드럽게 크기를 변경합니다.
            targetRect.localScale = Vector3.Lerp(start, target, time / animationDuration);
            time += Time.deltaTime;
            yield return null;
        }
        targetRect.localScale = target; // 정확히 목표 크기로 설정하여 종료
    }
}