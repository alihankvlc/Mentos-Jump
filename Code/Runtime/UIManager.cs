using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Assets._KVLC_Project_Helix_Jump.Code.Runtime
{
    public interface IUIHandler
    {
        public void UpdateScore(int score, bool isHasHighScore);
        public void UpdateHighScore(int highScore);
        public void UpdateScoreMultiplier(float multiplier);
        public void UpdateComboMultiplier(float multiplier);
        public void UpdateGameOverTimer(float timer, float duration);
    }

    public class UIManager : Singleton<UIManager>, IUIHandler
    {
        [Header("UI Score Settings")]
        [SerializeField] private TextMeshProUGUI _scoreTextMesh;
        [SerializeField] private TextMeshProUGUI _scoreMultiplierTextMesh;
        [SerializeField] private TextMeshProUGUI _highScoreTextMesh;
        [SerializeField] private GameObject _highScoreFx;

        [Header("UI Score Lock Settings")]
        [SerializeField] private Image _lock;
        [SerializeField] private Sprite _lockICON;
        [SerializeField] private Sprite _unLockICON;

        [Header("UI Score Multiplier Slider Settings")]
        [SerializeField] private GameObject _multiplierSlider;
        [SerializeField] private Image _multiplierImage;

        [Header("UI GameStatus Settings ")]
        [SerializeField] private GameObject _restartGameWindowObject;
        [SerializeField] private TextMeshProUGUI _informationTextMesh;
        [SerializeField] private TextMeshProUGUI _restartGameTimerTextMesh;
        [SerializeField] private Slider _restartGameSlider;

        [Inject] IGameControl _gameControl;

        private void Start()
        {
            GameManager.OnChangeGameStatus += ShowGameStatus;
            _lock.color = Color.red;
        }

        public void UpdateScore(int score, bool isHasHighScore)
        {
            _scoreTextMesh.SetText(score.ToString());
            _scoreTextMesh.color = isHasHighScore ? Color.blue : Color.white;
            _highScoreFx.SetActive(isHasHighScore);
        }

        public void UpdateHighScore(int highScore)
        {
            if (highScore > 0)
            {
                string info = $"High Score: <color=white>{highScore}</color>";
                _highScoreTextMesh.SetText(info);

                if (_highScoreTextMesh != null)
                {
                    _highScoreTextMesh.transform.DOScale(1f, 0.25f);
                }
            }
        }
        private void Update()
        {
            _restartGameWindowObject.SetActive(_gameControl.Status == GameStatus.GameOver);
        }
        #region UI_Player_Score
        public void UpdateScoreMultiplier(float multiplier)
        {
            float tempMultiplier = Mathf.Clamp(multiplier / 10f * 2, 0f, 1f);
            _multiplierImage.color = tempMultiplier < 0.2f ? Color.red : Color.white;

            if (_multiplierSlider != null)
            {
                _multiplierSlider.transform.DOScaleX(Mathf.Clamp(tempMultiplier, 0, 1), 0f);
            }
        }

        public void UpdateComboMultiplier(float multiplier)
        {
            string info = $"x{multiplier}";

            if (_scoreMultiplierTextMesh != null)
            {
                _scoreMultiplierTextMesh.transform.DOScale(multiplier > 0 ? 1 : 0, 0.25f);
                _scoreMultiplierTextMesh.SetText(info);
            }

            ScoreLocking(multiplier);
        }

        private void ScoreLocking(float multiplier)
        {
            bool checkLock = multiplier == 0;

            _lock.color = checkLock ? Color.red : Color.white;
            _lock.sprite = checkLock ? _lockICON : _unLockICON;

            if (_lock != null)
            {
                _lock.transform.DOScale(checkLock ? 1f : 0f, 0.35f).OnComplete(() =>
                {
                    if (_scoreTextMesh != null)
                    {
                        _scoreTextMesh.transform.DOScale(checkLock ? 0f : 1f, !checkLock ? 0.2f : 0f);
                    }
                });
            }
        } 
        #endregion
        #region UI_GameStatus
        private void ShowGameStatus(GameStatus status)
        {
            string info = status switch
            {
                GameStatus.None => "Click To Start",
                _ => "",
            };

            _informationTextMesh.SetText(info);

            bool check = status == GameStatus.Start ? true : false;
            _informationTextMesh.GetComponent<Animator>().SetBool("Disable", check);
        }


        public void UpdateGameOverTimer(float timer, float duration)
        {
            _restartGameSlider.maxValue = duration;
            _restartGameSlider.value = timer;

            _restartGameTimerTextMesh.SetText(timer.ToString("F0"));
        } 
        #endregion
        private void OnDestroy()
        {
            GameManager.OnChangeGameStatus -= ShowGameStatus;
        }
    }
}
