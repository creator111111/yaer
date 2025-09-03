using UnityEngine;
using UnityEngine.UI;

public class StatChangeBuffer : MonoBehaviour
{
    [SerializeField]
    private Slider StatSlider;
    [SerializeField]
    private Image BufferImage;
    [SerializeField]
    [Range(0f, 10f)]
    private float LerpSpeed;

    // Update is called once per frame
    void Update()
    {
        BufferImage.fillAmount = Mathf.Lerp(BufferImage.fillAmount, StatSlider.value, LerpSpeed * Time.deltaTime);
    }
}
