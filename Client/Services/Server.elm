module Services.Server exposing (..)

import Controls.Login as Login exposing (Model)
import Domain.Core exposing (..)


tryLogin : Login.Model -> Login.Model
tryLogin credentials =
    let
        successful =
            String.toLower credentials.username == "test" && String.toLower credentials.password == "test"
    in
        if successful then
            { username = credentials.username, password = credentials.password, loggedIn = True }
        else
            { username = credentials.username, password = credentials.password, loggedIn = False }


contributors : List Contributor
contributors =
    []


contributor : Id -> Maybe Contributor
contributor id =
    Nothing


links : Id -> Links
links profileId =
    initLinks


addLink : Id -> Link -> Result String Links
addLink profileId link =
    Err "Not implemented"


topicLinks : Topic -> ContentType -> Id -> List Link
topicLinks topic contentType id =
    []


usernameToId : String -> Id
usernameToId username =
    Id "undefined"


connections : Id -> List Connection
connections profileId =
    []


addConnection : Id -> Connection -> Result String (List Connection)
addConnection profileId connection =
    Err "Failed to add connection"


topics : List Topic
topics =
    []


platforms : List Platform
platforms =
    []


suggestedTopics : String -> List Topic
suggestedTopics search =
    []
