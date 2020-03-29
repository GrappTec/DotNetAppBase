﻿using System;
using DotNetAppBase.Std.Library;

// ReSharper disable CheckNamespace
public static class XFlowExtensions
// ReSharper restore CheckNamespace
{
	public static Context IfIs<T>(this object value, Action<T> isAction) where T : class => new Context(XHelper.Flow.IfIs(value, isAction), value);

    public static TResult IfIsGet<T, TResult>(this object value, Func<T, TResult> getAction, TResult defaultValue) where T : class => XHelper.Flow.IfIsGet(value, getAction, defaultValue);

    public static Context IfIsNot<T>(this object value, Action isAction) where T : class => new Context(XHelper.Flow.IfIsNot<T>(value, isAction), value);

    public static Context OrIfIs<T>(this Context ctx, Action<T> isAction) where T : class
	{
		if(ctx.Stop)
		{
			return ctx;
		}

		ctx.Stop = XHelper.Flow.IfIs(ctx.Value, isAction);

		return ctx;
	}

	public static Context OrIfIsNot<T>(this Context ctx, Action isAction) where T : class
	{
		if(ctx.Stop)
		{
			return ctx;
		}

		ctx.Stop = XHelper.Flow.IfIsNot<T>(ctx.Value, isAction);

		return ctx;
	}

	public static T FlowMustBeNotNull<T>(this T obj) => XHelper.Flow.FlowMustBeNotNull(obj);

    public struct Context
	{
		public Context(bool stop, object value)
		{
			Stop = stop;
			Value = value;
		}

		public bool Stop { get; internal set; }

		public object Value { get; internal set; }
	}
}