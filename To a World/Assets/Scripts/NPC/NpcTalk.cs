using UnityEngine;

public class NpcTalk : MonoBehaviour
{
    private Animator _anim;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        NPCChatSystem.NPCChatManager.OnAudioPlaybackStateChanged += AudioStateChanged;
    }

    private void OnDisable()
    {
        if (NPCChatSystem.NPCChatManager != null)
        {
            NPCChatSystem.NPCChatManager.OnAudioPlaybackStateChanged -= AudioStateChanged;
        }
    }

    private void AudioStateChanged(bool isPlaying)
    {
        _anim.SetBool("Talk", isPlaying);
    }
}
