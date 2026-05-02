using System;
using GameData;
using TMPro;
using UnityEngine;

public class ProfanityHandler : MonoBehaviour
{
    [SerializeField] private TMP_Text profanityText;

    public void Start()
    {
        if (GamePrefs.GetBool(GameDefaultData.ProfanityKey, true))
        {
            profanityText.text += " Doodle Jump!";
        }
        else
        {
            profanityText.text += " D#&dl@ J%mp!";
        }
    }

    public void StartGame()
    {
        GameManager.Instance.LoadGameScene();
    }
}
