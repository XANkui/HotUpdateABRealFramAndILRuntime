
using UnityEngine;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.CLR.Method;

public class WindowAdaptor : CrossBindingAdaptor
{
	

    public override System.Type BaseCLRType => typeof(Window);

    public override System.Type AdaptorType => typeof(Adaptor);

    public override object CreateCLRInstance(AppDomain appdomain, ILTypeInstance instance)
    {
        return new Adaptor(appdomain,instance);
    }

    public class Adaptor : Window,CrossBindingAdaptorType {
        AppDomain m_Appdomain;
        ILTypeInstance m_Instance;
        private object[] m_ParamList = new object[3];
        IMethod m_AwakeMethod;
        IMethod m_OnShowMethod;
        IMethod m_OnUpdateMethod;
        IMethod m_OnDisableMethod;
        IMethod m_OnCloseMethod;
        IMethod m_ToStringMethod;

        bool m_IsInvokingOnCloseMethod = false;
        public Adaptor() { }
        public Adaptor(AppDomain appdomain, ILTypeInstance instance) {
            m_Appdomain = appdomain;
            m_Instance = instance;
            
        }

        public ILTypeInstance ILInstance => m_Instance;

        public override void Awake(object param1 = null, object param2 = null, object param3 = null)
        {
            if (m_AwakeMethod==null)
            {
                m_AwakeMethod = m_Instance.Type.GetMethod("Awake",3);
            }
            if (m_AwakeMethod != null)
            {
                m_ParamList[0] = param1;
                m_ParamList[1] = param2;
                m_ParamList[2] = param3;
                m_Appdomain.Invoke(m_AwakeMethod,m_Instance,m_ParamList);
            }
        }

        public override void OnShow(object param1 = null, object param2 = null, object param3 = null)
        {
            if (m_OnShowMethod == null)
            {
                m_OnShowMethod = m_Instance.Type.GetMethod("OnShow", 3);
            }
            if (m_OnShowMethod != null)
            {
                m_ParamList[0] = param1;
                m_ParamList[1] = param2;
                m_ParamList[2] = param3;
                m_Appdomain.Invoke(m_OnShowMethod, m_Instance, m_ParamList);
            }
        }

        public override void OnUpdate()
        {
            if (m_OnUpdateMethod == null)
            {
                m_OnUpdateMethod = m_Instance.Type.GetMethod("OnUpdate", 0);
            }
            if (m_OnUpdateMethod != null)
            {
               
                m_Appdomain.Invoke(m_OnUpdateMethod, m_Instance, null);
            }
        }

        public override void OnDisable()
        {
            if (m_OnDisableMethod == null)
            {
                m_OnDisableMethod = m_Instance.Type.GetMethod("OnDisable", 0);
            }
            if (m_OnDisableMethod != null)
            {

                m_Appdomain.Invoke(m_OnDisableMethod, m_Instance, null);
            }
        }

        public override void OnClose()
        {
            if (m_OnCloseMethod==null)
            {
                m_OnCloseMethod = m_Instance.Type.GetMethod("OnClose", 0);
            }

            if (m_OnCloseMethod != null && m_IsInvokingOnCloseMethod == false)
            {
                m_IsInvokingOnCloseMethod = true;
                m_Appdomain.Invoke(m_OnCloseMethod, m_Instance, null);
                m_IsInvokingOnCloseMethod = false;

            }
            else {
                base.OnClose();
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
