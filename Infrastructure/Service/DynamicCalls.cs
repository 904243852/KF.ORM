using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace KF.ORM.Infrastructure.Service
{
    /// <summary>Delegate for calling a method that is not known at runtime.</summary>
    /// <param name="target">the object to be called or null if the call is to a static method.</param>
    /// <param name="parameters">the parameters to the method.</param>
    /// <returns>the return value for the method or null if it doesn't return anything.</returns>
    internal delegate object InvokeHandler(object target, object[] parameters);

    /// <summary>Delegate for creating and object at runtime using the default constructor.</summary>
    /// <returns>the newly created object.</returns>
    internal delegate object CreateInstanceHandler();

    /// <summary>Delegate to get an arbitraty property at runtime.</summary>
    /// <param name="target">the object instance whose property will be obtained.</param>
    /// <returns>the property value.</returns>
    internal delegate object PropertyGetHandler(object target);

    /// <summary>Delegate to set an arbitrary property at runtime.</summary>
    /// <param name="target">the object instance whose property will be modified.</param>
    /// <param name="parameter"></param>
    internal delegate void PropertySetHandler(object target, object parameter);

    /// <summary>Class with helper methods for dynamic invocation generating IL on the fly.</summary>
    internal static class DynamicCalls
    {
        /// <summary>
        /// 用于存放GetMethodInvoker的Dictionary
        /// </summary>
        private static Dictionary<MethodInfo, InvokeHandler> _dictInvoker = new Dictionary<MethodInfo, InvokeHandler>();

        /// <summary>
        /// 获取方法执行对象
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        public static InvokeHandler GetMethodInvoker(MethodInfo methodInfo)
        {
            lock (_dictInvoker)
            {
                if (_dictInvoker.ContainsKey(methodInfo)) return _dictInvoker[methodInfo];

                // generates a dynamic method to generate a InvokeHandler delegate
                DynamicMethod dynamicMethod = new DynamicMethod(string.Empty, typeof(object), new[] { typeof(object), typeof(object[]) }, methodInfo.DeclaringType.Module);

                ILGenerator ilGenerator = dynamicMethod.GetILGenerator();

                ParameterInfo[] parameters = methodInfo.GetParameters();

                Type[] paramTypes = new Type[parameters.Length];

                // copies the parameter types to an array
                for (int i = 0; i < paramTypes.Length; i++)
                {
                    if (parameters[i].ParameterType.IsByRef)
                        paramTypes[i] = parameters[i].ParameterType.GetElementType();
                    else
                        paramTypes[i] = parameters[i].ParameterType;
                }

                LocalBuilder[] locals = new LocalBuilder[paramTypes.Length];

                // generates a local variable for each parameter
                for (int i = 0; i < paramTypes.Length; i++)
                {
                    locals[i] = ilGenerator.DeclareLocal(paramTypes[i], true);
                }

                // creates code to copy the parameters to the local variables
                for (int i = 0; i < paramTypes.Length; i++)
                {
                    ilGenerator.Emit(OpCodes.Ldarg_1);
                    EmitInt(ilGenerator, i);
                    ilGenerator.Emit(OpCodes.Ldelem_Ref);
                    EmitCastToReference(ilGenerator, paramTypes[i]);
                    ilGenerator.Emit(OpCodes.Stloc, locals[i]);
                }

                if (!methodInfo.IsStatic)
                {
                    // loads the object into the stack
                    ilGenerator.Emit(OpCodes.Ldarg_0);
                }

                // loads the parameters copied to the local variables into the stack
                for (int i = 0; i < paramTypes.Length; i++)
                {
                    if (parameters[i].ParameterType.IsByRef)
                        ilGenerator.Emit(OpCodes.Ldloca_S, locals[i]);
                    else
                        ilGenerator.Emit(OpCodes.Ldloc, locals[i]);
                }

                // calls the method
                if (!methodInfo.IsStatic)
                {
                    ilGenerator.EmitCall(OpCodes.Callvirt, methodInfo, null);
                }
                else
                {
                    ilGenerator.EmitCall(OpCodes.Call, methodInfo, null);
                }

                // creates code for handling the return value
                if (methodInfo.ReturnType == typeof(void))
                {
                    ilGenerator.Emit(OpCodes.Ldnull);
                }
                else
                {
                    EmitBoxIfNeeded(ilGenerator, methodInfo.ReturnType);
                }

                // iterates through the parameters updating the parameters passed by ref
                for (int i = 0; i < paramTypes.Length; i++)
                {
                    if (parameters[i].ParameterType.IsByRef)
                    {
                        ilGenerator.Emit(OpCodes.Ldarg_1);
                        EmitInt(ilGenerator, i);
                        ilGenerator.Emit(OpCodes.Ldloc, locals[i]);
                        if (locals[i].LocalType.IsValueType)
                            ilGenerator.Emit(OpCodes.Box, locals[i].LocalType);
                        ilGenerator.Emit(OpCodes.Stelem_Ref);
                    }
                }

                // returns the value to the caller
                ilGenerator.Emit(OpCodes.Ret);

                // converts the DynamicMethod to a InvokeHandler delegate to call to the method
                InvokeHandler invoker = (InvokeHandler)dynamicMethod.CreateDelegate(typeof(InvokeHandler));

                _dictInvoker.Add(methodInfo, invoker);

                return invoker;
            }
        }

        /// <summary>
        /// 用于存放GetInstanceCreator的Dictionary
        /// </summary>
        private static Dictionary<Type, CreateInstanceHandler> _dictCreator = new Dictionary<Type, CreateInstanceHandler>();

        /// <summary>Gets the instance creator delegate that can be use to create instances of the specified type.</summary>
        /// <param name="type">The type of the objects we want to create.</param>
        /// <returns>A delegate that can be used to create the objects.</returns>
        public static CreateInstanceHandler GetInstanceCreator(Type type)
        {
            lock (_dictCreator)
            {
                if (_dictCreator.ContainsKey(type)) return _dictCreator[type];

                // generates a dynamic method to generate a CreateInstanceHandler delegate
                DynamicMethod dynamicMethod = new DynamicMethod(string.Empty, type, new Type[0], typeof(DynamicCalls).Module);

                ILGenerator ilGenerator = dynamicMethod.GetILGenerator();

                // generates code to create a new object of the specified type using the default constructor
                ilGenerator.Emit(OpCodes.Newobj, type.GetConstructor(Type.EmptyTypes));

                // returns the value to the caller
                ilGenerator.Emit(OpCodes.Ret);

                // converts the DynamicMethod to a CreateInstanceHandler delegate to create the object
                CreateInstanceHandler creator =
                    (CreateInstanceHandler)dynamicMethod.CreateDelegate(typeof(CreateInstanceHandler));

                _dictCreator.Add(type, creator);

                return creator;
            }
        }

        /// <summary>
        /// 用于存放GetPropertyGetter的Dictionary
        /// </summary>
        private static Dictionary<PropertyInfo, PropertyGetHandler> _dictGetter = new Dictionary<PropertyInfo, PropertyGetHandler>();

        /// <summary>
        /// 获取属性获取对象
        /// </summary>
        /// <param name="propInfo"></param>
        /// <returns></returns>
        public static PropertyGetHandler GetPropertyGetter(PropertyInfo propInfo)
        {
            lock (_dictGetter)
            {
                if (_dictGetter.ContainsKey(propInfo)) return _dictGetter[propInfo];

                // generates a dynamic method to generate a PropertyGetHandler delegate
                DynamicMethod dynamicMethod = new DynamicMethod(string.Empty, typeof(object), new[] { typeof(object) }, propInfo.DeclaringType.Module);

                ILGenerator ilGenerator = dynamicMethod.GetILGenerator();

                // loads the object into the stack
                ilGenerator.Emit(OpCodes.Ldarg_0);

                // calls the getter
                ilGenerator.EmitCall(OpCodes.Callvirt, propInfo.GetGetMethod(), null);

                // creates code for handling the return value
                EmitBoxIfNeeded(ilGenerator, propInfo.PropertyType);

                // returns the value to the caller
                ilGenerator.Emit(OpCodes.Ret);

                // converts the DynamicMethod to a PropertyGetHandler delegate to get the property
                PropertyGetHandler getter = (PropertyGetHandler)dynamicMethod.CreateDelegate(typeof(PropertyGetHandler));

                _dictGetter.Add(propInfo, getter);

                return getter;
            }
        }

        /// <summary>
        /// 用于存放SetPropertySetter的Dictionary
        /// </summary>
        private static Dictionary<PropertyInfo, PropertySetHandler> _dictSetter = new Dictionary<PropertyInfo, PropertySetHandler>();

        /// <summary>
        /// 获取属性设置对象
        /// </summary>
        /// <param name="propInfo"></param>
        /// <returns></returns>
        public static PropertySetHandler GetPropertySetter(PropertyInfo propInfo)
        {
            lock (_dictSetter)
            {
                if (_dictSetter.ContainsKey(propInfo)) return _dictSetter[propInfo];

                // generates a dynamic method to generate a PropertySetHandler delegate
                DynamicMethod dynamicMethod = new DynamicMethod(string.Empty, null, new[] { typeof(object), typeof(object) }, propInfo.DeclaringType.Module);

                ILGenerator ilGenerator = dynamicMethod.GetILGenerator();

                // loads the object into the stack
                ilGenerator.Emit(OpCodes.Ldarg_0);

                // loads the parameter from the stack
                ilGenerator.Emit(OpCodes.Ldarg_1);

                // cast to the proper type (unboxing if needed)
                EmitCastToReference(ilGenerator, propInfo.PropertyType);

                // calls the setter
                ilGenerator.EmitCall(OpCodes.Callvirt, propInfo.SetMethod, null);

                // terminates the call
                ilGenerator.Emit(OpCodes.Ret);

                // converts the DynamicMethod to a PropertyGetHandler delegate to get the property
                PropertySetHandler setter = (PropertySetHandler)dynamicMethod.CreateDelegate(typeof(PropertySetHandler));

                _dictSetter.Add(propInfo, setter);

                return setter;
            }
        }

        /// <summary>Emits the cast to a reference, unboxing if needed.</summary>
        /// <param name="ilGenerator"></param>
        /// <param name="type">The type to cast.</param>
        private static void EmitCastToReference(ILGenerator ilGenerator, Type type)
        {
            if (type.IsValueType)
            {
                ilGenerator.Emit(OpCodes.Unbox_Any, type);
            }
            else
            {
                ilGenerator.Emit(OpCodes.Castclass, type);
            }
        }

        /// <summary>Boxes a type if needed.</summary>
        /// <param name="ilGenerator">The MSIL generator.</param>
        /// <param name="type">The type.</param>
        private static void EmitBoxIfNeeded(ILGenerator ilGenerator, Type type)
        {
            if (type.IsValueType)
            {
                ilGenerator.Emit(OpCodes.Box, type);
            }
        }

        /// <summary>Emits code to save an integer to the evaluation stack.</summary>
        /// <param name="ilGenerator"></param>
        /// <param name="value">The value to push.</param>
        private static void EmitInt(ILGenerator ilGenerator, int value)
        {
            // for small integers, emit the proper opcode
            switch (value)
            {
                case -1:
                    ilGenerator.Emit(OpCodes.Ldc_I4_M1);
                    return;
                case 0:
                    ilGenerator.Emit(OpCodes.Ldc_I4_0);
                    return;
                case 1:
                    ilGenerator.Emit(OpCodes.Ldc_I4_1);
                    return;
                case 2:
                    ilGenerator.Emit(OpCodes.Ldc_I4_2);
                    return;
                case 3:
                    ilGenerator.Emit(OpCodes.Ldc_I4_3);
                    return;
                case 4:
                    ilGenerator.Emit(OpCodes.Ldc_I4_4);
                    return;
                case 5:
                    ilGenerator.Emit(OpCodes.Ldc_I4_5);
                    return;
                case 6:
                    ilGenerator.Emit(OpCodes.Ldc_I4_6);
                    return;
                case 7:
                    ilGenerator.Emit(OpCodes.Ldc_I4_7);
                    return;
                case 8:
                    ilGenerator.Emit(OpCodes.Ldc_I4_8);
                    return;
            }

            // for bigger values emit the short or long opcode
            if (value > -129 && value < 128)
            {
                ilGenerator.Emit(OpCodes.Ldc_I4_S, (SByte)value);
            }
            else
            {
                ilGenerator.Emit(OpCodes.Ldc_I4, value);
            }
        }
    }
}