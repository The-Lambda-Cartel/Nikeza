namespace Nikeza.Mobile.UILogic.Registration

open System.Windows.Input
open Nikeza.Common
open Nikeza.Mobile.UILogic
open Nikeza.Access.Specification
open Nikeza.Access.Specification.Events
open Nikeza.Access.Specification.Registration
open Nikeza.Mobile.Access.Submission
open Nikeza.Mobile.Access.Registration

type ViewModel(dependencies) as x =

    inherit ViewModelBase()

    let update formValidated events = 
        events.Head::events.Tail |> List.exists formValidated

    let trySubmit =   dependencies.Attempt.Submit
    let sideEffects = dependencies.SideEffects

    let mutable validatedForm = None

    let validate() =

        let isValidated = function
            | FormValidated form -> validatedForm <- Some form; true
            | _ -> false
        
        Unvalidated { Email=    Email    x.Email
                      Password= Password x.Password
                      Confirm=  Password x.Confirm
                    } 
                      |> validate
                      |> update isValidated
               
    let submit() =

        let broadcast events =
            events.Head::events.Tail |> List.iter (fun event -> sideEffects.ForRegistrationSubmission |> handle' event)

        validatedForm |> function 
                         | Some form -> 
                                form |> trySubmit
                                     |> toEvents
                                     |> broadcast
                                     
                         | None -> ()

    let validateCommand = DelegateCommand( (fun _ -> x.IsValidated <- validate()) , fun _ -> true)

    let submitCommand =   DelegateCommand( (fun _ -> submit() ),
                                            fun _ -> x.IsValidated <- validate(); true )

    let emailPlaceholder =    "enter email address"
    let passwordPlaceholder = "password"
    let confirmPlaceholder =  "confirm"

    let mutable email =    emailPlaceholder
    let mutable password = passwordPlaceholder
    let mutable confirm =  confirmPlaceholder
    let mutable isValidated = false

    member x.Validate = validateCommand :> ICommand
    member x.Submit =   submitCommand   :> ICommand
    
    member x.Email
             with get() =      email 
             and  set(value) = email <- value
                               base.NotifyPropertyChanged (<@ x.Email @>)

    member x.Password
             with get() =      password
             and  set(value) = password <- value
                               base.NotifyPropertyChanged (<@ x.Password @>)

    member x.Confirm
        with get() =           confirm
        and  set(value) =      confirm <- value
                               base.NotifyPropertyChanged (<@ x.Confirm @>)

    member x.IsValidated
             with get() =      isValidated
             and  set(value) = isValidated <- value
                               base.NotifyPropertyChanged (<@ x.IsValidated @>)

    member x.EmailPlaceholder =    emailPlaceholder
    member x.PasswordPlaceholder = emailPlaceholder
    member x.ConfirmPlaceholder =  emailPlaceholder