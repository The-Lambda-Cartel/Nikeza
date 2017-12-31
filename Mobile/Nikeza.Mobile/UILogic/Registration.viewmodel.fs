namespace Nikeza.Mobile.UILogic.Registration

open System.Windows.Input
open Nikeza.Mobile.UILogic
open Nikeza.Mobile.Profile.Commands.Registration
open Nikeza.Mobile.Profile.Registration
open Nikeza.Mobile.Profile.Events

type UIForm = Registration.Types.Form
type DomainForm = Nikeza.Mobile.Profile.Registration.Form

type ViewModel() as x =

    let form() = { 
        UIForm.Email=    x.Email
        UIForm.Password= x.Password
        UIForm.Confirm=  x.Confirm
    }

    let toDomainForm (form:UIForm) : DomainForm = { 
          Email =    Email    form.Email
          Password = Password form.Password
          Confirm =  Password form.Password
    }

    let mutable validatedForm = None

    let formValidated = function
        | FormValidated form -> validatedForm <- Some form; true
        | _ -> false

    let validate() =
        { UnvalidatedForm.Form= form() |> toDomainForm } 
          |> Validate.Execute 
          |> Targeting.ValidateRegistration.workflow
          |> List.exists formValidated

    let eventToPage = function
        | RegistrationSucceeded _ -> () // displayPortal()
        | RegistrationFailed    _ -> () // displayError()

    let withPage events =
        events |> List.iter eventToPage
               
    let submit() =
        validatedForm |> function 
                         | Some form ->
                                form |> Submit.Execute 
                                     |> Targeting.SubmitRegistration.workflow
                                     |> withPage
                                     
                         | None -> ()

    let validateCommand = DelegateCommand( (fun _ -> x.IsValidated <- validate()) , fun _ -> true)

    let submitCommand =   DelegateCommand( (fun _ -> ()), 
                                            fun _ -> x.IsValidated <- validate(); x.IsValidated )
    let mutable email =    ""
    let mutable password = ""
    let mutable confirm =  ""
    let mutable isValidated = false

    member x.Email
        with get() =      email 
        and  set(value) = email <- value

    member x.Password
        with get() =      password
        and  set(value) = password <- value

    member x.Confirm
        with get() =      confirm
        and  set(value) = confirm <- value

    member x.IsValidated
        with get() =      isValidated
        and  set(value) = isValidated <- value

    member x.Validate = validateCommand :> ICommand
    member x.Submit =   submitCommand   :> ICommand