using UnityEngine;
using UnityEngine.UI;

// update fill slider UI sesuai value slider
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
            OnSliderValueChanged(parentSlider.value);   // set awal
        }
    }

    private void OnSliderValueChanged(float value)
    {
        fillImage.fillAmount = value;                   // isi fill sesuai value slider
    }
}
