using UnityEngine;
using TMPro;

public class InfoPanelController : MonoBehaviour
{
    public TextMeshProUGUI dayLabel;
    public TextMeshProUGUI timeLabel;
    public TextMeshProUGUI fpLabel;

    float _gameMinutes = 480f;
    int   _day = 1;
    int   _season = 0;
    static readonly string[] Seasons = { "SPRING", "SUMMER", "AUTUMN", "WINTER" };

    void Update()
    {
        _gameMinutes += Time.deltaTime * 2f;
        if (_gameMinutes >= 1440f) { _gameMinutes -= 1440f; AdvanceDay(); }

        if (dayLabel  != null) dayLabel.text  = $"DAY {_day}, {Seasons[_season % 4]} {_season + 1}";
        if (timeLabel != null) timeLabel.text = FormatTime(_gameMinutes);
        if (fpLabel   != null)
        {
            float fp = ResourceSystem.Instance != null ? ResourceSystem.Instance.FocusPoints : 0f;
            float mult = ResourceSystem.Instance != null ? ResourceSystem.Instance.GlobalMultiplier : 1f;
            fpLabel.text = mult > 1.01f ? $"{(int)fp} FP (x{mult:F2})" : $"{(int)fp} FP";
        }
    }

    void AdvanceDay()
    {
        _day++;
        if (_day > 28) { _day = 1; _season++; }
    }

    static string FormatTime(float minutes)
    {
        int h = (int)(minutes / 60) % 24;
        int m = (int)(minutes % 60);
        string ampm = h >= 12 ? "PM" : "AM";
        int h12 = h % 12; if (h12 == 0) h12 = 12;
        return $"{h12}:{m:D2} {ampm}";
    }
}
