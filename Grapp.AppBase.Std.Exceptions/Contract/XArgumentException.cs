﻿using System;
using System.ComponentModel;
using System.Linq;

namespace Grapp.AppBase.Std.Exceptions.Contract
{
    [Serializable]
	public class XArgumentException : ArgumentException
	{
		private const string DefaultMessageObjectTypeInvalid = "Argumento {0} é inválido, necessáriamente ele deveria ser do tipo : {1}";
		private const string DefaultMessageNumericValueInvalid = "Argumento numérico {0} é inválido, restrição: {1}";
		private const string DefaultMessageEnumerableIsEmpty = "Argumento {0} não possui elementos";
		private const string DefaultMessageEnumInvalidValid = "Argumento {0} não possui um valor válido para o enumerador {1}";

		protected XArgumentException() { }

		protected XArgumentException(string message) : base(message) { }

		protected XArgumentException(string paramName, string message) : base(message, paramName) { }

		public static XArgumentException Create(string paramName, [Localizable(isLocalizable: false)] string message) => new XArgumentException(paramName, message);

	    public static XArgumentException CreateInvalidEnumerableIsEmpty(string paramName) 
	        => new XArgumentException(paramName, message: String.Format(DefaultMessageEnumerableIsEmpty, paramName));

	    public static XArgumentException CreateInvalidEnumValue(string paramName, Enum value) 
	        => new XArgumentException(paramName, message: String.Format(DefaultMessageEnumInvalidValid, paramName, value.GetType().Name));

	    public static XArgumentException CreateInvalidNumericValue(string paramName, string numericRestriction) 
	        => new XArgumentException(paramName, message: String.Format(DefaultMessageNumericValueInvalid, paramName, numericRestriction));

	    public static XArgumentException CreateInvalidObjectType(string paramName, Type expectedType) 
	        => new XArgumentException(paramName, message: String.Format(DefaultMessageObjectTypeInvalid, paramName, expectedType.Name));
	}
}