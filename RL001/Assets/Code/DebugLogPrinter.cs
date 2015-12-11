using UnityEngine;
using UnityEngine.UI;

namespace Code
{
    public class DebugLogPrinter : MonoBehaviour
    {
        public string Output
        {
            get;
            set;
        }

        public string Stack
        {
            get;
            set;
        }

        void OnEnable()
        {
            Application.logMessageReceived += HandleLog;
        }

        void OnDisable()
        {
            Application.logMessageReceived -= HandleLog;
        }

        void HandleLog(string logString, string stackTrace, LogType type)
        {
            Output = logString;
            Stack = stackTrace;

            Text text = GetComponent<Text>();
            if(text)
            {
                text.text = Output + "\n" + Stack;
            }
        }
    }
}
