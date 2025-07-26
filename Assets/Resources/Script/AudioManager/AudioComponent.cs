using UnityEngine;

public class AudioComponent : MonoBehaviour
{
    AudioSource source;
    public float loopTime = 0.0f;
    public float currentTime = 0.0f;
    public bool isEssential { set; get; } = false;
    public int index { set; get; } = 0;
    public bool isActive { set; get; } = false;
    public string clipName = "";

    void Awake() {
        source = GetComponent<AudioSource>();
    }

    void Update() {
        if (source.isPlaying) {
            return;
        }

        if (_IsLoop()) {
            return;
        }

        if (!isEssential) {
            _Destroy();
        }
    }

    void FixedUpdate()
    {
        if (!isActive) {
            return;
        }

        LoopWithPauseUpdate();
    }

    void LoopWithPauseUpdate() {
        if (!_IsLoop()) {
            return;
        }

        if (source.isPlaying) {
            return;
        }

        currentTime += Time.fixedDeltaTime;
        if (currentTime > loopTime) {
            source.Play();
            currentTime = 0.0f;
        }
    }

    public void SetClip(AudioClip clip) {
        source.clip = clip;
    }

    public string GetClipName() {
        return source.clip.name;
    }

    public void Play() {
        if (source.isPlaying) {
            return;
        }

        isActive = true;
        source.Play();
    }

    public void Stop() {
        isActive = false;
        source.Stop();
    }

    public void SetVolume(float vol) {
        source.volume = vol;
    }

    void _Destroy() {
        AudioManager.instance.DestroyComp(index);
    }

    bool _IsLoop() {
        return loopTime > 0.0f;
    }
}
