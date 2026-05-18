using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

public class FadeController : MonoBehaviour
{
    private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f; // 페이드 한 방향에 걸리는 시간

    void Start()
    {
        fadeImage = GetComponent<Image>();
        if (fadeImage == null)
        {
            Debug.LogError("FadeController: Image 컴포넌트를 찾을 수 없습니다.");
            enabled = false;
            return;
        }

        // 씬이 로드되자마자 검은 화면에서 밝아지도록 페이드 아웃 시작
        StartCoroutine(FadeOutOnStart());
    }

    // 씬이 로드되자마자 자동으로 실행되는 페이드 아웃 시퀀스
    private IEnumerator FadeOutOnStart()
    {
        // 1. 초기 상태를 검은색(알파 1.0)으로 설정 (Inspector에서 미리 설정해야 함)
        // 만약 Inspector에서 설정하지 않았다면 코드로 설정:
        // Color startColor = fadeImage.color; startColor.a = 1.0f; fadeImage.color = startColor;

        // 2. Fade Out (검정 -> 투명) 시작
        yield return StartCoroutine(Fade(0.0f, fadeDuration)); // 목표 알파 0.0으로

        // 3. 페이드 아웃 후 필요하다면 스크립트 비활성화 (성능 최적화)
        // gameObject.SetActive(false);
    }

    // 외부(버튼 클릭 등)에서 호출하여 씬 이동을 준비하는 페이드 인 함수
    public void FadeInAndExecute(Action onFadeComplete)
    {
        // 현재 실행 중인 모든 페이드 코루틴 중지
        StopAllCoroutines();
        StartCoroutine(FadeInSequence(onFadeComplete));
    }

    private IEnumerator FadeInSequence(Action onFadeComplete)
    {
        // 1. Fade In (투명 -> 검정) 시작
        yield return StartCoroutine(Fade(1.0f, fadeDuration)); // 목표 알파 1.0 (검정)으로 이동

        // 2. 콜백 실행 (날짜 변경 및 씬 로드)
        if (onFadeComplete != null)
        {
            onFadeComplete.Invoke();
        }
    }

    // 실제 투명도를 부드럽게 조절하는 코루틴
    private IEnumerator Fade(float targetAlpha, float duration)
    {
        // ... (기존 Fade 로직) ...
        float startAlpha = fadeImage.color.a;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);

            Color color = fadeImage.color;
            color.a = newAlpha;
            fadeImage.color = color;

            yield return null;
        }

        Color finalColor = fadeImage.color;
        finalColor.a = targetAlpha;
        fadeImage.color = finalColor;
    }

    // 대사 중간 연출용: 완전히 검어질 때까지만 기다리는 코루틴
    public IEnumerator FadeInCoroutine()
    {
        yield return StartCoroutine(Fade(1.0f, fadeDuration));
    }

    // 대사 중간 연출용: 다시 밝아질 때까지만 기다리는 코루틴
    public IEnumerator FadeOutCoroutine()
    {
        yield return StartCoroutine(Fade(0.0f, fadeDuration));
    }
}