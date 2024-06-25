using UnityEngine;
using UnityEngine.Video;

public class VIdeoPlayerController : MonoBehaviour
{
    private VideoPlayer videoPlayer;

    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        Invoke(nameof(PlayVideo), 1f);
    }

    private void PlayVideo()
    {
        videoPlayer.Play();
    }
}
