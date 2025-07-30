using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance = null;

    public enum EssentialAudio {
        Main,
        Combat,
    }

    [Serializable]
    public struct EssentialAudioData {
        public EssentialAudio essentialType;
        public AudioClip clip;
        public float timeBeforeLoop;
    }

    Dictionary<string, AudioClip> vecClip = new Dictionary<string, AudioClip>();

    Dictionary<int, AudioComponent> m_vecAudioComp = new Dictionary<int, AudioComponent>();
    Dictionary<EssentialAudio, AudioComponent> m_vecEssentialAudioComp = new Dictionary<EssentialAudio, AudioComponent>();
    Dictionary<string, bool> m_vecPlaying = new Dictionary<string, bool>();

    [Header("Audio Manager Setting")]
    [SerializeField] int maxComponent = 10;
    [SerializeField] float volume = 1.0f;

    [Header("Object Reference")]
    [SerializeField] GameObject prefabAudioComp = null;
    public List<EssentialAudioData> vecEssential;

    private Queue<int> m_queueAvailable = new Queue<int>();

    void Awake() {
        if (instance != null) {
            Destroy(gameObject);
            return;
        }

        instance = this;
        List<AudioClip> vecLoad = Resources.LoadAll<AudioClip>("Audio").ToList();

        foreach (AudioClip clip in vecLoad) {
            vecClip.Add(clip.name, clip);
        }

        DontDestroyOnLoad(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 1; i < maxComponent + 1; i++) {
            m_queueAvailable.Enqueue(i);
        }

        CreateEssentialComp();
    }

    public AudioComponent CreateComp() {
        if (m_queueAvailable.Count <= 0) {
            Debug.Log("Not enough audioComp");
            return null;
        }
        int index = m_queueAvailable.Dequeue();
        GameObject obj = Instantiate(prefabAudioComp);
        AudioComponent comp = obj.GetComponent<AudioComponent>();
        m_vecAudioComp.Add(index, comp);
        comp.SetVolume(volume);
        comp.index = index;

        return comp;
    }

    public void CreateEssentialComp() {
        foreach (EssentialAudioData data in vecEssential) {
            GameObject obj = Instantiate(prefabAudioComp);
            AudioComponent comp = obj.GetComponent<AudioComponent>();

            comp.SetClip(data.clip);
            comp.loopTime = data.timeBeforeLoop;

            comp.isEssential = true;
            comp.transform.SetParent(transform);


            m_vecEssentialAudioComp.Add(data.essentialType, comp);
        }
    }

    public void DestroyComp(int index) {
        m_vecPlaying.Remove(m_vecAudioComp[index].GetClipName());
        Destroy(m_vecAudioComp[index].gameObject);
        m_vecAudioComp.Remove(index);
        m_queueAvailable.Enqueue(index);
    }

    public void PlayEssential(EssentialAudio type) {
        m_vecEssentialAudioComp[type].Play();
    }

    public void StopEssential(EssentialAudio type) {
        m_vecEssentialAudioComp[type].Stop();
    }

    public void Play(string clipName) {
        if (!vecClip.ContainsKey(clipName)) {
            return;
        }

        if (m_vecPlaying.ContainsKey(clipName)) {
            return;
        }

        AudioComponent comp = CreateComp();
        if (comp == null) {
            return;
        }

        comp.SetClip(vecClip[clipName]);
        comp.Play();
        m_vecPlaying.Add(clipName, true);
    }

    public void SetVolume(float vol) {
        volume = vol;

        foreach (var pair in m_vecAudioComp) {
            pair.Value.SetVolume(vol);
        }

        foreach (var pair in m_vecEssentialAudioComp) {
            pair.Value.SetVolume(vol);
        }
    }

    public void StopAll() {
        for (int i = 0; i < m_vecAudioComp.Count(); i++) {
            DestroyComp(i);
        }
        m_vecAudioComp.Clear();

        StopEssential(EssentialAudio.Main);
        StopEssential(EssentialAudio.Combat);
    }
}
