using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DayNightUI : MonoBehaviour
{
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text labelText;
    [SerializeField] private Sprite sunSprite;
    [SerializeField] private Sprite moonSprite;

    private void Update()
    {
        if (waveManager == null || iconImage == null || labelText == null)
            return;

        switch (waveManager.CurrentPhase)
        {
            case WaveManager.Phase.Day:
                iconImage.sprite = sunSprite;
                labelText.text = "Day";
                break;

            case WaveManager.Phase.Night:
                iconImage.sprite = moonSprite;
                labelText.text = "Night";
                break;
        }
    }
}
