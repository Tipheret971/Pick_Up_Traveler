using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("Looping SFX")]
    public AudioSource loopSfxSource; // 루프 전용 오디오 소스 (인스펙터에서 할당)

    [Header("BGM Fade Settings")]
    public float fadeDuration = 0.3f;

    [Header("Common Clips")]
    public AudioClip titleBGM;
    public AudioClip mapBGM;
    public AudioClip gamecenterBGM;
    public AudioClip shrineBGM;
    public AudioClip dayBGM;
    public AudioClip nightBGM;
    public AudioClip travelBGM;
    public AudioClip loveBGM;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- 배경음(BGM) 재생 함수 --- PlsyBGM(Bgm 이름, 음량, 반복여부)
    public void PlayBGM(AudioClip clip, float targetVolume = 1.0f, bool loop = true)
    {
        // 동일한 클립이더라도 처음부터 새로 재생하고 싶으시다면 
        // 아래의 if(bgmSource.clip == clip) 체크 로직을 삭제하시면 됩니다.
        StartCoroutine(FadeAndPlayNewBGM(clip, targetVolume, loop));
    }

    private IEnumerator FadeAndPlayNewBGM(AudioClip clip, float targetVolume, bool loop)
    {
        // 1. 현재 재생 중인 음악이 있다면 페이드 아웃 (볼륨 0으로)
        if (bgmSource.isPlaying)
        {
            float startVol = bgmSource.volume;
            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                bgmSource.volume = Mathf.Lerp(startVol, 0, t / fadeDuration);
                yield return null;
            }
            bgmSource.Stop();
        }

        // 2. 새로운 곡으로 교체 및 재생 시작
        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.volume = 0; // 0인 상태에서 시작

        if (clip != null)
        {
            bgmSource.Play();

            // 3. 페이드 인 (볼륨을 타겟 음량까지 올림)
            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                bgmSource.volume = Mathf.Lerp(0, targetVolume, t / fadeDuration);
                yield return null;
            }
            bgmSource.volume = targetVolume;
        }
    }

    // --- 효과음(SFX) 재생 함수 ---
    public void PlaySFX(AudioClip clip, float volume = 1.0f)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    // 루프 효과음 시작
    public void PlayLoopSFX(AudioClip clip, float volume = 1.0f)
    {
        if (loopSfxSource.clip == clip && loopSfxSource.isPlaying) return;

        loopSfxSource.clip = clip;
        loopSfxSource.loop = true; // 루프 설정
        loopSfxSource.volume = volume;
        loopSfxSource.Play();
    }

    // 루프 효과음 중단
    public void StopLoopSFX()
    {
        loopSfxSource.Stop();
        loopSfxSource.clip = null;
    }
}