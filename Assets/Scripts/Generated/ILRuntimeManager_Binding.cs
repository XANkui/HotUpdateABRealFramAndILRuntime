using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Reflection;
using ILRuntime.CLR.Utils;

namespace ILRuntime.Runtime.Generated
{
    unsafe class ILRuntimeManager_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            FieldInfo field;
            Type[] args;
            Type type = typeof(global::ILRuntimeManager);

            field = type.GetField("testMyDelegateFunction", flag);
            app.RegisterCLRFieldGetter(field, get_testMyDelegateFunction_0);
            app.RegisterCLRFieldSetter(field, set_testMyDelegateFunction_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_testMyDelegateFunction_0, AssignFromStack_testMyDelegateFunction_0);
            field = type.GetField("testMyDelegateMethod", flag);
            app.RegisterCLRFieldGetter(field, get_testMyDelegateMethod_1);
            app.RegisterCLRFieldSetter(field, set_testMyDelegateMethod_1);
            app.RegisterCLRFieldBinding(field, CopyToStack_testMyDelegateMethod_1, AssignFromStack_testMyDelegateMethod_1);
            field = type.GetField("testSystemActionMethod", flag);
            app.RegisterCLRFieldGetter(field, get_testSystemActionMethod_2);
            app.RegisterCLRFieldSetter(field, set_testSystemActionMethod_2);
            app.RegisterCLRFieldBinding(field, CopyToStack_testSystemActionMethod_2, AssignFromStack_testSystemActionMethod_2);


        }



        static object get_testMyDelegateFunction_0(ref object o)
        {
            return ((global::ILRuntimeManager)o).testMyDelegateFunction;
        }

        static StackObject* CopyToStack_testMyDelegateFunction_0(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((global::ILRuntimeManager)o).testMyDelegateFunction;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_testMyDelegateFunction_0(ref object o, object v)
        {
            ((global::ILRuntimeManager)o).testMyDelegateFunction = (global::TestMyDelegateFunction)v;
        }

        static StackObject* AssignFromStack_testMyDelegateFunction_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            global::TestMyDelegateFunction @testMyDelegateFunction = (global::TestMyDelegateFunction)typeof(global::TestMyDelegateFunction).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ((global::ILRuntimeManager)o).testMyDelegateFunction = @testMyDelegateFunction;
            return ptr_of_this_method;
        }

        static object get_testMyDelegateMethod_1(ref object o)
        {
            return ((global::ILRuntimeManager)o).testMyDelegateMethod;
        }

        static StackObject* CopyToStack_testMyDelegateMethod_1(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((global::ILRuntimeManager)o).testMyDelegateMethod;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_testMyDelegateMethod_1(ref object o, object v)
        {
            ((global::ILRuntimeManager)o).testMyDelegateMethod = (global::TestMyDelegateMethod)v;
        }

        static StackObject* AssignFromStack_testMyDelegateMethod_1(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            global::TestMyDelegateMethod @testMyDelegateMethod = (global::TestMyDelegateMethod)typeof(global::TestMyDelegateMethod).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ((global::ILRuntimeManager)o).testMyDelegateMethod = @testMyDelegateMethod;
            return ptr_of_this_method;
        }

        static object get_testSystemActionMethod_2(ref object o)
        {
            return ((global::ILRuntimeManager)o).testSystemActionMethod;
        }

        static StackObject* CopyToStack_testSystemActionMethod_2(ref object o, ILIntepreter __intp, StackObject* __ret, IList<object> __mStack)
        {
            var result_of_this_method = ((global::ILRuntimeManager)o).testSystemActionMethod;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_testSystemActionMethod_2(ref object o, object v)
        {
            ((global::ILRuntimeManager)o).testSystemActionMethod = (System.Action<System.String>)v;
        }

        static StackObject* AssignFromStack_testSystemActionMethod_2(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, IList<object> __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Action<System.String> @testSystemActionMethod = (System.Action<System.String>)typeof(System.Action<System.String>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            ((global::ILRuntimeManager)o).testSystemActionMethod = @testSystemActionMethod;
            return ptr_of_this_method;
        }



    }
}
