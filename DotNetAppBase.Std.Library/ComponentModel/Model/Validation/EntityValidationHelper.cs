﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using DotNetAppBase.Std.Library.ComponentModel.Model.Validation.Enums;
using DotNetAppBase.Std.Library.ComponentModel.Types;

namespace DotNetAppBase.Std.Library.ComponentModel.Model.Validation
{
	public class EntityValidationHelper<TEntity>
	{
		private readonly object _syncValidations = new object();

		public EntityValidationHelper()
		{
			TypeDescriptor = XTypeDescriptor.Get<TEntity>();
			
            Validations = new List<ValidationResult>();
		}

		public bool HasViolations
		{
			get
			{
				lock(_syncValidations)
				{
					return Validations.Any();
				}
			}
		}

		public TEntity Current { get; private set; }

		public List<ValidationResult> Validations { get; }

        public XTypeDescriptor TypeDescriptor { get; }

        public EntityValidationHelper<TEntity> AddValidationResult(ValidationResult validationResult)
		{
			lock(_syncValidations)
			{
				Validations.Add(validationResult);
			}

			return this;
		}

		public EntityValidationHelper<TEntity> AddValidationsResult(IEnumerable<ValidationResult> validationResult)
		{
			lock(_syncValidations)
			{
				Validations.AddRange(validationResult);
			}

			return this;
		}

		public EntityValidationHelper<TEntity> AddValidationsResult(EntityValidationResult entityValidationResult) => AddValidationsResult(entityValidationResult.Validations);

        public void Begin(TEntity entity)
		{
			lock(_syncValidations)
			{
				Validations.Clear();
				Current = entity;
			}
		}

		public EntityValidationResult End()
		{
			lock(_syncValidations)
			{
				var result = new EntityValidationResult(Validations.ToArray());

				Validations.Clear();

				return result;
			}
		}

		public EntityValidationHelper<TEntity> IfNoViolationsValidate(
			Expression<Func<TEntity, object>> memberExpression,
			Func<TEntity, bool> conditional,
			string errorMessage, 
			bool format = true,
			EValidationKind validationKind = EValidationKind.Error)
		{
			lock(_syncValidations)
            {
                return HasViolations ? this : Validate(memberExpression, conditional, errorMessage, format, validationKind);
            }
		}

		public EntityValidationHelper<TEntity> IfNoViolationsValidate<TConditionalValue>(
			Expression<Func<TEntity, TConditionalValue>> memberExpression,
			Func<TEntity, TConditionalValue> calculateConditionalValue,
			Func<TEntity, TConditionalValue, bool> conditional,
			string errorMessage, 
			bool format = true,
			EValidationKind validationKind = EValidationKind.Error)
		{
			lock(_syncValidations)
            {
                return HasViolations ? this : Validate(memberExpression, calculateConditionalValue, conditional, errorMessage, format, validationKind);
            }
		}

		public EntityValidationHelper<TEntity> IfNoViolationsValidateComplex(
			Expression<Func<TEntity, object>> memberExpression,
			Func<TEntity, bool> conditional = null)
		{
			lock(_syncValidations)
			{
				if(HasViolations)
				{
					return this;
				}

				return ValidateComplex(memberExpression, conditional);
			}
		}

		public EntityValidationHelper<TEntity> IfNoViolationsValidateComplex(
			Expression<Func<TEntity, object>> memberExpression,
			Func<TEntity, bool> conditional,
			Func<TEntity, EntityValidationResult> validation)
		{
			lock(_syncValidations)
			{
				if(HasViolations)
				{
					return this;
				}

				return ValidateComplex(memberExpression, conditional, validation);
			}
		}

		public EntityValidationHelper<TEntity> Validate(
			Expression<Func<TEntity, object>> memberExpression,
			Func<TEntity, bool> conditional,
			string errorMessage, 
			bool format = true,
			EValidationKind validationKind = EValidationKind.Error)
		{
			lock(_syncValidations)
			{
				if(!conditional(Current))
				{
					return this;
				}

				var member = XHelper.Expressions.GetMemberName(memberExpression);
				var displayPropertyValue = TypeDescriptor.GetModelDisplayName(Current);

				Validations.Add(
                    CreateValidation(
					                validationKind,
					                format
						                ? string.Format(errorMessage, TypeDescriptor.GetPropertyDisplayName(member, false), displayPropertyValue)
						                : errorMessage));

				return this;
			}
		}

		public EntityValidationHelper<TEntity> Validate<TConditionalValue>(
			Expression<Func<TEntity, TConditionalValue>> memberExpression,
			Func<TEntity, TConditionalValue> calculateConditionalValue,
			Func<TEntity, TConditionalValue, bool> conditional,
			string errorMessage, 
			bool format = true,
			EValidationKind validationKind = EValidationKind.Error) =>
            Validate(
                memberExpression,
                null,
                calculateConditionalValue,
                conditional,
                errorMessage,
                format,
                validationKind);

        public EntityValidationHelper<TEntity> Validate<TConditionalValue>(
	        Expression<Func<TEntity, TConditionalValue>> memberExpression,
	        Func<TEntity, bool> conditionalToCheck,
	        Func<TEntity, TConditionalValue> calculateConditionalValue,
	        Func<TEntity, TConditionalValue, bool> conditional,
	        string errorMessage, 
	        bool format = true,
	        EValidationKind validationKind = EValidationKind.Error)
	    {
	        lock(_syncValidations)
	        {
	            if(!(conditionalToCheck?.Invoke(Current) ?? true))
	            {
	                return this;
	            }

	            var conditionalValue = calculateConditionalValue(Current);
	            if(!conditional(Current, conditionalValue))
	            {
	                return this;
	            }

	            var member = XHelper.Expressions.GetMemberName(memberExpression);
	            var displayPropertyValue = TypeDescriptor.GetModelDisplayName(Current);

	            Validations.Add(CreateValidation(
	                                validationKind,
	                                format
	                                    ? string.Format(errorMessage, TypeDescriptor.GetPropertyDisplayName(member, false), conditionalValue, displayPropertyValue)
	                                    : errorMessage));

	            return this;
	        }
	    }

		public EntityValidationHelper<TEntity> ValidateComplex(
			Expression<Func<TEntity, object>> memberExpression,
			Func<TEntity, bool> conditional = null)
		{
			lock(_syncValidations)
			{
				if(conditional != null && !conditional(Current))
				{
					return this;
				}

				var complexValue = XHelper.Expressions.GetMemberValue(memberExpression, Current);

				EntityValidator.Validate(complexValue, Validations);
			}

			return this;
		}

		public EntityValidationHelper<TEntity> ValidateComplex(
			Expression<Func<TEntity, object>> memberExpression,
			Func<TEntity, bool> conditional,
			Func<TEntity, EntityValidationResult> validation)
		{
			lock(_syncValidations)
			{
				if(!conditional(Current))
				{
					return this;
				}

				var result = validation(Current) ?? EntityValidationResult.Empty;

				Validations.AddRange(result.Validations);
			}

			return this;
		}

        public EntityValidationHelper<TEntity> ValidateMany(
            Expression<Func<TEntity, IEnumerable>> memberExpression,
            Func<TEntity, bool> conditional = null)
        {
            lock (_syncValidations)
            {
                if (conditional != null && !conditional(Current))
                {
                    return this;
                }

                var many = XHelper.Expressions.GetMemberValue(memberExpression, Current);
                foreach (var one in many)
                {
                    if (!EntityValidator.Validate(one, Validations))
                    {
                        break;
                    }
                }
            }

            return this;
        }

		private ValidationResult CreateValidation(EValidationKind validationKind, string message)
		{
			switch(validationKind)
			{
				case EValidationKind.Error:
					return new ValidationResult(message);

				case EValidationKind.Warning:
					return new WarningValidationResult(message);

				default:
					throw new ArgumentOutOfRangeException(nameof(validationKind));
			}
		}
	}
}