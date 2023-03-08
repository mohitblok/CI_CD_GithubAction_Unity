using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum LoaderType
{
    DefaultUILoader,
    UILoader1,
    UILoader2,
    SphericalMeshLoader
}

public class DontDestroyOnLoad : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}

public class LoadingScreen : MonoSingleton<LoadingScreen>
{
    #region variables
    [SerializeField] private float _fadeDuration = 1f;

    [SerializeField] private GameObject _canvas;
    [SerializeField] private Transform _canvasTransform;

    [SerializeField] private CanvasGroup _fadeCanvasGroup;
    [SerializeField] private Slider _slider;
    [SerializeField] private TMPro.TextMeshProUGUI _percentageText;
    [SerializeField] private GameObject _instantiatedUIPrefab;

    private System.Action<string> _statusCallback;
    private AsyncOperation _operation;

    #endregion

    public void Init(Action callback)
    {
        callback?.Invoke(); //do we need this?
    }

    private void Start()
    {
        _canvas = GameObject.Find("CanvasGroup");
        
        if (_canvas == null)
        {
            _canvas = new GameObject();
            _canvas.name = "CanvasGroup";
            _canvas.AddComponent<RectTransform>();
            _canvas.AddComponent<CanvasGroup>();
            _canvas.AddComponent<DontDestroyOnLoad>();
        }
        _canvasTransform = _canvas.transform;
        _fadeCanvasGroup = _canvas.GetComponent<CanvasGroup>();


        //below events make sure loading is showing when assets/environemnt is/are loading.
        if (LoadingManager.Instance != null)
        {            
            LoadingManager.Instance.OnLoadingEnvironment += Instance_OnLoadingEnvironment;
            LoadingManager.Instance.OnLeavingEnvironment += Instance_OnLeavingEnvironment;
            LoadingManager.Instance.OnEnvironmentLoaded += Instance_OnEnvironmentLoaded;
        }

        if (SceneLoadingManager.Instance != null)
        {
            SceneLoadingManager.Instance.OnUpdateLoadingAmount += Instance_OnUpdateLoadingAmount;
        }

        //test loading to see if loading screen is working.
        ShowLoading(null, LoaderType.DefaultUILoader,true); //show UI loader when scene loads            
    }

    /// <summary>
    /// Disable/Fade out loading when environment has loaded
    /// </summary>
    private void Instance_OnEnvironmentLoaded()
    {
        StartCoroutine(Fade(1f)); //sending fade duration of 1 seconds. canvas will fade in 1 second.
    }

    /// <summary>
    /// show loading when leaving the environment
    /// </summary>
    private void Instance_OnLeavingEnvironment()
    {
        ShowLoading(null, LoaderType.DefaultUILoader);
    }

    /// <summary>
    /// show loading when environment is loading
    /// </summary>
    private void Instance_OnLoadingEnvironment()
    {
        ShowLoading(null, LoaderType.DefaultUILoader); //show UI loader when scene loads            
    }

    /// <summary>
    /// As assets are loading update loading percentage slider and text
    /// </summary>
    /// <param name="value"></param>
    private void Instance_OnUpdateLoadingAmount(float value)
    {
        if (_slider)
        {
            _slider.value = value;
        }
        if (_percentageText)
        {
            _percentageText.text = "Loading: " + (value * 100).ToString("0") + "%";
        }

        if (value >= 1)
        {
            StartCoroutine(Fade(0f));
        }
    }   

    /// <summary>
    /// call this function to show loading screen whenever required.
    /// </summary>
    /// <param name="statusCallback"></param>    //this is your callback event
    /// <param name="type"></param>              //what type of loader want to show
    /// <param name="showFadeWithDelay"></param> //do we automatically fade out loading after some time
    /// <param name="fadeDelayValue"></param>    //how many seconds after we fade out if previous parameter is true
    /// <param name="fadeDuration"></param>      //how long it should take to fade out
    public void ShowLoading(System.Action<string> statusCallback, LoaderType type = LoaderType.DefaultUILoader,
        bool showFadeWithDelay = false, float fadeDelayValue = 3f, float fadeDuration = 1f)
    {
        _statusCallback = statusCallback;
        _fadeDuration = fadeDuration;
        StartCoroutine(Instantiate(type));
        if (showFadeWithDelay)
        {
            StartCoroutine(ShowFadeWithDelay(fadeDelayValue));
        }
    }

    private IEnumerator ShowFadeWithDelay(float time)
    {   
        float duration = time;
                            
        float normalizedTime = 0;
        while(normalizedTime <= 1f)
        {
            if (_slider)
            {
                _slider.value = normalizedTime;
            }
            if (_percentageText)
            {
                _percentageText.text = "Loading: " + (normalizedTime * 100).ToString("0") + "%";
            }

            normalizedTime += Time.deltaTime / duration;
            yield return null;
        }

        StartCoroutine(Fade(0f));
    }

    /// <summary>
    /// Main funciton which handles loader instantiation based on type
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private IEnumerator Instantiate(LoaderType type)
    {
        switch (type)
        {
            case LoaderType.DefaultUILoader:
                GameObject g = (GameObject)Resources.Load("Prefabs/Loader/DefaultUILoaderCanvas");
                _instantiatedUIPrefab = Instantiate(g);
                break;
            case LoaderType.UILoader1:
                _instantiatedUIPrefab = Instantiate((GameObject)Resources.Load("Prefabs/Loader/UILoader1Canvas"));
                break;
            case LoaderType.UILoader2:
                _instantiatedUIPrefab = Instantiate((GameObject)Resources.Load("Prefabs/Loader/UILoader2Canvas"));
                break;
            case LoaderType.SphericalMeshLoader:
                _instantiatedUIPrefab = Instantiate((GameObject)Resources.Load("Prefabs/Loader/SphericalMeshLoader"));
                break;
        }

        yield return new WaitForSeconds(0.1f);

        _instantiatedUIPrefab.transform.SetParent(_canvasTransform,false);

        if (_statusCallback != null) { _statusCallback("loader added"); } //callback status

        //make sure the load has text and slider in child hierarchy
        _slider = _instantiatedUIPrefab.transform.GetChild(0).GetChild(1).GetComponent<Slider>();
        _percentageText = _instantiatedUIPrefab.transform.GetChild(0).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>();
        //once instantiated loader will block all UI raycasts
        _fadeCanvasGroup.GetComponent<CanvasGroup>().blocksRaycasts = true;
    }   

    private IEnumerator Fade(float finalAlpha)
    {
        if (_statusCallback != null) { _statusCallback("fade out started"); } //callback status

        float fadeSpeed = Mathf.Abs(_fadeCanvasGroup.alpha - finalAlpha) / _fadeDuration;

        while (!Mathf.Approximately(_fadeCanvasGroup.alpha, finalAlpha))
        {
            _fadeCanvasGroup.alpha = Mathf.MoveTowards(_fadeCanvasGroup.alpha, finalAlpha,
                fadeSpeed * Time.deltaTime);
            yield return null;
        }
        //disable 
        _fadeCanvasGroup.GetComponent<CanvasGroup>().blocksRaycasts = false;
        Destroy(_instantiatedUIPrefab);
    }
}
