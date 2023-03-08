using UnityEngine;

/// <summary>
/// Makes the alpha of a CanvasGroup animate from start to finish over a period of time.
/// Used to show a pulse of colour on an LED which gradually fades out
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class AlphaPulse : MonoBehaviour
{
    [SerializeField] private AnimationCurve curve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 1), new Keyframe(1, 0) });
    [SerializeField] private float _duration = 1;
    [SerializeField] private bool _pulseOnStartup;
    [SerializeField] private bool _loop;

    private float _pulseTime = 1;
    private float PulseTime
    {
        get
        {
            return _pulseTime;
        }
        set
        {
            _pulseTime = Mathf.Clamp01(value);
        }
    }
    private CanvasGroup _canvasGroup;
    private CanvasGroup canvasGroup
    {
        get
        {
            if (!_canvasGroup)
                _canvasGroup = GetComponent<CanvasGroup>();
            return _canvasGroup;
        }
    }
    private bool IsPulsing => PulseTime <= 1;

    private void Start()
    {
        DisplayAlphaPulse(1);

        if (_pulseOnStartup)
        {
            Pulse();
        }
    }

    private void Update()
    {
        if (!IsPulsing)
        {
            return;
        }

        ProgressTimer();
        DisplayAlphaPulse(PulseTime);
    }

    [ContextMenu("Test Pulse")]
    public void Pulse()
    {
        PulseTime = 0;
    }

    private void ProgressTimer()
    {
        PulseTime += Time.deltaTime * (1 / _duration);

        if (_loop && PulseTime >= 1)
        {
            Pulse();
        }
    }

    private void DisplayAlphaPulse(float value)
    {
        canvasGroup.alpha = curve.Evaluate(value);
    }

    public void EndPulse()
    {
        PulseTime = 1;
    }
}