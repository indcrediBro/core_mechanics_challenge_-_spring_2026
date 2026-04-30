using IncredibleAttributes;
using UnityEngine;

namespace GameData
{
    public static class GameDefaultData
    {
        [Title("Score")]
        [SerializeField] private static int scorePerPlatform = 10;
        [SerializeField] private static int scorePerKill     = 50;

        [SerializeField] private static string HIGH_SCORE_KEY = "HighScore";
        [Title("Audio")]
        [SerializeField] private static string MASTER_VOL_KEY = "Master";
        [SerializeField] private static string MUSIC_VOL_KEY  = "Music";
        [SerializeField] private static string SFX_VOL_KEY    = "SFX";
        [SerializeField] private static string UI_VOL_KEY  = "UI";

        [Title("Graphics")]
        [SerializeField] private static string GFX_QUALITY_KEY = "Quality";
        [SerializeField] private static string FULLSCREEN_KEY = "FullScreen";

        [Title("Language")]
        [SerializeField] private static string[] allLanguages = { "English","Russian","Japanese","Deutsch","Latvian", "Spanish", "Italian" };
        [SerializeField] private static string LANGUAGE_KEY = "Language";

        [SerializeField] private static string PROFANITY_KEY = "Profanity";

        [Title("Gameplay")]
        [SerializeField] private static string MOUSE_SENSITIVITY_KEY = "Sensitivity";
        [SerializeField] private static string MOUSE_INVERTY_KEY = "InvertY";

        public static int ScorePerPlatform => scorePerPlatform;
        public static int ScorePerKill => scorePerKill;

        public static string HighScoreKey => HIGH_SCORE_KEY;
        public static string SensitivityKey => MOUSE_SENSITIVITY_KEY;
        public static string MasterKey => MASTER_VOL_KEY;
        public static string SfxVolKey => SFX_VOL_KEY;
        public static string MusicVolKey => MUSIC_VOL_KEY;

        public static string[] AllLanguages => allLanguages;

        public static string LanguageKey => LANGUAGE_KEY;
    }
}