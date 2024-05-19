using System;
using UnityEngine;
using System.Collections.Generic;
using Zenject;
using DG.Tweening;

namespace Assets._KVLC_Project_Helix_Jump.Code.Runtime
{
    public interface IPlayerScoreProvider
    {
        void UpdateScore();
    }

    public class NotifyPlayerScore : Singleton<NotifyPlayerScore>, IPlayerScoreProvider
    {
        [SerializeField] private float _comboTimerDuration = 0.75f;
        [SerializeField] private GameObject _fireworks; // geçici
        [SerializeField] private GameObject _burningVFX; // geçici
        [SerializeField] private GameObject _ballObject;

        [SerializeField] private List<MeshRenderer> _meshRenderer = new();

        private int _score;
        private int _highScore;

        [Inject] private ISoundHandler _soundHandler;
        [Inject] private ISaveProvider _saveProvider;
        [Inject] private IUIHandler _uiManager;

        private float _comboMultiplier;
        private float _scoreMultiplier;

        private float _comboTimer;

        private bool _isIncreaseMultiplier;
        private bool _isHasHighScore;

        private const int _defaultScore = 3;
        private const int _defaultScoreMultiplier = 0;

        public static event EventHandler<ScoreChangedEventArgs<int>> OnScoreChanged;
        public static event EventHandler<ScoreChangedEventArgs<int>> OnHighScoreChanged;
        public static event EventHandler<ScoreChangedEventArgs<float>> OnComboMultiplierChanged;
        public static event EventHandler<ScoreChangedEventArgs<float>> OnScoreMultiplierChanged;

        [SerializeField]
        public int Score
        {
            get => _score;
            private set
            {
                Action(ref _score, value, OnScoreChangedEvent);
                _uiManager?.UpdateScore(_score, _isHasHighScore);
            }
        }

        [SerializeField]
        public int HighScore
        {
            get => _highScore;
            private set
            {
                Action(ref _highScore, value, OnHighScoreChangedEvent);
                _uiManager?.UpdateHighScore(_highScore);
            }
        }

        [SerializeField]
        public float ComboMultiplier
        {
            get => _comboMultiplier;
            private set
            {
                Action(ref _comboMultiplier, value, OnComboMultiplierChangedEvent);
                _uiManager?.UpdateComboMultiplier(_comboMultiplier);
            }
        }

        [SerializeField]
        public float ScoreMultiplier
        {
            get => _scoreMultiplier;
            private set
            {
                Action(ref _scoreMultiplier, value, OnScoreMultiplierChangedEvent);
                _uiManager?.UpdateScoreMultiplier(_scoreMultiplier);
            }
        }

        private void OnScoreChangedEvent() => OnScoreChanged?.Invoke(this, new ScoreChangedEventArgs<int>(Score));
        private void OnHighScoreChangedEvent() => OnHighScoreChanged?.Invoke(this, new ScoreChangedEventArgs<int>(HighScore));
        private void OnComboMultiplierChangedEvent() => OnComboMultiplierChanged?.Invoke(this, new ScoreChangedEventArgs<float>(ComboMultiplier));
        private void OnScoreMultiplierChangedEvent() => OnScoreMultiplierChanged?.Invoke(this, new ScoreChangedEventArgs<float>(ScoreMultiplier));

        private void Start()
        {
            _saveProvider.LoadHighScore(ref _highScore);
            HighScore = _highScore;
        }

        private void Update()
        {

            if (GameManager.Instance.Status == GameStatus.GameOver)
            {
                _meshRenderer.ForEach(r => r.material.DOColor(Color.white, 0.5f));
                _burningVFX.SetActive(false);
                return;
            }

            CheckHighScore();

            if (_isIncreaseMultiplier)
                Combo();
        }

        public void UpdateScore()
        {
            if (ComboMultiplier >= 1)
            {
                Score = (_isHasHighScore) ? (HighScore += _defaultScore * Mathf.FloorToInt(_scoreMultiplier)) : (Score += _defaultScore * Mathf.FloorToInt(_scoreMultiplier));
            }

            ScoreMultiplier = _isIncreaseMultiplier ? ScoreMultiplier + 1 : _defaultScoreMultiplier + 1;

            if (Mathf.FloorToInt(ScoreMultiplier) % 5 == 0)
            {
                ComboMultiplier++;
                ScoreMultiplier = 0;
                _soundHandler?.PlayOneShot(1, true);
            }

            _isIncreaseMultiplier = true;
            _comboTimer = 0f;
        }

        private void CheckHighScore()
        {
            if (!_isHasHighScore && _score >= _highScore && _score != 0)
            {
                _fireworks?.SetActive(true);
                _soundHandler?.PlayOneShot(3, true);
                _soundHandler?.PlayOneShot(4, true);

                _meshRenderer.ForEach(r => r.material.DOColor(Color.red, 2f));
                _burningVFX.SetActive(true);

                _uiManager?.UpdateScore(_score, true);
                _isHasHighScore = true;
            }
        }
        private void Combo()
        {
            _comboTimer += Time.deltaTime;

            if (_comboTimer >= _comboTimerDuration)
            {
                if (ScoreMultiplier <= 0)
                {
                    if (ComboMultiplier < 1)
                    {
                        ScoreMultiplier = 0;
                        _comboTimer = 0;
                        _isIncreaseMultiplier = false;
                    }
                    else
                    {
                        ScoreMultiplier = 5;
                        ComboMultiplier--;
                    }
                }

                if (ScoreMultiplier > 0)
                    ScoreMultiplier = Mathf.MoveTowards(ScoreMultiplier, 0, Time.deltaTime);
            }
        }

        private void Action<T>(ref T field, T value, Action eventInvoker) where T : struct
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                eventInvoker?.Invoke();
            }
        }

        private void OnApplicationQuit()
        {
            _saveProvider.SaveHighScore(_highScore);
        }
    }
}
