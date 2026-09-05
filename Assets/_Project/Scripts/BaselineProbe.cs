using System;
using System.Text;
using Unity.Profiling;
using UnityEngine;

public class BaselineProbe : MonoBehaviour
{
    public KeyCode startKey = KeyCode.F9;
    public float warmupSeconds = 25f;
    public float measureSeconds = 30f;
    public string runLabel = "baseline-stress";
    public string enemyTag = "Enemy";
    public bool showOnScreen = true;

    const int MaxSamples = 20000;

    ProfilerRecorder mainThreadRecorder;
    ProfilerRecorder gcAllocRecorder;
    ProfilerRecorder batchesRecorder;
    ProfilerRecorder setPassRecorder;

    float[] frameMs = new float[MaxSamples];
    float[] sortBuffer = new float[MaxSamples];
    int sampleCount;

    double cpuMsSum;
    int cpuMsSamples;
    long gcAllocSum;
    int gcAllocSamples;
    long batchesSum;
    long setPassSum;
    int renderSamples;

    long fallbackAllocSum;
    long lastTotalMemory;
    int gcCountStart;

    int enemyCountAtStart;
    int enemyCountAtEnd;

    int state;
    float stateTimer;
    string result = "";

    void OnEnable()
    {
        mainThreadRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread");
        gcAllocRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
        batchesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
        setPassRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
    }

    void OnDisable()
    {
        if (mainThreadRecorder.Valid) mainThreadRecorder.Dispose();
        if (gcAllocRecorder.Valid) gcAllocRecorder.Dispose();
        if (batchesRecorder.Valid) batchesRecorder.Dispose();
        if (setPassRecorder.Valid) setPassRecorder.Dispose();
    }

    void Update()
    {
        if (Input.GetKeyDown(startKey) && state == 0)
        {
            BeginWarmup();
            return;
        }

        if (state == 1)
        {
            stateTimer -= Time.unscaledDeltaTime;
            if (stateTimer <= 0f) BeginMeasure();
            return;
        }

        if (state == 2)
        {
            Sample();
            stateTimer -= Time.unscaledDeltaTime;
            if (stateTimer <= 0f) Finish();
        }
    }

    void BeginWarmup()
    {
        state = 1;
        stateTimer = warmupSeconds;
        result = "";
        enemyCountAtStart = CountEnemies();
        Debug.Log("[Probe] warmup " + warmupSeconds.ToString("0") + "s, enemies=" + enemyCountAtStart);
    }

    void BeginMeasure()
    {
        state = 2;
        stateTimer = measureSeconds;
        sampleCount = 0;
        cpuMsSum = 0.0;
        cpuMsSamples = 0;
        gcAllocSum = 0;
        gcAllocSamples = 0;
        batchesSum = 0;
        setPassSum = 0;
        renderSamples = 0;
        fallbackAllocSum = 0;
        lastTotalMemory = GC.GetTotalMemory(false);
        gcCountStart = GC.CollectionCount(0);
        Debug.Log("[Probe] measuring " + measureSeconds.ToString("0") + "s");
    }

    void Sample()
    {
        if (sampleCount < MaxSamples)
        {
            frameMs[sampleCount] = Time.unscaledDeltaTime * 1000f;
            sampleCount++;
        }

        if (mainThreadRecorder.Valid && mainThreadRecorder.LastValue > 0)
        {
            cpuMsSum += mainThreadRecorder.LastValue * 1e-6;
            cpuMsSamples++;
        }

        if (gcAllocRecorder.Valid)
        {
            gcAllocSum += gcAllocRecorder.LastValue;
            gcAllocSamples++;
        }

        if (batchesRecorder.Valid && setPassRecorder.Valid)
        {
            batchesSum += batchesRecorder.LastValue;
            setPassSum += setPassRecorder.LastValue;
            renderSamples++;
        }

        long now = GC.GetTotalMemory(false);
        if (now > lastTotalMemory) fallbackAllocSum += now - lastTotalMemory;
        lastTotalMemory = now;
    }

    void Finish()
    {
        state = 0;
        enemyCountAtEnd = CountEnemies();

        int gcCollections = GC.CollectionCount(0) - gcCountStart;

        Array.Copy(frameMs, sortBuffer, sampleCount);
        Array.Sort(sortBuffer, 0, sampleCount);

        float avg = 0f;
        for (int i = 0; i < sampleCount; i++) avg += sortBuffer[i];
        avg = sampleCount > 0 ? avg / sampleCount : 0f;

        float median = Percentile(sortBuffer, sampleCount, 0.50f);
        float p95 = Percentile(sortBuffer, sampleCount, 0.95f);
        float p99 = Percentile(sortBuffer, sampleCount, 0.99f);
        float max = sampleCount > 0 ? sortBuffer[sampleCount - 1] : 0f;
        float fps = avg > 0f ? 1000f / avg : 0f;

        string cpuMs = cpuMsSamples > 0 ? (cpuMsSum / cpuMsSamples).ToString("0.00") : "n/a";
        string allocKb;
        string allocSource;
        if (gcAllocSamples > 0 && gcAllocSum > 0)
        {
            allocKb = (gcAllocSum / (double)gcAllocSamples / 1024.0).ToString("0.00");
            allocSource = "recorder";
        }
        else
        {
            allocKb = sampleCount > 0 ? (fallbackAllocSum / (double)sampleCount / 1024.0).ToString("0.00") : "0";
            allocSource = "gc-delta";
        }
        string batches = renderSamples > 0 ? (batchesSum / renderSamples).ToString() : "n/a";
        string setPass = renderSamples > 0 ? (setPassSum / renderSamples).ToString() : "n/a";

        StringBuilder sb = new StringBuilder(512);
        sb.Append("[Probe] ").Append(runLabel).Append('\n');
        sb.Append("platform=").Append(Application.platform)
          .Append(" unity=").Append(Application.unityVersion)
          .Append(" screen=").Append(Screen.width).Append('x').Append(Screen.height)
          .Append(" quality=").Append(QualitySettings.names[QualitySettings.GetQualityLevel()])
          .Append(" vsync=").Append(QualitySettings.vSyncCount)
          .Append('\n');
        sb.Append("frames=").Append(sampleCount)
          .Append(" enemies=").Append(enemyCountAtStart).Append("->").Append(enemyCountAtEnd)
          .Append('\n');
        sb.Append("| ").Append(runLabel)
          .Append(" | ").Append(avg.ToString("0.00"))
          .Append(" | ").Append(median.ToString("0.00"))
          .Append(" | ").Append(p95.ToString("0.00"))
          .Append(" | ").Append(p99.ToString("0.00"))
          .Append(" | ").Append(max.ToString("0.00"))
          .Append(" | ").Append(fps.ToString("0.0"))
          .Append(" | ").Append(cpuMs)
          .Append(" | ").Append(allocKb).Append(" (").Append(allocSource).Append(')')
          .Append(" | ").Append(gcCollections)
          .Append(" | ").Append(batches)
          .Append(" | ").Append(setPass)
          .Append(" |");

        result = sb.ToString();
        Debug.Log(result);
        GUIUtility.systemCopyBuffer = result;
    }

    static float Percentile(float[] sorted, int count, float p)
    {
        if (count == 0) return 0f;
        int idx = Mathf.Clamp(Mathf.RoundToInt(p * (count - 1)), 0, count - 1);
        return sorted[idx];
    }

    int CountEnemies()
    {
        if (string.IsNullOrEmpty(enemyTag)) return -1;
        try
        {
            GameObject[] found = GameObject.FindGameObjectsWithTag(enemyTag);
            return found != null ? found.Length : 0;
        }
        catch (UnityException)
        {
            return -1;
        }
    }

    void OnGUI()
    {
        if (!showOnScreen) return;

        GUI.color = Color.white;
        if (state == 1)
        {
            GUI.Label(new Rect(10, 10, 600, 24), "PROBE: warmup " + stateTimer.ToString("0.0"));
        }
        else if (state == 2)
        {
            GUI.Label(new Rect(10, 10, 600, 24), "PROBE: measuring " + stateTimer.ToString("0.0") + "  frames=" + sampleCount);
        }
        else if (!string.IsNullOrEmpty(result))
        {
            GUI.Label(new Rect(10, 10, 1200, 200), result);
        }
        else
        {
            GUI.Label(new Rect(10, 10, 600, 24), "PROBE: press " + startKey + " to run");
        }
    }
}