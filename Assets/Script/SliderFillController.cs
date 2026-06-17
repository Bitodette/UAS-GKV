using UnityEngine;
using UnityEngine.UI;

public class SliderFillController : MonoBehaviour
{
    private Image fillImage;
    private Slider parentSlider;

    private void Start()
    {
        fillImage = GetComponent<Image>();
        parentSlider = GetComponentInParent<Slider>();

        if (parentSlider != null)
        {
            parentSlider.onValueChanged.AddListener(OnSliderValueChanged);
            OnSliderValueChanged(parentSlider.value);
        }
    }

    private void OnSliderValueChanged(float value)
    {
        fillImage.fillAmount = value;
    }
}
