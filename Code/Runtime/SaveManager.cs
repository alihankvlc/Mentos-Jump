using System.IO;
using UnityEngine;

namespace Assets._KVLC_Project_Helix_Jump.Code.Runtime
{
    public interface ISaveProvider
    {
        void SaveHighScore(int highScore);
        void LoadHighScore(ref int highScore);
        void ResetHighScore(ref int highScore);
    }

    public class SaveManager : Singleton<SaveManager>, ISaveProvider
    {
        private string _saveFilePath;

        private struct ScoreData
        {
            public int HighScore;
        }
        protected override void Awake()
        {
            _saveFilePath = Path.Combine(Application.dataPath, "ballData.json");
        }

        public void SaveHighScore(int highScore)
        {
            try
            {
                ScoreData scoreData = new ScoreData
                {
                    HighScore = highScore,
                };

                string json = JsonUtility.ToJson(scoreData);
                File.WriteAllText(_saveFilePath, json);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error save high score: {ex}");
            }

        }

        public void LoadHighScore(ref int highScore)
        {
            try
            {
                if (File.Exists(_saveFilePath))
                {
                    string json = File.ReadAllText(_saveFilePath);
                    ScoreData scoreData = JsonUtility.FromJson<ScoreData>(json);
                    highScore = scoreData.HighScore;
                }
                else
                {
                    SaveHighScore(0);
                    highScore = 0;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error loading high score: {ex}");
                highScore = 0; 
            }
        }

        public void ResetHighScore(ref int highScore)
        {
            try
            {
                ScoreData scoreData = new ScoreData
                {
                    HighScore = 0
                };

                string json = JsonUtility.ToJson(scoreData);
                File.WriteAllText(_saveFilePath, json);
                highScore = 0;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error resetting high score: {ex}");
            }
        }
    }
}
