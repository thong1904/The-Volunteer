using UnityEngine;
using System;

/// <summary>
/// Quản lý chu kỳ ngày/đêm. Là con của GameManager.
/// Truy cập qua: GameManager.Instance.DayNight
/// </summary>
public class DayNightManager : MonoBehaviour
{
    public static DayNightManager Instance { get; private set; }

    [Header("Thời gian")]
    [SerializeField] private float startHour = 6f;
    [SerializeField] private float endHour = 21f;
    [SerializeField] private float minutesPerHour = 0.3f;

    [Header("Skybox (Procedural)")]
    [Tooltip("Skybox Material dùng Shader: Skybox/Procedural")]
    [SerializeField] private Material skyboxMaterial;
    
    [Header("Màu bầu trời theo thời gian")]
    [SerializeField] private Color skyTintMorning = new Color(0.8f, 0.9f, 1f);      // Sáng sớm - xanh nhạt
    [SerializeField] private Color skyTintNoon = new Color(0.5f, 0.7f, 1f);          // Trưa - xanh đậm
    [SerializeField] private Color skyTintSunset = new Color(1f, 0.6f, 0.4f);        // Hoàng hôn - cam
    [SerializeField] private Color skyTintNight = new Color(0.1f, 0.1f, 0.2f);       // Tối - tím đen
    
    [Header("Exposure bầu trời")]
    [SerializeField] private float exposureMorning = 1.0f;
    [SerializeField] private float exposureNoon = 1.3f;
    [SerializeField] private float exposureSunset = 0.6f;
    [SerializeField] private float exposureNight = 0.05f;  // Rất tối
    
    [Header("Ánh sáng mặt trời")]
    [SerializeField] private Light sunLight;
    [SerializeField] private Color sunColorMorning = new Color(1f, 0.85f, 0.6f);
    [SerializeField] private Color sunColorNoon = new Color(1f, 0.98f, 0.95f);
    [SerializeField] private Color sunColorSunset = new Color(1f, 0.4f, 0.15f);
    [SerializeField] private Color sunColorNight = new Color(0.05f, 0.05f, 0.1f);  // Gần như tắt
    
    [Header("Cường độ ánh sáng")]
    [SerializeField] private float intensityMorning = 0.7f;
    [SerializeField] private float intensityNoon = 1.2f;
    [SerializeField] private float intensitySunset = 0.3f;
    [SerializeField] private float intensityNight = 0.0f;  // Tắt hoàn toàn
    
    [Header("Ambient Light (ánh sáng môi trường)")]
    [SerializeField] private Color ambientMorning = new Color(0.5f, 0.5f, 0.6f);
    [SerializeField] private Color ambientNoon = new Color(0.7f, 0.7f, 0.75f);
    [SerializeField] private Color ambientSunset = new Color(0.3f, 0.2f, 0.2f);
    [SerializeField] private Color ambientNight = new Color(0.02f, 0.02f, 0.04f);  // Gần như đen
    
    [Header("Fog (sương mù ban đêm)")]
    [SerializeField] private bool useFog = true;
    [SerializeField] private Color fogColorDay = new Color(0.7f, 0.8f, 0.9f);
    [SerializeField] private Color fogColorNight = new Color(0.02f, 0.02f, 0.05f);
    [SerializeField] private float fogDensityDay = 0.001f;
    [SerializeField] private float fogDensityNight = 0.02f;
    
    [Header("Reflection Intensity")]
    [SerializeField] private float reflectionDay = 1f;
    [SerializeField] private float reflectionNight = 0.1f;
    
    [Header("Thời điểm chuyển đổi (giờ)")]
    [SerializeField] private float morningEnd = 9f;       // Kết thúc sáng sớm
    [SerializeField] private float afternoonEnd = 16f;    // Kết thúc trưa/chiều
    [SerializeField] private float sunsetEnd = 19f;       // Kết thúc hoàng hôn

    private float currentHour;
    private bool isRunning = true;

    public event Action<float> OnTimeChanged;
    public event Action OnSunset;
    public event Action OnNewDay;

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        currentHour = startHour;
        
        // Đảm bảo skybox được gán
        if (skyboxMaterial != null)
        {
            RenderSettings.skybox = skyboxMaterial;
        }
        
        UpdateEnvironment();
    }

    private void Update()
    {
        if (!isRunning) return;

        currentHour += (Time.deltaTime / 60f) / minutesPerHour;

        if (currentHour >= endHour)
        {
            currentHour = endHour;
            isRunning = false;
            OnSunset?.Invoke();
            Debug.Log("Đã đến 9PM - Dừng thời gian!");
        }

        UpdateEnvironment();
        OnTimeChanged?.Invoke(currentHour);
    }

    private void UpdateEnvironment()
    {
        // Tính toán phase hiện tại (0-1) cho mỗi giai đoạn
        float phase = GetTimePhase();
        
        UpdateSkybox(phase);
        UpdateSunLight(phase);
        UpdateAmbientLight(phase);
        UpdateSunPosition();
    }

    /// <summary>
    /// Lấy phase hiện tại (0-4) dựa trên thời gian
    /// 0-1: Morning (6-9h)
    /// 1-2: Noon (9-16h)  
    /// 2-3: Sunset (16-19h)
    /// 3-4: Night (19-21h)
    /// </summary>
    private float GetTimePhase()
    {
        if (currentHour < morningEnd)
        {
            // Morning: 0 -> 1
            return Mathf.InverseLerp(startHour, morningEnd, currentHour);
        }
        else if (currentHour < afternoonEnd)
        {
            // Noon: 1 -> 2
            return 1f + Mathf.InverseLerp(morningEnd, afternoonEnd, currentHour);
        }
        else if (currentHour < sunsetEnd)
        {
            // Sunset: 2 -> 3
            return 2f + Mathf.InverseLerp(afternoonEnd, sunsetEnd, currentHour);
        }
        else
        {
            // Night: 3 -> 4
            return 3f + Mathf.InverseLerp(sunsetEnd, endHour, currentHour);
        }
    }

    /// <summary>
    /// Cập nhật Skybox - chuyển màu mượt mà
    /// </summary>
    private void UpdateSkybox(float phase)
    {
        if (skyboxMaterial == null) return;
        
        Color targetTint;
        float targetExposure;
        
        if (phase < 1f)
        {
            // Morning -> Noon
            targetTint = Color.Lerp(skyTintMorning, skyTintNoon, phase);
            targetExposure = Mathf.Lerp(exposureMorning, exposureNoon, phase);
        }
        else if (phase < 2f)
        {
            // Noon (giữ nguyên)
            float t = phase - 1f;
            targetTint = skyTintNoon;
            targetExposure = exposureNoon;
        }
        else if (phase < 3f)
        {
            // Noon -> Sunset
            float t = phase - 2f;
            targetTint = Color.Lerp(skyTintNoon, skyTintSunset, t);
            targetExposure = Mathf.Lerp(exposureNoon, exposureSunset, t);
        }
        else
        {
            // Sunset -> Night
            float t = phase - 3f;
            targetTint = Color.Lerp(skyTintSunset, skyTintNight, t);
            targetExposure = Mathf.Lerp(exposureSunset, exposureNight, t);
        }
        
        // Áp dụng cho Procedural Skybox
        if (skyboxMaterial.HasProperty("_SkyTint"))
        {
            skyboxMaterial.SetColor("_SkyTint", targetTint);
        }
        
        if (skyboxMaterial.HasProperty("_Exposure"))
        {
            skyboxMaterial.SetFloat("_Exposure", targetExposure);
        }
        
        // Cập nhật atmosphere thickness để tạo hiệu ứng hoàng hôn
        if (skyboxMaterial.HasProperty("_AtmosphereThickness"))
        {
            float thickness = phase < 2f ? 1f : Mathf.Lerp(1f, 2f, (phase - 2f) / 2f);
            skyboxMaterial.SetFloat("_AtmosphereThickness", thickness);
        }
    }

    /// <summary>
    /// Cập nhật ánh sáng mặt trời
    /// </summary>
    private void UpdateSunLight(float phase)
    {
        if (sunLight == null) return;
        
        Color targetColor;
        float targetIntensity;
        
        if (phase < 1f)
        {
            // Morning -> Noon
            targetColor = Color.Lerp(sunColorMorning, sunColorNoon, phase);
            targetIntensity = Mathf.Lerp(intensityMorning, intensityNoon, phase);
        }
        else if (phase < 2f)
        {
            // Noon
            targetColor = sunColorNoon;
            targetIntensity = intensityNoon;
        }
        else if (phase < 3f)
        {
            // Noon -> Sunset
            float t = phase - 2f;
            targetColor = Color.Lerp(sunColorNoon, sunColorSunset, t);
            targetIntensity = Mathf.Lerp(intensityNoon, intensitySunset, t);
        }
        else
        {
            // Sunset -> Night
            float t = phase - 3f;
            targetColor = Color.Lerp(sunColorSunset, sunColorNight, t);
            targetIntensity = Mathf.Lerp(intensitySunset, intensityNight, t);
        }
        
        sunLight.color = targetColor;
        sunLight.intensity = targetIntensity;
    }

    /// <summary>
    /// Cập nhật Ambient Light (ánh sáng môi trường cho tất cả objects)
    /// </summary>
    private void UpdateAmbientLight(float phase)
    {
        Color targetAmbient;
        
        if (phase < 1f)
        {
            targetAmbient = Color.Lerp(ambientMorning, ambientNoon, phase);
        }
        else if (phase < 2f)
        {
            targetAmbient = ambientNoon;
        }
        else if (phase < 3f)
        {
            float t = phase - 2f;
            targetAmbient = Color.Lerp(ambientNoon, ambientSunset, t);
        }
        else
        {
            float t = phase - 3f;
            targetAmbient = Color.Lerp(ambientSunset, ambientNight, t);
        }
        
        // Áp dụng ambient light
        RenderSettings.ambientLight = targetAmbient;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        
        // Cập nhật Reflection Intensity
        float dayProgress = (currentHour - startHour) / (endHour - startHour);
        float reflectionIntensity = dayProgress < 0.5f 
            ? Mathf.Lerp(reflectionDay * 0.8f, reflectionDay, dayProgress * 2f)
            : Mathf.Lerp(reflectionDay, reflectionNight, (dayProgress - 0.5f) * 2f);
        RenderSettings.reflectionIntensity = reflectionIntensity;
        
        // Cập nhật Fog
        if (useFog)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            
            float fogT = Mathf.Clamp01((currentHour - sunsetEnd) / (endHour - sunsetEnd));
            if (currentHour < sunsetEnd)
            {
                RenderSettings.fogColor = fogColorDay;
                RenderSettings.fogDensity = fogDensityDay;
            }
            else
            {
                RenderSettings.fogColor = Color.Lerp(fogColorDay, fogColorNight, fogT);
                RenderSettings.fogDensity = Mathf.Lerp(fogDensityDay, fogDensityNight, fogT);
            }
        }
        
        // Cập nhật environment
        DynamicGI.UpdateEnvironment();
    }

    /// <summary>
    /// Cập nhật vị trí mặt trời
    /// </summary>
    private void UpdateSunPosition()
    {
        if (sunLight == null) return;
        
        float totalHours = endHour - startHour;
        float currentProgress = (currentHour - startHour) / totalHours;
        
        // Độ cao mặt trời (sin curve)
        float elevationAngle = Mathf.Sin(currentProgress * Mathf.PI) * 70f;
        
        // Hướng mặt trời: Đông -> Tây
        float azimuthAngle = Mathf.Lerp(-90f, 90f, currentProgress);
        
        sunLight.transform.rotation = Quaternion.Euler(elevationAngle, azimuthAngle, 0f);
    }

    #region Public Methods
    public float GetCurrentHour() => currentHour;
    
    public string GetTimeString()
    {
        int hours = (int)currentHour;
        int minutes = (int)((currentHour - hours) * 60);
        return $"{hours:D2}:{minutes:D2}";
    }
    
    public bool IsTimeRunning() => isRunning;
    
    public bool IsNighttime() => currentHour >= sunsetEnd;
    
    public bool IsDaytime() => currentHour >= morningEnd && currentHour < afternoonEnd;
    
    /// <summary>
    /// Lấy % thời gian trong ngày (0 = sáng, 1 = tối)
    /// </summary>
    public float GetDayProgress() => (currentHour - startHour) / (endHour - startHour);

    public void StartNewDay()
    {
        currentHour = startHour;
        isRunning = true;
        OnNewDay?.Invoke();
        Debug.Log("Ngày mới bắt đầu!");
    }
    
    /// <summary>
    /// Đặt thời gian cụ thể (dùng cho debug)
    /// </summary>
    public void SetTime(float hour)
    {
        currentHour = Mathf.Clamp(hour, startHour, endHour);
        UpdateEnvironment();
    }
    
    /// <summary>
    /// Tạm dừng/tiếp tục thời gian
    /// </summary>
    public void SetTimeRunning(bool running)
    {
        isRunning = running;
    }
    #endregion
}