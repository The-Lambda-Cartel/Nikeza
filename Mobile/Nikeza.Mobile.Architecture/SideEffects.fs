﻿namespace Nikeza.Access.Specification

module Registration =
    open Events
    open Nikeza.Access.Specification.Try

    type Implementation = {
        Submit : Submit
    }

    type SideEffects = {
        ForRegistrationSubmission : (RegistrationSubmissionEvent -> unit) list
    }

    type Dependencies = {
        Implementation : Implementation
        SideEffects    : SideEffects
    }


module Login =

    open Nikeza.Common
    open Try
    open Events

    type SideEffects =  { 
        ForLoginAttempt : (LoginEvent -> unit) nonempty 
    }

    type Implementation =  { 
        Login : Login
    }

    type Dependencies = { 
        Implementation : Implementation
        SideEffects    : SideEffects
    }