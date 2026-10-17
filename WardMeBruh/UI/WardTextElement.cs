using TMPro;
using UnityEngine;

namespace WardMeBruh.UI;

public class WardTextElement : MonoBehaviour
{
    public int baseFontSize;
    public string localizationKey;
    public bool toUpper;

    public void UpdateLocalization()
    {
        if (string.IsNullOrEmpty(localizationKey) || !localizationKey.StartsWith("$"))
        {
            return;
        }

        TextMeshProUGUI tmp = GetComponent<TextMeshProUGUI>();
        if (tmp != null)
        {
            string localized = localizationKey.Localize();
            tmp.text = toUpper ? localized.ToUpperInvariant() : localized;
        }
    }
}

