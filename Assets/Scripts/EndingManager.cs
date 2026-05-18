using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class EndingManager : MonoBehaviour
{
    [Header("UI References")]
    public Image endingIllustration;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descText;
    public CanvasGroup uiCanvasGroup;
    public Button returnButton;

    [Header("Special Cutscene")]
    public GameObject tsukinagaPhotoObject; // 츠키나가의 카톡/사진 UI
    // public Sprite tsukinagaMessageSprite;

    [Header("Ending Sprites")]
    public Sprite badEndingSprite;        // 공복
    public Sprite normalReturnSprite;     // 5만엔 미만
    public Sprite firstClassReturnSprite; // 5만엔 이상
    public Sprite richEndingSprite;       // 50만엔 이상

    public AudioClip dm;

    void Start()
    {
        returnButton.gameObject.SetActive(false);
        tsukinagaPhotoObject.SetActive(false);
        uiCanvasGroup.alpha = 0;
        SoundManager.Instance.PlayBGM(null);

        DetermineAndShowEnding();
    }

    private void DetermineAndShowEnding()
    {
        Debug.Log("엔딩 결정 로직 시작");
        var gm = GameManager.Instance;

        // 중요: 데이터를 초기화하기 전에 연출용 조건을 미리 백업합니다.
        bool backupTsukinagaSpecial = gm.isTsukinagaSpecialEnded;
        int backupDay24Choice = gm.day24Choice;

        string title = "";
        string desc = "";
        Sprite sprite = null;

        // 1. 공복 엔딩 (초기화 하지 않음 - 다시 로드해서 밥 먹을 기회 제공)
        if (gm.hungerDebuffStack >= 2)
        {
            title = "BAD ENDING: 기력 소진";
            desc = "제대로 먹지 못한 채 무리했던 여정은 여기서 끝납니다. 다음엔 꼭 밥을 챙겨 드세요.";
            sprite = badEndingSprite;
        }
        // 2. 정상 30일차 종료 (여기서부터는 데이터 초기화 진행)
        else if (gm.gameDay >= 30)
        {
            if (gm.money >= 500000) // 50만엔 이상
            {
                title = "LEGEND ENDING: 리셀의 제왕";
                desc = "당신은 크레인 게임과 굿즈 리셀에 천부적인 재능을 가졌네요!\n서울 한복판에 집을 구할 수 있겠네요! 화이팅~\n(세이브 데이터가 1일차로 초기화 됩니다)";
                sprite = richEndingSprite;

                gm.ResetDataForNewGame();
                gm.SaveGame();
            }
            else if (gm.money >= 50000) // 5만엔 이상
            {
                title = "GOLD ENDING: 퍼스트 클래스 귀국";
                desc = "굿즈 리셀이 천직인 당신!\n다음 회차부터 굿즈 가격이 2배가 됩니다.\n(세이브 데이터가 1일차로 초기화 됩니다)";
                sprite = firstClassReturnSprite;

                // 1) 배율 먼저 증가
                gm.globalPriceMultiplier *= 2.0f;
                // 2) 증가한 배율을 포함하여 데이터 초기화 (ResetData 내부에서 배율 보존 로직 확인 필수)
                gm.ResetDataForNewGame();
                // 3) 초기화된 1일차 상태 저장
                gm.SaveGame();
            }
            else // 5만엔 미만
            {
                title = "NORMAL ENDING: 무사 귀가";
                desc = "평범하지만 즐거웠던 한 달간의 여정을 마치고 한국으로 돌아갑니다.\n좋은 추억이었습니다.\n(세이브 데이터가 1일차로 초기화 됩니다)";
                sprite = normalReturnSprite;

                gm.ResetDataForNewGame();
                gm.SaveGame();
            }
        }

        Debug.Log($"선택된 엔딩: {title}");

        // 백업해둔 로컬 변수를 코루틴으로 넘겨줍니다.
        StartCoroutine(EndingSequence(title, desc, sprite, backupTsukinagaSpecial, backupDay24Choice));
    }

    // 매개변수에 백업된 데이터 추가
    IEnumerator EndingSequence(string title, string desc, Sprite sprite, bool isSpecialEnded, int day24Choice)
    {
        endingIllustration.sprite = sprite;
        yield return new WaitForSeconds(1f);

        titleText.text = title;
        descText.text = desc;

        while (uiCanvasGroup.alpha < 1)
        {
            uiCanvasGroup.alpha += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(3f);

        // GameManager 대신 백업된 변수로 체크합니다.
        if (isSpecialEnded && day24Choice == 1)
        {
            yield return StartCoroutine(ShowTsukinagaSpecial());
        }

        returnButton.gameObject.SetActive(true);
    }

    IEnumerator ShowTsukinagaSpecial()
    {
        // 츠키나가 사진/메시지 연출
        tsukinagaPhotoObject.SetActive(true);
        // 간단한 팝업 효과음이나 애니메이션이 있으면 좋습니다.
        SoundManager.Instance.PlaySFX(dm);
        Debug.Log("츠키나가로부터 연락이 왔습니다!");
        yield return new WaitForSeconds(4f);
    }

    public void OnClickReturnToTitle()
    {
        SceneManager.LoadScene("TitleScene");
    }
}