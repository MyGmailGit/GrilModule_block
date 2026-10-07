using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Utility
{
    public class CoroutineTool : MonoBehaviour
    {
        private static CoroutineTool instance;
        public static CoroutineTool Instance => instance;
        public static void Create()
        {
            if (instance != null) return;

            var obj = new GameObject("[CoroutineTool]");
            instance = obj.AddComponent<CoroutineTool>();
        }
        void Awake()
        {
            DontDestroyOnLoad(this);
        }

        public Coroutine StartACoroutine(IEnumerator routine)
        {
            if (instance == null) return null;
            return StartCoroutine(routine);
        }

        public void StopACoroutine(Coroutine routine)
        {
            if (routine != null)
            {
                StopCoroutine(routine);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
