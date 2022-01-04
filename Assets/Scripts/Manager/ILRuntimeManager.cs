
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#region 测试跨域自定义类


// 测试自定义基类
public abstract class TestClassBase {
	
	public virtual int Value { get => 100; }

	public virtual void TestVirtual(string str) {
		Debug.Log($"TestVirtual(string str) str = {str}");
	}

	public abstract void TestAbstract(int arg);
}

public class InheritanceAdapter : CrossBindingAdaptor {
    public override System.Type BaseCLRType => typeof(TestClassBase);

    public override System.Type AdaptorType => typeof(Adapter);

    public override object CreateCLRInstance(AppDomain appdomain, ILTypeInstance instance)
    {
        return new Adapter(appdomain,instance);
    }

	class Adapter : TestClassBase, CrossBindingAdaptorType {
		AppDomain m_Appdomain;
		ILTypeInstance m_Instance;
		IMethod m_TestAbstractMethod;
		IMethod m_TestVirtualMethod;
		IMethod m_GetValueMethod; // get 属性
		IMethod m_ToStringMethod;
		object[] m_Param = new object[1];
		bool m_IsInvokeingTestVirtualMethod = false; // 是否正在调用虚方法
		bool m_IsInvokeingGetValueMethod = false; // 是否正在调用属性方法
		public Adapter() { }
		public Adapter(AppDomain appdomain, ILTypeInstance instance) {
			m_Appdomain = appdomain;
			m_Instance = instance;
		}

		public ILTypeInstance ILInstance => m_Instance;

		// 在适配其中重写所有需要在热更脚本重写的方法
		// 并且将控制权转义到脚本里去
		public override void TestAbstract(int arg)
        {
            if (m_TestAbstractMethod==null)
            {
				m_TestAbstractMethod = m_Instance.Type.GetMethod("TestAbstract", 1);
			}
			if (m_TestAbstractMethod != null)
            {
				m_Param[0] = arg;
				m_Appdomain.Invoke(m_TestAbstractMethod, m_Instance, m_Param);
            }
		}

		// 注意避免造成循环调用
        public override void TestVirtual(string str)
        {
			if (m_TestVirtualMethod == null)
			{
				m_TestVirtualMethod = m_Instance.Type.GetMethod("TestAbstract", 1);
			}

			// 必须要设定一个标识，来表示当前是否在调用中，
			// 否则如果脚本里调用了 base.方法，就会造成无限循环调用
			if (m_TestVirtualMethod != null && m_IsInvokeingTestVirtualMethod == false)
			{
				m_IsInvokeingTestVirtualMethod = true;
				m_Param[0] = str;
				m_Appdomain.Invoke(m_TestVirtualMethod, m_Instance, m_Param);
				m_IsInvokeingTestVirtualMethod = false;
			}
			else {
				base.TestVirtual(str);
			}
		}

        // Get 属性
        public override int Value {
			get {
                if (m_GetValueMethod==null)
                {
					m_GetValueMethod = m_Instance.Type.GetMethod("get_value",1);

				}

				if (m_GetValueMethod != null && m_IsInvokeingGetValueMethod == false)
				{
					m_IsInvokeingGetValueMethod = true;
					int res = (int)m_Appdomain.Invoke(m_GetValueMethod, m_Instance, null);
					m_IsInvokeingGetValueMethod = false;
					return res;
				}
				else {
					return base.Value;
				}
			}
		}

        // 自带 ToString
        public override string ToString()
		{
			if (m_ToStringMethod == null)
			{
				m_ToStringMethod = m_Appdomain.ObjectType.GetMethod("ToString",0);
            }
			IMethod toStringM = m_Instance.Type.GetVirtualMethod(m_ToStringMethod);
			if (toStringM == null || toStringM is ILMethod)
			{
				return m_Instance.ToString();
			}
			else {
				return m_Instance.Type.FullName;
			}
        }
    }
}

#endregion

#region 测试绑定

public class CLRBindingTestClass { 
	public static float DoSomeTest(int a,float b)
    {

		return a + b;
    }
}

#endregion

#region 测试协程适配器
public class CoroutineAdaptor : CrossBindingAdaptor {
    public override System.Type BaseCLRType => null;

    public override System.Type AdaptorType => typeof(Adaptor);

    public override System.Type[] BaseCLRTypes => new System.Type[] { 
		typeof(IEnumerator<object>), typeof(IEnumerator),typeof(System.IDisposable)
	};

    public override object CreateCLRInstance(AppDomain appdomain, ILTypeInstance instance)
    {
		return new Adaptor(appdomain, instance);
    }

	public class Adaptor:IEnumerator<System.Object>,IEnumerator, System.IDisposable,CrossBindingAdaptorType {
		AppDomain m_Appdomain; 
		ILTypeInstance m_Instance;
		IMethod m_CurMetod;
		IMethod m_MoveNextMetod;
		IMethod m_ResetMetod;
		IMethod m_DisposeMetod;
		IMethod m_ToStringMethod;
		public Adaptor() { }
		public Adaptor(AppDomain appdomain, ILTypeInstance instance) {

			m_Appdomain = appdomain;
			m_Instance = instance;
		}

        public object Current {
			get {
                if (m_CurMetod==null)
                {
					m_CurMetod = m_Instance.Type.GetMethod("get_Current",0);
                    if (m_CurMetod==null)
                    {
						m_CurMetod = m_Instance.Type.GetMethod("System.Collections.IEnumerator.get_Current", 0);
					}
				}

				if (m_CurMetod != null)
				{
					var res = m_Appdomain.Invoke(m_CurMetod, m_Instance, null);
					return res;
				}
				else {
					return null;
				}
			}
		}

        public bool MoveNext()
        {
			if (m_MoveNextMetod == null)
			{
				m_MoveNextMetod = m_Instance.Type.GetMethod("MoveNext", 0);
				
			}

			if (m_MoveNextMetod != null)
			{
				return (bool)m_Appdomain.Invoke(m_MoveNextMetod, m_Instance, null);
			}
			else
			{
				return false;
			}
		}

        public void Reset()
        {
			
			if (m_ResetMetod == null)
			{
				m_ResetMetod = m_Instance.Type.GetMethod("Reset", 0);

			}

			if (m_ResetMetod != null)
			{
				m_Appdomain.Invoke(m_ResetMetod, m_Instance, null);
			}
			
		}

        public void Dispose()
        {
			if (m_DisposeMetod == null)
			{
				m_DisposeMetod = m_Instance.Type.GetMethod("Dispose", 0);
				if (m_DisposeMetod == null)
				{
					m_DisposeMetod = m_Instance.Type.GetMethod("System.IDisposable.Dispose", 0);
				}
			}

			if (m_DisposeMetod != null)
			{
				m_Appdomain.Invoke(m_DisposeMetod, m_Instance, null);		
			}
			
		}

		// 自带 ToString
		public override string ToString()
		{
			if (m_ToStringMethod == null)
			{
				m_ToStringMethod = m_Appdomain.ObjectType.GetMethod("ToString", 0);
			}
			IMethod toStringM = m_Instance.Type.GetVirtualMethod(m_ToStringMethod);
			if (toStringM == null || toStringM is ILMethod)
			{
				return m_Instance.ToString();
			}
			else
			{
				return m_Instance.Type.FullName;
			}
		}

		public ILTypeInstance ILInstance => m_Instance;
    }
}
#endregion

#region 测试 MonoBehaviour 适配器
public class MonoBehaviourAdaptor : CrossBindingAdaptor {
    public override System.Type BaseCLRType => typeof(MonoBehaviour);

    public override System.Type AdaptorType => typeof(Adaptor);

    public override object CreateCLRInstance(AppDomain appdomain, ILTypeInstance instance)
    {
		return new Adaptor(appdomain, instance);
    }

	public class Adaptor : MonoBehaviour,CrossBindingAdaptorType {
		AppDomain m_Appdomain;
		ILTypeInstance m_Instance;
		IMethod m_AwakeMethod;
		IMethod m_StartMethod;
		IMethod m_UpdateMethod;
		IMethod m_ToStringMethod;
		public Adaptor() { }
		public Adaptor(AppDomain appdomain, ILTypeInstance instance) {
			m_Appdomain = appdomain;
			m_Instance = instance;
		}

        public ILTypeInstance ILInstance { get => m_Instance; set => m_Instance = value; }
        public AppDomain AppDomain { get => m_Appdomain; set => m_Appdomain = value; }

		public void Awake() {
            if (m_Instance!=null)
            {
                if (m_AwakeMethod==null)
                {
					m_AwakeMethod = m_Instance.Type.GetMethod("Awake",0);
                }

                if (m_AwakeMethod!=null)
                {
					m_Appdomain.Invoke(m_AwakeMethod,m_Instance,null);
                }
            }
		}

		void Start()
		{
			if (m_StartMethod == null)
			{
				m_StartMethod = m_Instance.Type.GetMethod("Start", 0);
			}

			if (m_StartMethod != null)
			{
				m_Appdomain.Invoke(m_StartMethod, m_Instance, null);
			}
		}

		void Update()
		{
			if (m_UpdateMethod == null)
			{
				m_UpdateMethod = m_Instance.Type.GetMethod("Update", 0);
			}

			if (m_UpdateMethod != null)
			{
				m_Appdomain.Invoke(m_UpdateMethod, m_Instance, null);
			}
		}

		// 自带 ToString
		public override string ToString()
		{
			if (m_ToStringMethod == null)
			{
				m_ToStringMethod = m_Appdomain.ObjectType.GetMethod("ToString", 0);
			}
			IMethod toStringM = m_Instance.Type.GetVirtualMethod(m_ToStringMethod);
			if (toStringM == null || toStringM is ILMethod)
			{
				return m_Instance.ToString();
			}
			else
			{
				return m_Instance.Type.FullName;
			}
		}
	}
}
#endregion

#region 测试自定义委托
// 测试自定义委托定义
public delegate void TestMyDelegateMethod(int arg);
public delegate string TestMyDelegateFunction(int arg);
#endregion

public class ILRuntimeManager : Singleton<ILRuntimeManager>
{
    private const string DLL_PATH = "Assets/GameData/Data/HotFix/HotFix.dll.bytes";
    private const string PDB_PATH = "Assets/GameData/Data/HotFix/HotFix.pdb.bytes";

    AppDomain m_AppDomain;
	MemoryStream m_Dll;
	MemoryStream m_Pdb;
	public void Init() {
		LoadHotFixAssembly();
	}

	public void OnDestroy()
	{
		if (m_Dll != null)
			m_Dll.Close();
		if (m_Pdb != null)
			m_Pdb.Close();
		m_Dll = null;
		m_Pdb = null;
	}
	void LoadHotFixAssembly() {
		// 整个工程中只有一个 ILRunmtime 的 AppDomain
		m_AppDomain = new AppDomain();
		// 读取热更资源的dll
		TextAsset dllText = ResourceManager.Instance.LoadResource<TextAsset>(DLL_PATH);
		// pdb 文件，调试数据，日志报错使用
		TextAsset pdbText = ResourceManager.Instance.LoadResource<TextAsset>(PDB_PATH);

        try
        {
			m_Dll = new MemoryStream(dllText.bytes);
			m_Pdb = new MemoryStream(pdbText.bytes);
			m_AppDomain.LoadAssembly(m_Dll, m_Pdb, new ILRuntime.Mono.Cecil.Pdb.PdbReaderProvider());
		}
        catch (System.Exception e)
        {

			Debug.LogError($"ILRuntime 读取错误，请检查：{e}");
			return;
        }
		

		InitializaILRuntime();
		OnHotFixLoaded();

	}

	void InitializaILRuntime() {
		DelegateRegisterConvertor();
		InheritanceClassAdapterRegister();
		CoroutineClassAdapterRegister();
		MonobehaviourClassAdapterRegister();
		SetupCLRAddComponentRedirectionRegister();
		SetupCLRGetComponentRedirectionRegister();
		CLRBindingRegister();
	}

	// 跨域委托注册
	void DelegateRegisterConvertor() {
		// (自定义委托)跨域调用，先转为默认的 Action / Function 类型委托
		m_AppDomain.DelegateManager.RegisterDelegateConvertor<TestMyDelegateMethod>((action) => {
			return new TestMyDelegateMethod((a) => {
				((System.Action<int>)action)(a);
			});
		});
		m_AppDomain.DelegateManager.RegisterDelegateConvertor<TestMyDelegateFunction>((function) => {
			return new TestMyDelegateFunction((foo) => {
				return ((System.Func<int, string>)(function))(foo);
			});
		});

		// （默认委托）跨域委托调用的时候注意，这里要注册(委托参数类型 method 类似 Action 委托；Function 类似 Function)
		m_AppDomain.DelegateManager.RegisterMethodDelegate<string>();
		m_AppDomain.DelegateManager.RegisterMethodDelegate<int>();
		m_AppDomain.DelegateManager.RegisterFunctionDelegate<int, string>();
	}

	// 跨域继承类适配器注册
	private void InheritanceClassAdapterRegister() {
		m_AppDomain.RegisterCrossBindingAdaptor(new InheritanceAdapter());
	}

	// 协程适配器注册
	private void CoroutineClassAdapterRegister()
	{
		m_AppDomain.RegisterCrossBindingAdaptor(new CoroutineAdaptor());
	}

	// MonoBehaviour适配器注册
	private void MonobehaviourClassAdapterRegister()
	{
		m_AppDomain.RegisterCrossBindingAdaptor(new MonoBehaviourAdaptor());
	}

	// 绑定注册
	// 绑定能够大大提升代码效率 （最后绑定注册）
	private void CLRBindingRegister() {
		ILRuntime.Runtime.Generated.CLRBindings.Initialize(m_AppDomain); // ILRuntime.Runtime.Generated 只有在生成的 绑定才会有该函数
	}

	void OnHotFixLoaded() {
		//TestCallFunc1();
		//TestCallFunc2();
		//TestCallFunc3();
		//TestCallFunc4();
		//TestCallFunc5();
		//TestCallFunc6();
		//TestCallFunc7();
		//TestCallFunc8();
		//TestCallFunc9();
		//TestCallFunc10();
		//TestCallFunc11();
		//TestCallFunc12();
		TestCallFunc13();
	}

	#region Test
	private void TestCallFunc1() {
		m_AppDomain.Invoke("HotFix.TestClass", "StaticTestFunc", null, null);
	}

	// 获取 类
	private void TestCallFunc2()
	{
		// 先单独获取类，之后就可以一直使用该类进行该类的函数调用
		IType type = m_AppDomain.LoadedTypes["HotFix.TestClass"];
		// 获取该类的某方法，然后调用
		IMethod method = type.GetMethod("StaticTestFunc",0);
		m_AppDomain.Invoke(method,null,null);
	}

	// 带参数函数的调用方法1
	private void TestCallFunc3()
	{
		// 先单独获取类，之后就可以一直使用该类进行该类的函数调用
		IType type = m_AppDomain.LoadedTypes["HotFix.TestClass"];
		// 获取该类的某方法，然后调用
		IMethod method = type.GetMethod("StaticTestFunc2", 1);
		m_AppDomain.Invoke(method, null, 5);
	}

	// 带参数函数的调用方法2
	private void TestCallFunc4()
	{
		// 先单独获取类，之后就可以一直使用该类进行该类的函数调用
		IType type = m_AppDomain.LoadedTypes["HotFix.TestClass"];
		// 指定参数类型
		IType intTYpe = m_AppDomain.GetType(typeof(int));
		List<IType> paraList = new List<IType>();
		paraList.Add(intTYpe);
		// 获取该类的某方法，然后调用
		IMethod method = type.GetMethod("StaticTestFunc2", paraList,null);
		m_AppDomain.Invoke(method, null, 10);
	}

	// 实例化
	private void TestCallFunc5() {
		// 先单独获取类，之后就可以一直使用该类进行该类的函数调用
		IType type = m_AppDomain.LoadedTypes["HotFix.TestClass"];

		// 实例化（不带参数）
		object obj = ((ILType)type).Instantiate();
		int id = (int)m_AppDomain.Invoke("HotFix.TestClass","get_ID",obj,null);
		Debug.Log($"实例化（不带参数）ID = {id}");

		// 第二种实例化（可带参数）
		obj = m_AppDomain.Instantiate("HotFix.TestClass",new object[] {6 });
		id = (int)m_AppDomain.Invoke("HotFix.TestClass", "get_ID", obj, null);
		Debug.Log($"实例化（不带参数）ID = {id}");
	}

	// 调用泛型函数
	private void TestCallFunc6()
	{

		// 第一种泛型调用方法
		IType stringType = m_AppDomain.GetType(typeof(string));
		IType[] genericArguments = new IType[] { stringType};
		m_AppDomain.InvokeGenericMethod("HotFix.TestClass", "GenericMethod",genericArguments,null,"Xan 11");

		//第二种泛型调用方法
		// 先单独获取类，之后就可以一直使用该类进行该类的函数调用
		IType type = m_AppDomain.LoadedTypes["HotFix.TestClass"];
		List<IType> paraList = new List<IType>();
		paraList.Add(stringType);
		IMethod method = type.GetMethod("GenericMethod",paraList,genericArguments);
		m_AppDomain.Invoke(method,null,"Xan 22");
	}

	// 委托调用
	private void TestCallFunc7()
	{
		// 先单独获取类，之后就可以一直使用该类进行该类的函数调用
		IType type = m_AppDomain.LoadedTypes["HotFix.TestDelegate"];
		// 获取该类的某方法，然后调用
		IMethod method1 = type.GetMethod("Initialize", 0);
		IMethod method2 = type.GetMethod("RunTest", 0);
		m_AppDomain.Invoke(method1, null, null);
		m_AppDomain.Invoke(method2, null, null);
	}

	public TestMyDelegateFunction testMyDelegateFunction;
	public TestMyDelegateMethod testMyDelegateMethod;
	public System.Action<string> testSystemActionMethod;
	// 跨域调用委托
	private void TestCallFunc8()
	{
		// 先单独获取类，之后就可以一直使用该类进行该类的函数调用
		IType type = m_AppDomain.LoadedTypes["HotFix.TestDelegate"];
		// 获取该类的某方法，然后调用
		IMethod method1 = type.GetMethod("Initialize2", 0);
		IMethod method2 = type.GetMethod("RunTest2", 0);
		m_AppDomain.Invoke(method1, null, null);
		m_AppDomain.Invoke(method2, null, null);

		// 自行调用测试
		testMyDelegateFunction.Invoke(10000);
		testMyDelegateMethod.Invoke(20000);
		testSystemActionMethod.Invoke("30000 Test Action");
	}

	// 跨域继承（集成类需要适配器，较麻烦）
	private void TestCallFunc9() {
		//方法1： 获取实例，并且调用
		TestClassBase testClassBase = m_AppDomain.Instantiate<TestClassBase>("HotFix.TestInheritanceClass");
		testClassBase.TestAbstract(1000);
		testClassBase.TestVirtual("TestVirtual ");
		Debug.Log("testClassBase.Value (Get)1 = " + testClassBase.Value);

		//方法2： 获取实例，并且调用
		TestClassBase testClassBase2 = (TestClassBase)m_AppDomain.Invoke("HotFix.TestInheritanceClass", "GetInstance",null,null);
		testClassBase2.TestAbstract(2000);
		testClassBase2.TestVirtual("TestVirtual2 ");
		Debug.Log("testClassBase.Value (Get)2 = " + testClassBase2.Value);
	}

	// 测试绑定
	private void TestCallFunc10() {
		// 未绑定前耗时平均：6248343
		long curTime = System.DateTime.Now.Ticks;
		m_AppDomain.Invoke("HotFix.TestCLRBinding", "RunTest",null,null);
		Debug.Log($"使用时间：{System.DateTime.Now.Ticks - curTime}");
	}

	// 测试协程适配器
	private void TestCallFunc11() {
		m_AppDomain.Invoke("HotFix.TestCoroutineClass", "RunTest", null, null);
	}

	#region 测试 MonoBeaviour 适配器

	private void TestCallFunc12()
	{
		m_AppDomain.Invoke("HotFix.TestMonobehaviourClass", "RunTest", null, GameStart.Instance.gameObject);
	}

	private void TestCallFunc13()
	{
		m_AppDomain.Invoke("HotFix.TestMonobehaviourClass", "RunTest2", null, GameStart.Instance.gameObject);
	}

	unsafe void SetupCLRGetComponentRedirectionRegister()
	{
		var arr = typeof(GameObject).GetMethods();
		foreach (var i in arr)
		{
			if (i.Name == "GetComponent" && i.GetGenericArguments().Length == 1)
			{
				m_AppDomain.RegisterCLRMethodRedirection(i, GetComponent);
			}
		}
	}

	unsafe void SetupCLRAddComponentRedirectionRegister() {
		var arr = typeof(GameObject).GetMethods();
        foreach (var i in arr)
        {
            if (i.Name == "AddComponent" && i.GetGenericArguments().Length==1)
            {
				m_AppDomain.RegisterCLRMethodRedirection(i, AddComponent);
            }
        }
	}

	private unsafe StackObject* GetComponent(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
	{

		AppDomain __domain = __intp.AppDomain;
		var ptr = __esp - 1;
		GameObject instance = StackObject.ToObject(ptr, __domain, __mStack) as GameObject;
		if (instance == null)
		{
			throw new System.NullReferenceException();
		}

		__intp.Free(ptr);

		var genericArgument = __method.GenericArguments;
		if (genericArgument != null && genericArgument.Length == 1)
		{
			var type = genericArgument[0];
			object res = null;
			if (type is CLRType) // CLRType 表示这个类型时 Unity 工程里的类型 ，ILType 表示是热更dll 里面的类型
			{
				// Unity 主工程的类，不需要做处理
				res = instance.GetComponent(type.TypeForCLR);
			}
			else
			{
				var clrInstances = instance.GetComponents<MonoBehaviourAdaptor.Adaptor>();
                foreach (var clrInstance in clrInstances)
                {
                    if (clrInstance.ILInstance!=null)
                    {
                        if (clrInstance.ILInstance.Type==type)
                        {
							res = clrInstance.ILInstance;
							break;
                        }
                    }
                }
			}

			return ILIntepreter.PushObject(ptr, __mStack, res);
		}

		return __esp;
	}

	private unsafe StackObject* AddComponent(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
	{

		AppDomain __domain = __intp.AppDomain;
		var ptr = __esp - 1;
		GameObject instance = StackObject.ToObject(ptr,__domain,__mStack) as GameObject;
        if (instance==null)
        {
			throw new System.NullReferenceException();
        }

		__intp.Free(ptr);

		var genericArgument = __method.GenericArguments;
        if (genericArgument!=null && genericArgument.Length ==1)
        {
			var type = genericArgument[0];
			object res;
			if (type is CLRType) // CLRType 表示这个类型时 Unity 工程里的类型 ，ILType 表示是热更dll 里面的类型
			{
				// Unity 主工程的类，不需要做处理
				res = instance.AddComponent(type.TypeForCLR);
			}
			else {
				// 创建 出来 MonoTest
				var ilInstance = new ILTypeInstance(type as ILType,false);
				var clrInstance = instance.AddComponent<MonoBehaviourAdaptor.Adaptor>();
				clrInstance.ILInstance = ilInstance;
				clrInstance.AppDomain = __domain;
				// 这个实例默认创建CLRInstance不是通过AddComponent 出来的有效实例，所以需要替换
				ilInstance.CLRInstance = clrInstance;

				res = clrInstance.ILInstance;

				// 补调用Awake
				clrInstance.Awake();
			}

			return ILIntepreter.PushObject(ptr, __mStack, res);
        }

		return __esp;
	}	

    #endregion

    #endregion
}
