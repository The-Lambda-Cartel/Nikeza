﻿namespace Nikeza.Access.Specification

open Nikeza
open Common
open Events
open DataTransfer
    

module Workflows =
    open Nikeza.Access.Specification.Commands
    type ValidateWorkflow = Registration.ValidateCommand -> RegistrationValidationEvent nonempty

module Session =

    type HandleLogin =  Result<Provider option, Credentials>  -> LoginEvent  nonempty
    type HandleLogout = Result<Provider, Provider>            -> LogoutEvent nonempty


module Submission =
    open Commands.Registration.Submit

    type RegistrationSubmission = ResultOf.Submit -> RegistrationSubmissionEvent nonempty


module Validation =
    open Commands.Registration.Validate
    
    type ValidateForm = UnvalidatedForm -> Result<ValidatedForm, UnvalidatedForm>

    type RegistrationValidation = ResultOf.Validate -> RegistrationValidationEvent nonempty