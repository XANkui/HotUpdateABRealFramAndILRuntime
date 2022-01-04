using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HotFix
{
    public class TestCoroutineClass
    {
        public static void RunTest() {
            GameStart.Instance.StartCoroutine(Coroutine());
        }

        static System.Collections.IEnumerator Coroutine() {
            Debug.Log("开始协程："+ Time.time);
            yield return new WaitForSeconds(5);
            Debug.Log("结束协程："+ Time.time);
        }
    }
}
