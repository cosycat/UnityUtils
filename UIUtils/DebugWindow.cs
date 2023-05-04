using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UnityUtils.UIUtils
{
    public class DebugWindow : MonoBehaviour
    {
        [SerializeField] private TMP_Text debugText;
        [SerializeField] [CanBeNull] private TMP_Text titleText;

        private ScrollRect scrollRect;

        private void Awake()
        {
            if (debugText == null)
            {
                debugText = gameObject.GetComponentInChildren<TMP_Text>();
                if (debugText == null)
                {
                    return;
                }
                debugText.text += "debug Text not set in inspector\n";
            }

            if (titleText != null)
            {
                titleText.text = "Debug Window";
            }
            
            // Cache references
            scrollRect = GetComponentInChildren<ScrollRect>();

            // Subscribe to log message events
            // Application.logMessageReceived += HandleLog;
            Application.logMessageReceivedThreaded += HandleLog;

            // Set the starting text
            debugText.text +=
                "Debug messages will appear here.\n"
                //+ "The [UWPDataAccess] error is being filtered, look at code if it might be an issue\n"
                ;
        }

        private void OnDestroy()
        {
            // Application.logMessageReceived -= HandleLog;
            Application.logMessageReceivedThreaded -= HandleLog;
        }

        private void HandleLog(string message, string stackTrace, LogType type)
        {
            // if (message.Equals("[UWPDataAccess] No currentSpatialCoordinateSystem or Eyes API not available!")) return;

            if (debugText.text.Length >= 5000)
            {
                debugText.text = message + "\n( " + stackTrace.Substring(0, stackTrace.Length > 250 ? 250 : stackTrace.Length) + " )\n";
            } // Substring(0, stackTrace.Length > 200 ? 200 : stackTrace.Length)
            else
            {
                debugText.text += message +"\n" + stackTrace.Substring(0, stackTrace.Length > 250 ? 250 : stackTrace.Length) +" \n";
            }

            Canvas.ForceUpdateCanvases();
            
            if (scrollRect != null)
            {
                // Scroll to the bottom when new text is added.
                scrollRect.verticalNormalizedPosition = 0;
            }
        }
    }
}