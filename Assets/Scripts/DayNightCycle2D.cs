using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DayNightCycle2D : MonoBehaviour
{
    public float cycleDuration = 60f; // Duração total do ciclo em segundos (dia + noite)
    public Gradient lightColorOverTime; // Cores ao longo do ciclo
    public AnimationCurve intensityOverTime; // Intensidade da luz ao longo do tempo

    private Light2D globalLight;
    private float timer = 0f;

    void Start()
    {
        globalLight = GetComponent<Light2D>();
        if (globalLight == null)
        {
            Debug.LogError("Light2D component not found!");
        }
    }

    void Update()
    {
        if (globalLight == null) return;

        timer += Time.deltaTime;
        if (timer > cycleDuration)
        {
            timer -= cycleDuration;
        }

        float t = timer / cycleDuration;

        globalLight.color = lightColorOverTime.Evaluate(t);
    }
}
