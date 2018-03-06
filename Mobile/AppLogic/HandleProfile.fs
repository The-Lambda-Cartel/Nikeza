﻿namespace Nikeza.Mobile.AppLogic

module ProfileEvents =

    open Nikeza.Mobile.Profile.Events
    open System.Diagnostics

    module Register =

        open Nikeza.Mobile.UILogic.Registration

        let addResponders responders =
 
            let handle =  function
             | RegistrationSucceeded p -> (Debug.WriteLine(sprintf "Request: Navigate to Portal\n %A" p))
             | RegistrationFailed    _ -> ()

            let handlers = handle::responders.ForRegistrationSubmission
            { responders with Responders.ForRegistrationSubmission= handlers }

    module Save =
        
        open Nikeza.Mobile.UILogic.Portal.ProfileEditor

        let addResponders (responders:Responders) =
 
            let handle =  function
             | ProfileSaved      p -> (Debug.WriteLine(sprintf "Request: Navigate to previous page"))
             | ProfileSaveFailed _ -> ()

            let handlers = handle::responders.ForProfileSave
            { responders with Responders.ForProfileSave= handlers }

    module Topics =
        
        open Nikeza.Mobile.UILogic.Portal.ProfileEditor

        let addResponders (responders:Responders) =
 
            let handle = function
                QueryTopicsFailed msg -> (Debug.WriteLine(sprintf "Request: Navigate to Error page\n %s" msg))

            let handlers = handle::responders.ForTopicsFnFailed
            { responders with Responders.ForTopicsFnFailed= handlers }