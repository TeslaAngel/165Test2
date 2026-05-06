using UnityEngine;
using TMPro;

public class UIRaceDisplay : MonoBehaviour
{
  public RaceManager raceManager;

  public TextMeshProUGUI countdownText;
  public TextMeshProUGUI timerText;
  public TextMeshProUGUI penaltyText;

  void Update()
  {
    if (raceManager == null) return;

    if (raceManager.IsCountdownActive())
    {
      countdownText.gameObject.SetActive(true);
      timerText.gameObject.SetActive(false);

      float t = raceManager.GetCountdownTime();
      countdownText.text = Mathf.Ceil(t).ToString();
    }
    else
    {
      countdownText.gameObject.SetActive(false);
      timerText.gameObject.SetActive(true);
      timerText.text = FormatTime(raceManager.GetRaceTime());
      if (raceManager.GetPenalty())
      {
        penaltyText.gameObject.SetActive(true);
        penaltyText.text = FormatTime(raceManager.GetPenaltyTime());
      }
      else
      {
        penaltyText.gameObject.SetActive(false);
      }
    }
  }

  string FormatTime(float t)
  {
    int minutes = Mathf.FloorToInt(t / 60f);
    int seconds = Mathf.FloorToInt(t % 60f);
    int ms = Mathf.FloorToInt((t * 100f) % 100f);

    return $"{minutes:00}:{seconds:00}:{ms:00}";
  }
}