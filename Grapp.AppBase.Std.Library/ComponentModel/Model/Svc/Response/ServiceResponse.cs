﻿using System.Runtime.Serialization;
using Grapp.AppBase.Std.Library.ComponentModel.Model.Validation;

namespace Grapp.AppBase.Std.Library.ComponentModel.Model.Svc
{
    public abstract class ServiceResponse
    {
        [DataMember]
        public bool Fail => Status == EServiceResponse.Failed;

        [DataMember]
        public EServiceResponse Status => ValidationResult.HasViolations ? EServiceResponse.Failed : EServiceResponse.Succeeded;

        [DataMember]
        public bool Success => Status == EServiceResponse.Succeeded;

        [DataMember]
        public EntityValidationResult ValidationResult { get; internal set; }
    }
}