using UnityEngine;
using UnityEngine.UI;

public class AtmosphereController : MonoBehaviour
{
    public Light sunLight;
    public Material skyMaterial;
    public Gradient skyColors;
    public Gradient lightColors;
    public AnimationCurve lightIntensityCurve;
    public AnimationCurve fogDensityCurve;
    public Color dayFogColor;
    public Color nightFogColor;

    [Range(6, 18)]
    public float timeOfDay;
    public Slider timeSlider;

    void Start()
    {
        timeSlider.value = timeOfDay;

        timeSlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void OnSliderValueChanged(float value)
    {
        timeOfDay = value;
    }

    void Update()
    {
        UpdateLighting();
        UpdateSky();
        UpdateFog();
    }

    private void UpdateLighting()
    {
        float normalizedTime = timeOfDay / 18f;
        float sunRotationX = (timeOfDay / 24f) * 360f - 90f;

        sunLight.color = lightColors.Evaluate(normalizedTime);
        sunLight.intensity = lightIntensityCurve.Evaluate(normalizedTime);
        sunLight.transform.rotation = Quaternion.Euler(sunRotationX, 170f, 0);

    }

    private void UpdateSky()
    {
        skyMaterial.SetColor("_SkyTint", skyColors.Evaluate(timeOfDay / 18f));
    }

    private void UpdateFog()
    {
        float normalizedTime = timeOfDay / 18f;
        float fogIntensity = lightIntensityCurve.Evaluate(normalizedTime);
        RenderSettings.fogColor = Color.Lerp(nightFogColor, dayFogColor, fogIntensity);
        RenderSettings.fogDensity = fogDensityCurve.Evaluate(normalizedTime);
    }
}
