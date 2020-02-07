﻿using System;
using System.Linq;
using System.Reflection;
using Grapp.AppBase.Std.Exceptions.Assert;
using Grapp.AppBase.Std.Exceptions.Contract;

#if !NETSTANDARD 
using System.Web.Routing;
#endif

namespace Grapp.AppBase.Std.Library
{
	public partial class XHelper
	{
		public partial class Reflections
		{
			public static class Methods
			{
				public const BindingFlags InstanceNonPublicBindingFlags = BindingFlags.NonPublic | BindingFlags.NonPublic | BindingFlags.Instance;
				public const BindingFlags InstanceBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
				public const BindingFlags StaticBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

				public static MethodInfo Get(Type type, string methodName, bool ifNotFoundThrowAnException = true)
				{
					XContract.ArgIsNotNull(type, nameof(type));
					XContract.ArgIsNotNull(methodName, nameof(methodName));

					return Get(type, methodName, InstanceBindingFlags, ifNotFoundThrowAnException: ifNotFoundThrowAnException);
				}

				public static MethodInfo Get(object obj, string methodName, bool ifNotFoundThrowAnException = true)
				{
					XContract.ArgIsNotNull(obj, nameof(obj));
					XContract.ArgIsNotNull(methodName, nameof(methodName));

					return Get(obj.GetType(), methodName, InstanceBindingFlags, ifNotFoundThrowAnException: ifNotFoundThrowAnException);
				}

				public static MethodInfo GetStatic(Type type, string methodName, bool ifNotFoundThrowAnException = true)
				{
					XContract.ArgIsNotNull(type, nameof(type));
					XContract.ArgIsNotNull(methodName, nameof(methodName));

					return Get(type, methodName, StaticBindingFlags, ifNotFoundThrowAnException: ifNotFoundThrowAnException);
				}

				public static object Invoke(object obj, string methodName, params object[] arguments)
				{
					XContract.ArgIsNotNull(obj, nameof(obj));
					XContract.ArgIsNotNull(methodName, nameof(methodName));

					return Invoke(obj, Get(obj, methodName), arguments);
				}

				public static object Invoke(object obj, MethodInfo method, params object[] arguments)
				{
					XContract.ArgIsNotNull(obj, nameof(obj));
					XContract.ArgIsNotNull(method, nameof(method));

					return method.Invoke(obj, arguments);
				}

				public static object InvokeGeneric(object obj, string methodName, Type[] genericTypes, params object[] arguments)
				{
				    return InvokeGeneric(
				        obj,
				        Get(obj.GetType(), methodName, InstanceBindingFlags, methodInfo => InternalCheckIfEquals(methodInfo, genericTypes, arguments)),
				        genericTypes, arguments);
				}

			    public static object InvokeGeneric(object obj, string methodName, Type[] genericTypes, BindingFlags flags, params object[] arguments)
			    {
			        return InvokeGeneric(
			            obj,
			            Get(obj.GetType(), methodName, flags, methodInfo => InternalCheckIfEquals(methodInfo, genericTypes, arguments)),
			            genericTypes, arguments);
			    }

				public static object InvokeGeneric(object obj, MethodInfo method, Type[] genericTypes, params object[] arguments)
				{
					XContract.ArgIsNotNull(obj, nameof(obj));
					XContract.ArgIsNotNull(method, nameof(method));
					XContract.ArgIsNotNull(genericTypes, nameof(genericTypes));

					if(genericTypes.Length == 0)
					{
						throw XArgumentException.Create(nameof(genericTypes), "A quantidade de parâmetros genéricos não pode ser 0 (zero).");
					}

					var generic = method.MakeGenericMethod(genericTypes);
					return generic.Invoke(obj, arguments);
				}

				public static object InvokeStatic(Type objectType, string methodName, Type[] genericTypes, params object[] arguments)
				{
					XContract.ArgIsNotNull(objectType, nameof(objectType));
					XContract.ArgIsNotNull(methodName, nameof(methodName));

				    var generics = objectType
				        .GetMethods(BindingFlags.Public | BindingFlags.Static)
				        .Where(info => info.Name == methodName
				                       && info.IsGenericMethod
				                       && info.GetGenericArguments().Length == genericTypes.Length)
				        .Select(
				            info =>
				                {
				                    try
				                    {
				                        return info.MakeGenericMethod(genericTypes);
				                    }
				                    catch(Exception)
				                    {
				                        return null;
				                    }
				                })
				        .Where(info => info != null)
				        .ToArray();

				    return generics.First().Invoke(null, arguments);
				}

				public static bool MathParameters(MethodInfo method, object[] args)
				{
					XContract.ArgIsNotNull(method, nameof(method));
					XContract.ArgIsNotNull(args, nameof(args));

#if !NETSTANDARD
                    if(args.Length == 1 && args[0] is RouteValueDictionary)
					{
						var routeValueDictionary = (RouteValueDictionary)args[0];

						return MathParameters(method, args: routeValueDictionary.Values.ToArray());
					}
#endif

					var typesOfArgs = args.Select(o => o.GetType()).ToArray();

					return MathParameters(method, typesOfArgs);
				}

				public static bool MathParameters(MethodInfo method, Type[] typesOfArgs)
				{
					XContract.ArgIsNotNull(method, nameof(method));
					XContract.ArgIsNotNull(typesOfArgs, nameof(typesOfArgs));

					var parameters = method.GetParameters();

					if(parameters.Length != typesOfArgs.Length)
					{
						return false;
					}

					if(typesOfArgs.Length == 0)
					{
						return true;
					}

					for(var i = 0; i < parameters.Length; i++)
					{
						var argType = typesOfArgs[i];
						var parameterInfo = parameters[i];

						if(IsParameterTypeOf(parameterInfo, argType))
						{
							continue;
						}

						return false;
					}

					return true;
				}

#if !NETSTANDARD
				public static bool MathParameters(ParameterInfo[] parameters, RouteValueDictionary args)
				{
					if(parameters.Length != args.Count)
					{
						return false;
					}

					var index = 0;
					foreach(var arg in args)
					{
						var argType = arg.Value.GetType();
						var parameterInfo = parameters[index++];

						if(parameterInfo.Name == arg.Key && IsParameterTypeOf(parameterInfo, argType))
						{
							continue;
						}

						return false;
					}

					return true;
				}
#endif

				private static MethodInfo Get(
					Type type, string methodName, BindingFlags bindingFlags,
					Func<MethodInfo, bool> checkIfEquals = null, bool ifNotFoundThrowAnException = true)
				{
					XContract.ArgIsNotNull(type, nameof(type));
					XContract.ArgIsNotNull(methodName, nameof(methodName));

					try
					{
						var methodInfo = type.GetMethod(methodName, bindingFlags);

						if(methodInfo == null && ifNotFoundThrowAnException)
						{
							throw XArgumentException.Create(nameof(methodName), $"Method '{methodName}' not found on type {type.Name}.");
						}

						return methodInfo;
					}
					catch(AmbiguousMatchException)
					{
						if(checkIfEquals == null)
						{
							throw;
						}

						return type
							.GetMethods(bindingFlags)
							.FirstOrDefault(info => info.Name == methodName && checkIfEquals(info));
					}
				}

				private static bool InternalCheckIfEquals(MethodInfo methodInfo, Type[] genericTypes, object[] arguments)
				{
					return MathParameters(methodInfo, arguments) &&
					       (!methodInfo.IsGenericMethod || MathGenericParameters(methodInfo, genericTypes));
				}

				private static bool IsParameterTypeOf(ParameterInfo info, Type argType)
				{
					return Types.Is(info.ParameterType, argType)
					       || Types.IsNullable(info.ParameterType) && argType.IsAssignableFrom(info.ParameterType.GetGenericArguments()[0]);
				}

				private static bool MathGenericParameters(MethodInfo methodInfo, Type[] genericTypes)
				{
					XContract.ArgIsNotNull(methodInfo, nameof(methodInfo));
					XContract.ArgIsNotNull(genericTypes, nameof(genericTypes));

					if(!methodInfo.IsGenericMethod)
					{
						return false;
					}

					var genericArguments = methodInfo.GetGenericArguments();
					if(genericArguments.Length != genericTypes.Length)
					{
						return false;
					}

					try
					{
						methodInfo.MakeGenericMethod(genericTypes);
						return true;
					}
					catch(Exception)
					{
						return false;
					}
				}
			}
		}
	}
}