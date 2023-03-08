using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// This class extends the "BaseModule" class and provides module-specific implementation from coin gecko api.
/// Shows graph, price and/or to the moon of any token specified in the data.
/// </summary>
/// <see cref="BaseModule"/>
public class PriceTrackerModule : BaseModule
{
    /// <summary>
    /// Different states the different displays mode
    /// </summary>
    public enum DisplayMode
    {
        Ticker,
        Chart,
        ToTheMoon
    }
    
    private const string API_URL = "https://api.coingecko.com/api/v3/";

    private new PriceTrackerModuleData data;
    
    private bool isReady = false;
    private Material chartMaterial;
    private List<decimal[]> prices;
    private List<float> normalisedPrices = new List<float>();
    private float startY;

    private string GetPriceUrl(string token, string vsCurrency)
    {
        return $"{API_URL}simple/price?ids={token}&vs_currencies={vsCurrency}&include_24hr_change=true";
    }
    
    private string GetChartUrl(string token, string vsCurrency)
    {
        return $"{API_URL}coins/{token}/market_chart?vs_currency={vsCurrency}&days=1";
    }
    
    /// <summary>
    /// Creates a reference to the retrieved and apply module data.
    /// </summary>
    /// <param name="baseModuleData"></param>
    /// <param name="callback"></param>
    public override void Init(BaseModuleData baseModuleData, Action callback)
    {
        data = (PriceTrackerModuleData)baseModuleData;

        switch (data.DisplayMode)
        {
            case DisplayMode.Ticker:
                InitTicker(callback);
                break;
            case DisplayMode.Chart:
                InitChart(callback);
                break;
            case DisplayMode.ToTheMoon:
                startY = transform.position.y;
                InitToTheMoon(callback);
                break;
        }
    }
    
    private void InitTicker(Action callback)
    {
        var url = GetPriceUrl(data.token, data.vsCurrency);
        WebRequestManager.Instance.DownloadSmallItem(url, (Dictionary<string, Dictionary<string, decimal>> response) =>
        {
            var price = response[data.token][data.vsCurrency];
            var change = response[data.token][$"{data.vsCurrency}_24h_change"];
            
            
            var textMeshProUGUI = transform.Find(data.textName).GetComponent<TextMeshProUGUI>();

            textMeshProUGUI.text = $"{data.token.ToUpper()}/{data.vsCurrency.ToUpper()}\n{price}\n({change.ToString("F3")}%)";

            if (change == 0)
            {
                textMeshProUGUI.color = Color.gray;
            }  
            else if (change < 0)
            {
                textMeshProUGUI.color = Color.red;
            }
            else if (change > 0)
            {
                textMeshProUGUI.color = Color.green;
            }
            
            callback?.Invoke();
            
            Invoke("UpdateTicker", 60);
        }, s =>
        {
            Debug.LogError(s);
            callback?.Invoke();
        });
    }

    private void InitToTheMoon(Action callback)
    {
        var url = GetPriceUrl(data.token, data.vsCurrency);
        WebRequestManager.Instance.DownloadSmallItem(url, (Dictionary<string, Dictionary<string, decimal>> response) =>
        {
            var price = response[data.token][data.vsCurrency];
            var normalisedHeight = Mathf.InverseLerp(0, data.targetPrice, (float)price);

            var pos = transform.position;
            pos.y = Mathf.Lerp(startY, data.moonHeight, normalisedHeight);
            transform.position = pos;
            
            callback?.Invoke();
            
            Invoke("UpdateMoon", 60);
        }, s =>
        {
            Debug.LogError(s);
            callback?.Invoke();
        });
    }
    
    private void InitChart(Action callback)
    {
        var url = GetChartUrl(data.token, data.vsCurrency);
        WebRequestManager.Instance.DownloadSmallItem(url, (Dictionary<string, List<decimal[]>> response) =>
        {
            prices = response["prices"];
            
            decimal lowestPrice = 99999999;
            decimal highestPrice = -99999999;
            foreach (var price in prices)
            {
                if (price[1] < lowestPrice)
                {
                    lowestPrice = price[1];
                }
                
                if (price[1] > highestPrice)
                {
                    highestPrice = price[1];
                }
            }

            foreach (var price in prices)
            {
                normalisedPrices.Add(Mathf.InverseLerp((float)lowestPrice, (float)highestPrice, (float)price[1]));
            }

            if (!chartMaterial)
            {
                // Unity has a built-in shader that is useful for drawing
                // simple colored things.
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                chartMaterial = new Material(shader);
                chartMaterial.hideFlags = HideFlags.HideAndDontSave;
                // Turn on alpha blending
                chartMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                chartMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                // Turn backface culling off
                chartMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                // Turn off depth writes
                chartMaterial.SetInt("_ZWrite", 0);
            }

            isReady = true;
            callback?.Invoke();
        }, s =>
        {
            Debug.LogError(s);
            callback?.Invoke();
        });
    }

    private void OnRenderObject()
    {
        if (data.DisplayMode != DisplayMode.Chart || !isReady)
            return;
        
        // Apply the line material
        chartMaterial.SetPass(0);

        GL.PushMatrix();
        // Set transformation matrix for drawing to
        // match our transform
        GL.MultMatrix(transform.localToWorldMatrix);
        
        GL.Begin(GL.QUADS);
        
        GL.Color(Color.white);
        GL.Vertex3(0, 0, 0);
        GL.Vertex3(0f, 1, 0);
        GL.Vertex3(2, 1f, 0);
        GL.Vertex3(2f, 0, 0);
        GL.End();
        
        GL.Begin(GL.LINES);
        
        GL.Color(Color.black);
        
        GL.Vertex3(0.1f, 0.1f, 0);
        GL.Vertex3(0.1f, 0.9f, 0);
        GL.Vertex3(0.1f, 0.9f, 0);
        GL.Vertex3(1.9f, 0.9f, 0);
        GL.Vertex3(1.9f, 0.9f, 0);
        GL.Vertex3(1.9f, 0.1f, 0);
        GL.Vertex3(1.9f, 0.1f, 0);
        GL.Vertex3(0.1f, 0.1f, 0);
        GL.End();
        
        GL.Begin(GL.LINES);
        
        var pad = 0.1f;
        var step = (2f - (pad * 2f)) / prices.Count;
        for (int i = 0; i < prices.Count; ++i)
        {
            var price = prices[i];
            if (i == 0)
            {
                GL.Vertex3(0, pad + normalisedPrices[i] * 0.8f, 0);
            }
            else
            {
                if (price[1] == prices[0][1])
                {
                    GL.Color(Color.grey);
                }
                else if (price[1] < prices[0][1])
                {
                    GL.Color(Color.red);
                }
                else if (price[1] > prices[0][1])
                {
                    GL.Color(Color.green);
                }
                // Another vertex at edge of circle
                GL.Vertex3(pad + (i-1) * step, pad + normalisedPrices[i-1] * 0.8f, 0);
            }
            GL.Vertex3(pad + i * step, pad + normalisedPrices[i] * 0.8f, 0);
            
        }
        GL.End();
        GL.PopMatrix();
    }

    private void UpdateTicker()
    {
        InitTicker(null);
    }
    
    private void UpdateMoon()
    {
        InitToTheMoon(null);
    }

    public override void Deinit()
    {
        prices.Clear();
        normalisedPrices.Clear();
    }
}

/// <summary>
/// Data which is modifiable by users
/// </summary>
/// <see cref="BaseModuleData"/>
public class PriceTrackerModuleData : BaseModuleData
{
    /// <summary>
    /// The token
    /// </summary>
    public string token;
    /// <summary>
    /// The vs currency
    /// </summary>
    public string vsCurrency;
    /// <summary>
    /// The display mode
    /// </summary>
    public PriceTrackerModule.DisplayMode DisplayMode;
    /// <summary>
    /// The text name
    /// </summary>
    public string textName;
    /// <summary>
    /// The target price
    /// </summary>
    public float targetPrice;
    /// <summary>
    /// The moon height
    /// </summary>
    public float moonHeight;
}