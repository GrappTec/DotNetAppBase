﻿using System;
using System.Linq;
using Grapp.AppBase.Std.Library.ComponentModel.Model.Validation.Behaviors;
using Grapp.AppBase.Std.Library.ComponentModel.Model.Validation.Enums;

namespace Grapp.AppBase.Std.Library.ComponentModel.Model.Validation.Annotations.TypedDate
{
	public class XMonthYearAttribute : XValidationAttribute, IDateTimeConstraint
	{
	    public XMonthYearAttribute(bool useAdvancingCaret = true) 
	        : base(EDataType.Date, useAdvancingCaret ? EValidationMode.MaskDateTimeAdvancingCaret : EValidationMode.MaskDataTime) { }

	    public override string Mask => "MM/yyyy";

		public EDateTimeFormat Format => EDateTimeFormat.MonthYear;
        protected override bool InternalIsValid(object value) => true;
    }
}