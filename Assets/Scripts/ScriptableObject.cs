using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TravelEvent", menuName = "VN/TravelEventData")]
public class TravelEventData : ScriptableObject
{
    public int eventDay;
    public string locationName;

    [Header("Costs")]
    public int transportCost;
    public int specialOptionCost;

    [Header("Conditions")]
    public int requiredAffection;

    [Header("Rewards")] // 추가됨
    public int normalAffectionGain;
    public int specialAffectionGain;

    [Header("Visuals")]
    public Sprite heroineCostume; // 추가됨.

    [Header("Dialogues")]
    public List<DialogueSection> normalRoute;
    public List<DialogueSection> specialRoute;
}

[System.Serializable]
public struct DialogueSection
{
    public string speaker;
    [TextArea(3, 10)] public string line;
    public Sprite backgroundImage; // 이번에 추가: 해당 대사에서 보여줄 배경
    public Sprite characterSprite;    // 이번에 추가: 해당 대사의 히로인 자세/표정
    public Sprite cgIllustration; // 서비스 씬 일러스트 (있을 경우에만 할당)
    public bool isSpecialMoment; // 이 대사에서 서비스 씬을 띄울 것인지 여부
    public bool useFade;
    [Header("Day 24 Special Branch")]
    public bool checkAffectionBranch; // 이 대사 이후 호감도 160 체크를 할 것인가?
    // 추가: 이 대사가 시작될 때 페이드 연출을 사용할지 여부
    [Header("Sound Settings")]
    public AudioClip bgmClip; // 이 대사에서 바뀔 BGM (비어있으면 유지)
    public float bgmVolume; // BGM 볼륨 조절용
    [Header("SFX Settings")]
    public AudioClip sfxClip; // 이 대사에서 한 번 재생할 효과음
    [Range(0, 1)] public float sfxVolume;
}