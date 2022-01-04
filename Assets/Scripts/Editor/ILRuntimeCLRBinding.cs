#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;


[System.Reflection.Obfuscation(Exclude = true)]
public class ILRuntimeCLRBinding
{
    private const string DLL_PATH = "Assets/GameData/Data/HotFix/HotFix.dll.bytes";
    private const string GENERATE_PATH = "Assets/Scripts/Generated";

    // 根据热更 dll 使用的类型，自动进行全部绑定
    [MenuItem("ILRuntime/Generate CLR Binding Code by Analysis")]
    static void GenerateCLRBindingByAnalysis()
    {
        //用新的分析热更dll调用引用来生成绑定代码
        ILRuntime.Runtime.Enviorment.AppDomain domain = new ILRuntime.Runtime.Enviorment.AppDomain();
        using (System.IO.FileStream fs = new System.IO.FileStream(DLL_PATH, System.IO.FileMode.Open, System.IO.FileAccess.Read))
        {
            domain.LoadAssembly(fs);

            //Crossbind Adapter is needed to generate the correct binding code
            InitILRuntime(domain);
            ILRuntime.Runtime.CLRBinding.BindingCodeGenerator.GenerateBindingCode(domain, GENERATE_PATH);
        }

        AssetDatabase.Refresh();
    }

    static void InitILRuntime(ILRuntime.Runtime.Enviorment.AppDomain domain)
    {
        //这里需要注册所有热更DLL中用到的跨域继承Adapter，否则无法正确抓取引用
        domain.RegisterCrossBindingAdaptor(new MonoBehaviourAdaptor());
        domain.RegisterCrossBindingAdaptor(new CoroutineAdaptor());
        domain.RegisterCrossBindingAdaptor(new InheritanceAdapter());
        domain.RegisterCrossBindingAdaptor(new WindowAdaptor());
        //domain.RegisterValueTypeBinder(typeof(Vector3), new Vector3Binder());
    }
}
#endif
