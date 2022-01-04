using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HotFix
{
    public class TestMonobehaviourClass
    {

        public static void RunTest(GameObject go) {
            go.AddComponent<TestMono>();
        }

        public static void RunTest2(GameObject go)
        {
            go.AddComponent<TestMono>();
            TestMono mono = go.GetComponent<TestMono>();
            mono.Test();
        }

        public class TestMono : MonoBehaviour {
            private float m_CurTime = 0;
            void Awake() {
                Debug.Log("TestMono Awake");
            }

            void Start()
            {
                Debug.Log("TestMono Start");
            }

            void Update()
            {
                if (m_CurTime<0.25f)
                {
                    m_CurTime += Time.deltaTime;
                    Debug.Log("TestMono Update");
                }
                
            }

            public void Test() {
                Debug.Log("TestMono Test");
            }
        }
    }
}
