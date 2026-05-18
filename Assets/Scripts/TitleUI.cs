using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TitleUI : MonoBehaviour
{
    [SerializeField] private Button continueButton;
    // [SerializeField] private string mapSceneName = "Map"; // 이동할 맵 씬 이름

    void Start()
    {
        // StartCoroutine을 사용하여 한 프레임 쉰 뒤 재생합니다.
        StartCoroutine(PlayBGMDeferred());
        // 세이브 파일이 있을 때만 이어하기 버튼 활성화
        if (continueButton != null)
        {
            // GameManager.Instance가 없을 수도 있으므로 안전하게 파일 존재 여부 직접 체크도 가능
            string path = Application.persistentDataPath + "/savefile.json";
            continueButton.interactable = System.IO.File.Exists(path);
        }
    }

    private IEnumerator PlayBGMDeferred()
    {
        // SoundManager가 인스턴스화 될 때까지 한 프레임 대기
        yield return null;

        if (SoundManager.Instance != null && SoundManager.Instance.titleBGM != null)
        {
            Debug.Log("타이틀 BGM 지연 재생 시작");
            SoundManager.Instance.PlayBGM(SoundManager.Instance.titleBGM, 0.4f);
        }
    }

    public void OnClickNewGame()
    {
        if (GameManager.Instance != null)
        {
            // 1. 배율만 남기고 데이터 초기화
            GameManager.Instance.ResetDataForNewGame();

            // 2. 초기화된 상태를 일단 한 번 저장 (선택 사항)
            GameManager.Instance.SaveGame();
        }

        // 3. 첫 번째 맵 씬으로 이동 (SceneLoader 활용)
        LoadSceneWithFade("OpeningScene");
    }

    public void OnClickContinue()
    {
        if (GameManager.Instance != null)
        {
            // 1. 먼저 세이브 파일을 읽어와 데이터를 메모리에 복구합니다.
            bool loadSuccess = GameManager.Instance.LoadGame();

            if (loadSuccess)
            {
                Debug.Log("세이브 데이터 로드 성공!");

                // 2. 이어하기이므로 뉴게임 플래그는 꺼줍니다.
                GameManager.Instance.isNewGameStarting = false;

                // 3. 데이터를 품은 채로 맵 씬으로 이동합니다.
                SceneLoader sceneLoader = UnityEngine.Object.FindFirstObjectByType<SceneLoader>();
                if (sceneLoader != null) sceneLoader.LoadScene("Map");
                else UnityEngine.SceneManagement.SceneManager.LoadScene("Map");
            }
            else
            {
                Debug.LogError("세이브 파일 로드 실패!");
            }
        }
    }

    // 핵심: SceneLoader의 페이드 기능을 활용하는 함수
    private void LoadSceneWithFade(string sceneName)
    {
        SceneLoader loader = UnityEngine.Object.FindFirstObjectByType<SceneLoader>();

        if (loader != null)
        {
            // SceneLoader에 이미 만들어둔 StartFadeLoad는 private이므로 
            // 외부에서 호출 가능한 ReturnToMapScene이나 LoadScene을 활용합니다.
            loader.LoadScene(sceneName);
        }
        else
        {
            // 만약 타이틀 씬에 SceneLoader가 없다면 직접 로드
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }

    public void OnClickQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}