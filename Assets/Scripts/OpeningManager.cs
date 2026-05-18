using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class OpeningManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI talkText;       // 대사가 출력될 텍스트
    //public GameObject clickPrompt;         // "클릭하여 진행" 안내 (선택 사항)
    public Image backgroundImage;          // 배경 이미지 (필요 시)

    private string[] openingScripts = new string[]
    {
        "일본 한달 여행... 기대된다!",
        "......",
        "......?",
        "지갑이 어디갔지...?",
        "설마 잃어버렸나...",
        "(지갑을 찾아 헤맸지만 보이지 않는다.)",
        "여행 첫날부터 지갑을 잃어버린 당신!\n\n그런 당신의 머릿속에 스쳐 지나간 한 가지 방법!",
        "게임센터에서 굿즈를 뽑아서 되팔면 돈을 벌 수 있는데?",
        "게임센터에서 굿즈를 뽑고\n\n리셀샵에 가져다 팔아서\n\n식비와 여행비를 마련하세요!",
        "여행은 29일까지, 30일로 넘어가면 엔딩입니다!",
        "그럼 행운을 빌어요~"
    };

    private int currentIndex = 0;
    //private bool isTyping = false;

    void Start()
    {
        SoundManager.Instance.PlayBGM(null);
        ShowNextLine();
    }

    void Update()
    {
        // 마우스 클릭이나 스페이스바를 누르면 다음 대사로
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (currentIndex < openingScripts.Length)
            {
                ShowNextLine();
            }
            else
            {
                EndOpening();
            }
        }
    }

    private void ShowNextLine()
    {
        // 화면 클리어 연출 구간 (5번 인덱스 이후 등) 체크 가능
        if (currentIndex == 6)
        {
            // 여기서 배경을 바꾸거나 연출을 넣을 수 있습니다.
            Debug.Log("화면 클리어 및 분위기 전환");
        }

        talkText.text = openingScripts[currentIndex];
        currentIndex++;
    }

    private void EndOpening()
    {
        // 씬 로더를 찾아 맵 씬으로 페이드 로드
        SceneLoader loader = Object.FindFirstObjectByType<SceneLoader>();
        if (loader != null)
        {
            loader.LoadScene("Map");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Map");
        }
    }
}