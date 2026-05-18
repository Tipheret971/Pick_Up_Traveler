using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic; // FindFirstObjectByType 사용 시 UnityEngine 네임스페이스 내에 있음

// Reset Button에 부착하여 코인 상태를 보존하고 씬을 리로드하는 역할
public class ResetButtonHandler : MonoBehaviour
{
    // SceneLoader의 LoadScene 함수는 코루틴 딜레이를 사용하지 않으므로, 
    // 여기서는 SceneLoader 인스턴스를 찾아 LoadScene을 호출하도록 수정합니다.

    public void HandleResetClick()
    {
        // 1. CraneCoinManager를 찾아 코인 상태를 임시 저장
        CraneCoinManager coinManager = UnityEngine.Object.FindFirstObjectByType<CraneCoinManager>();
        if (coinManager != null)
        {
            coinManager.PrepareForReset(); // 코인 개수를 pendingCoins에 저장
        }

        // 2. SceneLoader 인스턴스를 찾습니다.
        SceneLoader sceneLoader = UnityEngine.Object.FindFirstObjectByType<SceneLoader>();

        if (sceneLoader != null)
        {
            string currentSceneName = SceneManager.GetActiveScene().name;

            // 핵심 수정: ReloadScene 함수를 호출합니다!
            sceneLoader.ReloadScene(currentSceneName);
        }
        else
        {
            Debug.LogError("SceneLoader 인스턴스를 찾을 수 없습니다! 씬을 바로 로드합니다.");
            // 비상시 페이드 연출 없이 바로 씬 로드
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}