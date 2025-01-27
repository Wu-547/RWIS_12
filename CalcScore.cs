using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class CalcScore : MonoBehaviour
{
    private string _micColorInfoFileName = "MicColorInfo.txt"; 
    private string _partLogFileName = "PartLog.txt"; 
    private string _micDetectionLogFileName = "MicDetectionLog.txt"; 

    private Dictionary<string, string> micToColorMap = new Dictionary<string, string>();
    private List<LogEntry> partLog = new List<LogEntry>();
    private List<MicLogEntry> micDetectionLog = new List<MicLogEntry>();
    private static Dictionary<string, int> colorScores = new Dictionary<string, int>
    {
        { "RED", 100 },
        { "GREEN", 100 },
        { "YELLOW", 100 }
    };

    public TextMeshProUGUI scoreDisplayText; 

    [System.Serializable]
    public class LogEntry
    {
        public float time;
        public string color;
    }

    [System.Serializable]
    public class MicLogEntry
    {
        public float time;
        public string color;
        public float volume;
    }

    void Start()
    {
        
        scoreDisplayText = GameObject.Find("ScoreText").GetComponent<TextMeshProUGUI>();

        LoadMicColorInfo();
        LoadPartLog();
        LoadMicDetectionLog();
        CalculateScores();
        DisplayScores();
    }

    void LoadMicColorInfo()
    {
        string filePath = Path.Combine(Application.dataPath, _micColorInfoFileName);

        if (!File.Exists(filePath))
        {
            Debug.LogError($"MicColorInfo file not found: {filePath}");
            return;
        }

        string[] lines = File.ReadAllLines(filePath);
        foreach (string line in lines)
        {
            string[] parts = line.Split(',');
            if (parts.Length == 2)
            {
                string micName = parts[0].Trim();
                string color = parts[1].Trim();
                micToColorMap[micName] = color;
            }
        }
        Debug.Log($"Loaded {micToColorMap.Count} microphone-color mappings.");
    }

    void LoadPartLog()
    {
        string filePath = Path.Combine(Application.dataPath, _partLogFileName);

        if (!File.Exists(filePath))
        {
            Debug.LogError($"PartLog file not found: {filePath}");
            return;
        }

        string[] lines = File.ReadAllLines(filePath);
        foreach (string line in lines)
        {
            string[] parts = line.Split(',');
            if (parts.Length == 2 && float.TryParse(parts[0].Trim(), out float time))
            {
                string color = parts[1].Trim();
                partLog.Add(new LogEntry { time = time, color = color });
            }
        }
        Debug.Log($"Loaded {partLog.Count} entries from PartLog.");
    }

    void LoadMicDetectionLog()
    {
        string filePath = Path.Combine(Application.dataPath, _micDetectionLogFileName);

        if (!File.Exists(filePath))
        {
            Debug.LogError($"MicDetectionLog file not found: {filePath}");
            return;
        }

        string[] lines = File.ReadAllLines(filePath);
        foreach (string line in lines)
        {
            string[] parts = line.Split(',');
            if (parts.Length == 4 && float.TryParse(parts[0].Trim(), out float time) && float.TryParse(parts[3].Trim(), out float volume))
            {
                string micName = parts[1].Trim();
                if (micToColorMap.TryGetValue(micName, out string color))
                {
                    micDetectionLog.Add(new MicLogEntry { time = time, color = color, volume = volume });
                }
            }
        }
        Debug.Log($"Loaded {micDetectionLog.Count} entries from MicDetectionLog.");
    }

    void CalculateScores()
    {
        foreach (var partEntry in partLog)
        {
            MicLogEntry bestMatch = null;
            float maxVolume = float.MinValue;

            foreach (var micEntry in micDetectionLog)
            {
                if (Mathf.Abs(micEntry.time - partEntry.time) <= 0.3f && micEntry.color == partEntry.color)
                {
                    if (micEntry.volume > maxVolume)
                    {
                        maxVolume = micEntry.volume;
                        bestMatch = micEntry;
                    }
                }
            }

            if (bestMatch == null)
            {
               
                if (colorScores.ContainsKey(partEntry.color))
                {
                    colorScores[partEntry.color]--;
                }
            }
        }
    }

    void DisplayScores()
    {
   
        string displayText = "Final Scores:\n";
        foreach (var score in colorScores)
        {
            displayText += $"{score.Key}: {score.Value}\n";
        }

        if (scoreDisplayText != null)
        {
            scoreDisplayText.text = displayText;
        }
        else
        {
            Debug.LogError("ScoreDisplayText is not assigned in the inspector.");
        }
    }

    public static Dictionary<string, int> GetFinalScores()
    {
        return new Dictionary<string, int>(colorScores);
    }
}
