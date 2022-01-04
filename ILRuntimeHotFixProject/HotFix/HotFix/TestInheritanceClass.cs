using System;
using UnityEngine;

namespace HotFix
{
    public class TestInheritanceClass : TestClassBase
    {

        public static TestInheritanceClass GetInstance()
        { 
            return new TestInheritanceClass();
        }

        public override void TestAbstract(int arg)
        {
            Debug.Log($"TestInheritanceClass TestAbstract(int arg) arg = {arg}");
        }

        public override void TestVirtual(string str)
        {
            base.TestVirtual(str);
            Debug.Log($"TestInheritanceClass TestVirtual(string str) str = {str}");
        }
    }
}
