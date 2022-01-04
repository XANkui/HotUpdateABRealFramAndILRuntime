using System;
using UnityEngine;

namespace HotFix
{
    public class TestClass
    {
        private int m_ID;
        public int ID { get => m_ID; }
        public TestClass() {
            m_ID = -1;
        }
        public TestClass(int id) {

            m_ID = id;
        }
        public static void StaticTestFunc() {
            Debug.Log("StaticTestFunc");
        }

        public static void StaticTestFunc2(int param)
        {
            Debug.Log("StaticTestFunc2(int param)param = " + param);
        }

        public static void GenericMethod<T>(T t)
        {
            Debug.Log("GenericMethod<T>(T t) t = " + t);
        }
    }
}
