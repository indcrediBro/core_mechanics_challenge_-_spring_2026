using IncredibleAttributes;
using UnityEngine;

namespace GameData
{
    [CreateAssetMenu(fileName = "GameDefaultData", menuName = "Game/Default Data", order = 0)]
    public class GameDefaultData : ScriptableObject
    {
        [Title("Score")]
        [SerializeField] private int scorePerPlatform = 10;
        [SerializeField] private int scorePerKill     = 50;

        [SerializeField] private string HIGH_SCORE_KEY = "HighScore";
        [Title("Audio")]
        [SerializeField] private string MASTER_VOL_KEY = "Master";
        [SerializeField] private string SFX_VOL_KEY    = "SFX";
        [SerializeField] private string MUSIC_VOL_KEY  = "Music";

        public int ScorePerPlatform => scorePerPlatform;
        public int ScorePerKill => scorePerKill;

        public string HighScoreKey => HIGH_SCORE_KEY;
        public string MasterKey => MASTER_VOL_KEY;
        public string SfxVolKey => SFX_VOL_KEY;
        public string MusicVolKey => MUSIC_VOL_KEY;
    }
}