module Services.Adapter exposing (..)

import Domain.Core exposing (..)
import Http exposing (..)
import Task exposing (succeed, perform)


httpSuccess : (Result Http.Error a -> msg) -> a -> Cmd msg
httpSuccess msg a =
    a
        |> Result.Ok
        |> msg
        |> Task.succeed
        |> Task.perform identity


type alias Loginfunction msg =
    Credentials -> (Result Http.Error JsonProvider -> msg) -> Cmd msg


type alias Registerfunction msg =
    Form -> (Result Http.Error JsonProfile -> msg) -> Cmd msg


type alias Providerfunction msg =
    Id -> (Result Http.Error JsonProvider -> msg) -> Cmd msg


type alias ProviderTopicfunction msg =
    Id -> Topic -> (Result Http.Error JsonProvider -> msg) -> Cmd msg


type alias Providersfunction msg =
    (Result Http.Error (List JsonProvider) -> msg) -> Cmd msg


type alias UpdateProfilefunction msg =
    Profile -> (Result Http.Error JsonProfile -> msg) -> Cmd msg


type alias AddSourcefunction msg =
    Source -> (Result Http.Error JsonSource -> msg) -> Cmd msg


type alias RemoveSourcefunction msg =
    Id -> (Result Http.Error JsonSource -> msg) -> Cmd msg


type alias Platformsfunction msg =
    (Result Http.Error (List String) -> msg) -> Cmd msg


type alias Bootstrapfunction msg =
    (Result Http.Error JsonBootstrap -> msg) -> Cmd msg


type alias Followersfunction msg =
    Id -> (Result Http.Error Members -> msg) -> Cmd msg


type alias Subscriptionsfunction msg =
    Id -> (Result Http.Error Members -> msg) -> Cmd msg


type alias Sourcesfunction msg =
    Id -> (Result Http.Error (List Source) -> msg) -> Cmd msg


type alias AddLinkfunction msg =
    Id -> Link -> (Result Http.Error JsonPortfolio -> msg) -> Cmd msg


type alias RemoveLinkfunction msg =
    Id -> Link -> (Result Http.Error JsonPortfolio -> msg) -> Cmd msg


type alias Linksfunction msg =
    Id -> (Result Http.Error JsonPortfolio -> msg) -> Cmd msg


type alias TopicLinksfunction msg =
    Id -> Topic -> ContentType -> (Result Http.Error (List JsonLink) -> msg) -> Cmd msg


type alias Followfunction msg =
    Id -> Id -> (Result Http.Error Members -> msg) -> Cmd msg


type alias Unsubscribefunction msg =
    Id -> Id -> (Result Http.Error Members -> msg) -> Cmd msg


type alias JsonProfile =
    { id : String
    , firstName : String
    , lastName : String
    , email : String
    , imageUrl : String
    , bio : String
    , sources : List JsonSource
    }


type alias JsonTopic =
    { name : String
    , isFeatured : Bool
    }


type JsonProviderLinks
    = JsonProviderLinks JsonLinkFields


type alias JsonLinkFields =
    { links : List JsonLink
    }


type alias JsonSource =
    { id : Int
    , profileId : String
    , platform : String
    , username : String
    , linksFound : JsonProviderLinks
    }


type alias JsonPortfolio =
    { articles : List JsonLink
    , videos : List JsonLink
    , podcasts : List JsonLink
    , answers : List JsonLink
    }


type alias JsonBootstrap =
    { providers : List JsonProvider
    , platforms : List String
    }


type alias JsonLink =
    { profile : JsonProfile
    , title : String
    , url : String
    , contentType : String
    , topics : List Topic
    , isFeatured : Bool
    }


type JsonProvider
    = JsonProvider JsonProviderFields


type alias JsonProviderFields =
    { profile : JsonProfile
    , topics : List JsonTopic
    , portfolio : JsonPortfolio
    , recentLinks : List JsonLink
    , subscriptions : List JsonProvider
    , followers : List JsonProvider
    }


toProfile : JsonProfile -> Profile
toProfile jsonProfile =
    { id = Id jsonProfile.id
    , firstName = Name jsonProfile.firstName
    , lastName = Name jsonProfile.lastName
    , email = Email jsonProfile.email
    , imageUrl = Url jsonProfile.imageUrl
    , bio = jsonProfile.bio
    , sources = jsonProfile.sources |> List.map toSource
    }


toJsonProfile : Profile -> JsonProfile
toJsonProfile profile =
    { id = idText profile.id
    , firstName = nameText profile.firstName
    , lastName = nameText profile.lastName
    , email = emailText profile.email
    , imageUrl = urlText profile.imageUrl
    , bio = profile.bio
    , sources = profile.sources |> List.map toJsonSource
    }


toJsonSource : Source -> JsonSource
toJsonSource source =
    let
        foo providerLinks =
            let
                (ProviderLinks fields) =
                    providerLinks
            in
                JsonProviderLinks (JsonLinkFields (fields.links |> List.map toJsonLink))
    in
        { id =
            case source.id |> idText |> String.toInt of
                Ok v ->
                    v

                Err _ ->
                    -1
        , profileId = idText source.profileId
        , platform = source.platform
        , username = source.username
        , linksFound = source.linksFound |> foo
        }


toSource : JsonSource -> Source
toSource jsonSource =
    let
        foo jsonProviderLinks =
            let
                (JsonProviderLinks fields) =
                    jsonProviderLinks
            in
                ProviderLinks (LinkFields (fields.links |> List.map toLink))
    in
        { id = jsonSource.id |> toString |> Id
        , profileId = jsonSource.profileId |> toString |> Id
        , platform = jsonSource.platform
        , username = jsonSource.username
        , linksFound = jsonSource.linksFound |> foo
        }


toJsonLinks : List Link -> List JsonLink
toJsonLinks links =
    links |> List.map toJsonLink


jsonProfileToProvider : JsonProfile -> Provider
jsonProfileToProvider jsonProfile =
    Provider (toProfile jsonProfile) initTopics initPortfolio initPortfolio [] initSubscription initSubscription


toMembers : List JsonProvider -> Members
toMembers jsonProviders =
    jsonProviders
        |> List.map toProvider
        |> Members


toLink : JsonLink -> Link
toLink =
    (\link ->
        { profile = link.profile |> toProfile
        , title = Title link.title
        , url = Url link.url
        , contentType = link.contentType |> toContentType
        , topics = link.topics
        , isFeatured = link.isFeatured
        }
    )


toLinks : List JsonLink -> List Link
toLinks jsonLinks =
    jsonLinks |> List.map toLink


toJsonLink : Link -> JsonLink
toJsonLink link =
    { profile = link.profile |> toJsonProfile
    , title = titleText link.title
    , url = urlText link.url
    , contentType = link.contentType |> contentTypeToText
    , topics = link.topics
    , isFeatured = link.isFeatured
    }


toPortfolio : JsonPortfolio -> Portfolio
toPortfolio jsonPortfolio =
    Portfolio
        (jsonPortfolio.articles |> toLinks)
        (jsonPortfolio.videos |> toLinks)
        (jsonPortfolio.podcasts |> toLinks)
        (jsonPortfolio.answers |> toLinks)


toTopics : List JsonTopic -> List Topic
toTopics jsonTopics =
    jsonTopics |> List.map (\t -> { name = t.name, isFeatured = t.isFeatured })


toProvider : JsonProvider -> Provider
toProvider jsonProvider =
    let
        (JsonProvider field) =
            jsonProvider
    in
        { profile = field.profile |> toProfile
        , topics = field.topics |> toTopics
        , portfolio = field.portfolio |> toPortfolio
        , filteredPortfolio = field.portfolio |> toPortfolio
        , recentLinks = field.recentLinks |> toLinks
        , followers = field.followers |> toMembers
        , subscriptions = field.subscriptions |> toMembers
        }
