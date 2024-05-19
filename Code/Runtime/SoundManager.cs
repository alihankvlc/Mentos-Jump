using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Assets._KVLC_Project_Helix_Jump.Code.Runtime
{
    [System.Serializable]
    public class Sound
    {
        [SerializeField] private int _id;
        [SerializeField] private AudioClip[] _clip;
        [SerializeField, Range(0f, 1f)] private float _volume;

#if UNITY_EDITOR
        [SerializeField, Multiline] private string _debuggingDescription;

#endif

        public AudioClip[] ClipArray => _clip;
        public int Id => _id;
        public float Volume => _volume;
    }

    public interface ISoundHandler
    {
        public void PlayOneShot(int id, bool isRandom = false);
        public void Play(int id, bool isRandom = false);
        public void Stop();
    }

    [RequireComponent(typeof(AudioSource))]
    public class SoundManager : Singleton<SoundManager>, ISoundHandler
    {
        [SerializeField] private List<Sound> _sounds = new();

        [SerializeField, ReadOnly] private Dictionary<int, Sound> _soundsDict = new();

        private AudioSource _audioSource;

        private void Start()
        {
            _soundsDict = _sounds.ToDictionary(r => r.Id);
            _audioSource = GetComponent<AudioSource>();
        }

        public void PlayOneShot(int id, bool isRandom = false)
        {
            Sound sound = GetSound(id);

            AudioClip clip = GetClip(sound, isRandom);
            _audioSource.PlayOneShot(clip, sound.Volume);
        }

        public void Play(int id, bool isRandom = false)
        {
            Sound sound = GetSound(id);
            AudioClip clip = GetClip(sound, isRandom);

            _audioSource.clip = clip;
            _audioSource.volume = sound.Volume;

            _audioSource.Play();
        }
        public void Stop()
        {
            if (_audioSource.isPlaying)
                _audioSource.Stop();
        }
        private Sound GetSound(int id)
        {
            if (_soundsDict.TryGetValue(id, out Sound existingSound))
            {
                return existingSound;
            }

            return null;
        }

        private AudioClip GetClip(Sound sound, bool isRandom = false)
        {
            AudioClip clip = isRandom ? sound.ClipArray[Random.Range(0, sound.ClipArray.Length)] : sound.ClipArray[0];
            return clip;
        }
    }
}