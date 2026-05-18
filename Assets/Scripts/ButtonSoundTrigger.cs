using UnityEngine;
using UnityEngine.UI;

public class ButtonSoundTrigger : MonoBehaviour
{
    public AudioClip clickSFX; // 버튼 클릭 시 날 소리

    void Start()
    {
        // 이 오브젝트(또는 자식들)에 있는 버튼을 찾아 기능을 연결합니다.
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(PlaySound);
        }
    }

    public void PlaySound()
    {
        if (SoundManager.Instance != null && clickSFX != null)
        {
            SoundManager.Instance.PlaySFX(clickSFX);
        }
    }
}