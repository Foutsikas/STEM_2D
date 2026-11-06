using System.Diagnostics;
using UnityEngine;

namespace STEM.Core
{
    // Editor-only logging. Calls are removed from builds, arguments included.
    public static class Log
    {
        [Conditional("UNITY_EDITOR")]
        public static void Info(object message)
        {
            UnityEngine.Debug.Log(message);
        }

        [Conditional("UNITY_EDITOR")]
        public static void Info(object message, Object context)
        {
            UnityEngine.Debug.Log(message, context);
        }
    }
}
