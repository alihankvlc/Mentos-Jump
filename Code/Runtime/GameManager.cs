using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using Cinemachine;
using DG.Tweening;
using Zenject;

namespace Assets._KVLC_Project_Helix_Jump.Code.Runtime
{
    public enum GameStatus
    {
        None,
        Loading,
        Start,
        Stop,
        Revive,
        GameOver,
        Restart,
    }

    [Serializable]
    public class ChangedPlayerScore : UnityEvent<int> { }

    [Serializable]
    public class ChangedPlayerScoreMultiplier : UnityEvent<float> { }

    public interface IGameControl
    {
        GameStatus Status { get; }
        void UpdateGameStatus(GameStatus status);
    }

    public class GameManager : Singleton<GameManager>, IGameControl
    {
        [SerializeField] private GameStatus _statusInfo;
        [SerializeField] private CinemachineVirtualCamera _virtualCamera;

        private const int _gameOverZoomCameraAmount = 50;
        private const float _restartGameDuration = 5f;
        private const float _spawnProtectionDuration = 5f;

        private bool _isSpawnProtection;

        private float _spawnProtectionTimer;
        private float _restartGameTimer;

        [Inject] private ISoundHandler _soundHandler;
        [Inject] private ISaveProvider _saveProvider;
        [Inject] private IUIHandler _uiHandler;

        [SerializeField] private ChangedPlayerScore _onChangeScore = new ChangedPlayerScore();
        [SerializeField] private ChangedPlayerScore _onChangedHighScore = new ChangedPlayerScore();
        [SerializeField] private ChangedPlayerScoreMultiplier _onChangedComboMultiplier = new ChangedPlayerScoreMultiplier();
        [SerializeField] private ChangedPlayerScoreMultiplier _onChangedScoreMultiplier = new ChangedPlayerScoreMultiplier();

        public static event Action<GameStatus> OnChangeGameStatus;

        public GameStatus Status
        {
            get => _statusInfo;
            private set => _statusInfo = value;
        }

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            HandleCursorState();
            HandleGameStatusUpdates();
        }

        private void Initialize()
        {
            _virtualCamera ??= FindObjectOfType<CinemachineVirtualCamera>();

            NotifyPlayerScore.OnScoreChanged += NotifyPlayerScore_OnScoreChanged;
            NotifyPlayerScore.OnHighScoreChanged += NotifyPlayerScore_OnHighScoreChanged;
            NotifyPlayerScore.OnScoreMultiplierChanged += NotifyPlayerScore_OnScoreMultiplierChanged;
            NotifyPlayerScore.OnComboMultiplierChanged += NotifyPlayerScore_OnComboMultiplierChanged;

            _restartGameTimer = _restartGameDuration;
        }

        private void HandleCursorState()
        {
            Cursor.lockState = Status != GameStatus.GameOver ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = Status != GameStatus.GameOver ? false : true;
        }

        private void HandleGameStatusUpdates()
        {
            if (Input.GetMouseButtonDown(0) && Status != GameStatus.GameOver && Status != GameStatus.Revive)
                UpdateGameStatus(GameStatus.Start);

            if (_isSpawnProtection)
                HandleSpawnProtection();

            if (Status == GameStatus.GameOver)
                HandleGameOver();
        }

        private void HandleSpawnProtection()
        {
            _spawnProtectionTimer -= Time.deltaTime;

            if (_spawnProtectionTimer <= 0)
            {
                Status = GameStatus.Start;

                _isSpawnProtection = false;
                _spawnProtectionTimer = _spawnProtectionDuration;
            }
        }

        private void HandleGameOver()
        {
            _restartGameTimer -= Time.deltaTime;
            _uiHandler.UpdateGameOverTimer(_restartGameTimer, _restartGameDuration);

            if (_restartGameTimer <= 0)
            {
                UpdateGameStatus(GameStatus.Restart);
                _restartGameTimer = _restartGameDuration;
            }
        }

        public void StartGame()
        {
        }

        private void StopGame()
        {
        }

        public void RestartGame()
        {
            _saveProvider.SaveHighScore(NotifyPlayerScore.Instance.HighScore);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void Loading()
        {
        }

        public void GameOver()
        {
            _saveProvider.SaveHighScore(NotifyPlayerScore.Instance.HighScore);
            _virtualCamera.m_Lens.FieldOfView = _gameOverZoomCameraAmount;
            _soundHandler.PlayOneShot(5, true);
        }

        public void Revive()
        {
            Status = GameStatus.Revive;

            _restartGameTimer = _restartGameDuration;
            _spawnProtectionTimer = _spawnProtectionDuration;
            _isSpawnProtection = true;
        }

        public void UpdateGameStatus(GameStatus status)
        {
            if (status == GameStatus.GameOver && _isSpawnProtection) return;

            Status = status;
            OnChangeGameStatus?.Invoke(status);

            switch (status)
            {
                case GameStatus.Loading:
                case GameStatus.Start:
                    StartGame();
                    break;
                case GameStatus.Stop:
                    StopGame();
                    break;
                case GameStatus.GameOver:
                    GameOver();
                    break;
                case GameStatus.Restart:
                    RestartGame();
                    break;

            }
        }

        private void NotifyPlayerScore_OnScoreChanged(object sender, ScoreChangedEventArgs<int> e)
        {
            _onChangeScore?.Invoke(e.GetValue);
        }

        private void NotifyPlayerScore_OnHighScoreChanged(object sender, ScoreChangedEventArgs<int> e)
        {
            _onChangedHighScore?.Invoke(e.GetValue);
        }

        private void NotifyPlayerScore_OnScoreMultiplierChanged(object sender, ScoreChangedEventArgs<float> e)
        {
            _onChangedScoreMultiplier?.Invoke(e.GetValue);
        }

        private void NotifyPlayerScore_OnComboMultiplierChanged(object sender, ScoreChangedEventArgs<float> e)
        {
            _onChangedComboMultiplier?.Invoke(e.GetValue);
        }

        private void OnDestroy()
        {
            NotifyPlayerScore.OnScoreChanged -= NotifyPlayerScore_OnScoreChanged;
            NotifyPlayerScore.OnHighScoreChanged -= NotifyPlayerScore_OnHighScoreChanged;
            NotifyPlayerScore.OnScoreMultiplierChanged -= NotifyPlayerScore_OnScoreMultiplierChanged;
            NotifyPlayerScore.OnComboMultiplierChanged -= NotifyPlayerScore_OnComboMultiplierChanged;
        }
    }
}
