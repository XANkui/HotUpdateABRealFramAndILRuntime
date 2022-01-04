using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HotFix
{
    public class TestDelegate
    {
        static TestMyDelegateFunction testMyDelegateFunction;
        static TestMyDelegateMethod testMyDelegateMethod;
        static Action<string> testSystemActionMethod;

        public static void Initialize() {
            testMyDelegateFunction = Function;
            testMyDelegateMethod = Method;
            testSystemActionMethod = ActionMethod;
        }
        public static void RunTest()
        {
            if (testMyDelegateFunction!=null)
            {
                Debug.Log("RunTest testMyDelegateFunction () = " +testMyDelegateFunction.Invoke(56));
            }
            if (testMyDelegateMethod != null) {
                testMyDelegateMethod.Invoke(23);
            }

            if (testSystemActionMethod != null)
            {
                testSystemActionMethod.Invoke("Test Action");
            }

        }

        public static void Initialize2()
        {
            ILRuntimeManager.Instance.testMyDelegateFunction = Function;
            ILRuntimeManager.Instance.testMyDelegateMethod = Method;
            ILRuntimeManager.Instance.testSystemActionMethod = ActionMethod;
        }
        public static void RunTest2()
        {
            if (ILRuntimeManager.Instance.testMyDelegateFunction != null)
            {
                Debug.Log("RunTest testMyDelegateFunction () = " + ILRuntimeManager.Instance.testMyDelegateFunction.Invoke(1000));
            }
            if (ILRuntimeManager.Instance.testMyDelegateMethod != null)
            {
                ILRuntimeManager.Instance.testMyDelegateMethod.Invoke(2000);
            }

            if (ILRuntimeManager.Instance.testSystemActionMethod != null)
            {
                ILRuntimeManager.Instance.testSystemActionMethod.Invoke("3000 Test Action");
            }

        }

        static void Method(int arg) {
            Debug.Log("TestDelegate Method(int arg) arg = "+ arg);
        }
        static string Function(int arg) {
            Debug.Log("TestDelegate Function(int arg) arg = " + arg);
            return arg.ToString();
        }

        static void ActionMethod(string arg)
        {
            Debug.Log("TestDelegate ActionMethod(string arg) arg = " + arg);
        }
    }
}
