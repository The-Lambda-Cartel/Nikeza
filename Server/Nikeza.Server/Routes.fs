module Nikeza.Server.Routes

open System
open Microsoft.AspNetCore.Http
open Giraffe.HttpContextExtensions
open Giraffe.HttpHandlers
open Store
open Model
open Platforms
open Authentication
open Giraffe.Tasks

[<Literal>]
let AuthScheme = "Cookie"

open Nikeza.Server.DatabaseCommand.Commands

//-----------------------------------------------------------------------
// DEPLOYMENT
//-----------------------------------------------------------------------
// dotnet publish -c Release -o "C:\Nikeza\deployment" --runtime win8-x64
//-----------------------------------------------------------------------


let private registrationHandler: HttpHandler = 
    fun next ctx -> 
        task {
            let! data = ctx.BindJson<RegistrationRequest>()
            match Registration.register { data with Email= data.Email.ToLower() } with
            | Success profile -> return! json profile next ctx
            | Failure         -> return! (setStatusCode 400 >=> json "registration failed") next ctx
        }

let private loginHandler: HttpHandler = 
    fun next ctx -> 
        task {
            let! data = ctx.BindJson<LogInRequest>()
            let  email = data.Email.ToLower()
            if   authenticate email data.Password
                 then match login email with
                      | Some provider -> return! json provider next ctx
                      | None          -> return! (setStatusCode 400 >=> json "Invalid login") next ctx
                 else return! (setStatusCode 400 >=> json "Invalid login") next ctx                                                       
        }

let private fetchProvider providerId: HttpHandler =
    fun next ctx ->
        getProvider providerId
         |> function
           | Some p -> ctx.WriteJson p
           | None   -> (setStatusCode 400 >=> json "provider not found") next ctx

let private followHandler: HttpHandler = 
    fun next ctx -> 
        task { let! data = ctx.BindJson<FollowRequest>()

               let alreadyFollowing = 
                   data.ProfileId 
                    |> getFollowers 
                    |> List.exists(fun f -> f.Profile.Id = data.SubscriberId)

               let getResult() =
                   match (getProvider data.SubscriberId, getProvider data.ProfileId) with
                   | (Some user, Some provider) -> json { User= user; Provider= provider }
                   | (Some _, None)             -> (setStatusCode 400 >=> json "provider not found")
                   | (None, Some _)             -> (setStatusCode 400 >=> json "user not found")    
                   | (None, None)               -> (setStatusCode 400 >=> json "user and provider not found")

               if not alreadyFollowing
                   then Follow data |> Command.execute |> ignore
                        return! getResult() next ctx
                   else return! getResult() next ctx
             } 

let private unsubscribeHandler: HttpHandler = 
    fun next ctx -> 
        task { let! data = ctx.BindJson<UnsubscribeRequest>()

               Unsubscribe data |> Command.execute |> ignore
               
               match (getProvider data.SubscriberId, getProvider data.ProfileId) with
               | (Some user, Some provider) -> return! json { User= user; Provider= provider } next ctx
               | (Some _, None)             -> return! (setStatusCode 400 >=> json "provider not found") next ctx
               | (None, Some _)             -> return! (setStatusCode 400 >=> json "user not found")     next ctx
               | (None, None)               -> return! (setStatusCode 400 >=> json "user and provider not found") next ctx
        } 

let private featureLinkHandler: HttpHandler = 
    fun next ctx -> 
        task { 
            let! data = ctx.BindJson<FeatureLinkRequest>()
            FeatureLink data |> Command.execute |> ignore
            return! json data.LinkId next ctx
        }

let private featuredTopicsHandler: HttpHandler = 
    fun next ctx -> 
        task { 
            let! data = ctx.BindJson<FeaturedTopicsRequest>()
            UpdateTopics data |> Command.execute |> ignore
            return! fetchProvider data.ProfileId next ctx
        }

let private updateProfileHandler: HttpHandler = 
    fun next ctx -> 
        task { 
            let! data = ctx.BindJson<ProfileRequest>()
            UpdateProfile data |> Command.execute |> ignore
            return! json data next ctx
        }

let private updateProviderHandler: HttpHandler = 
    fun next ctx -> 
        task { 
            let! data = ctx.BindJson<ProfileAndTopicsRequest>()
            let topicsRequest = { ProfileId= data.Profile.Id
                                  Names=     data.Topics |> List.map (fun t -> t.Name) }
                                  
            UpdateProfile data.Profile  |> Command.execute |> ignore
            UpdateTopics  topicsRequest |> Command.execute |> ignore

            match getProvider data.Profile.Id with
            | Some provider -> return! json provider next ctx
            | None ->          return! (setStatusCode 400 >=> json "provider not found") next ctx   
        }

let private addSourceHandler: HttpHandler = 
    fun next ctx -> 
        task { 
            let! data =    ctx.BindJson<DataSourceRequest>()
            let sourceId = AddSource data |> Command.execute
            let links =    data.ProfileId |> Store.linksFrom data.Platform |> List.toSeq
            let source = { data with Id = Int32.Parse(sourceId); Links = links }
            return! json source next ctx
        }

let private removeSourceHandler (sourceId:string): HttpHandler = 
    fun next ctx -> 
        task {
            let id = Int32.Parse(sourceId)
            let (data:RemoveDataSourceRequest) = { Id= id }
            RemoveSource data |> Command.execute |> ignore
            return! json sourceId next ctx
        }

let private addLinkHandler: HttpHandler = 
    fun next ctx -> 
        task { 
            let! data = ctx.BindJson<Link>()
            let linkId = AddLink { data with Description = ""; Timestamp= DateTime.Now } |> Command.execute
            return! json { data with Id = Int32.Parse(linkId) } next ctx
        }

let private removeLinkHandler: HttpHandler = 
    fun _ ctx -> 
        task { 
            let! data = ctx.BindJson<RemoveLinkRequest>()
            RemoveLink data |> Command.execute |> ignore
            return Some ctx
        }

let private updateThumbnailHandler: HttpHandler = 
    fun _ ctx -> 
        task { 
            let! data = ctx.BindJson<UpdateThumbnailRequest>()
            UpdateThumbnail data |> Command.execute |> ignore
            return Some ctx
        }

let private fetchBootstrap x: HttpHandler =

    StackOverflow.CachedTags.Instance() |> ignore

    let providers = getProviders()
    json { Providers= providers; Platforms=getPlatforms() }

let private syncSources x: HttpHandler =

    getAllSources() |> List.iter (fun s -> s |> syncDataSource |> ignore)
    json []

let private fetchProviders x: HttpHandler =
    json <| getProviders()

let private fetchLinks providerId: HttpHandler =

    let links =    providerId |> getLinks
    providerId 
     |> getProvider
     |> function
        | Some p -> json <| { p with Portfolio= links |> toPortfolio }
        | None   -> json []

let private fetchSuggestedTopics (text) =
    json <| Suggestions.getSuggestions text

let private fetchRecent (subscriberId) =
    json <| getProvidersWithRecent subscriberId
    
let private fetchFollowers (providerId) =
    json <| getFollowers providerId
    
let private fetchSubscriptions (providerId) =
    json <| getSubscriptions providerId

let private fetchSources (providerId) =
    json <| getSources providerId

let private fetchThumbnail (platform:string , accessId:string) =

    let thumbnail() =
        platform.ToLower() 
         |> platformFromString 
         |> Platforms.getThumbnail accessId
                                 
    json { ImageUrl= thumbnail(); Platform= platform }
         
let webApp: HttpHandler = 
    choose [
        GET >=>
            choose [
                route "/"                   >=> htmlFile "index.html"
                route  "/options"           >=> setHttpHeader "Allow" "GET, OPTIONS, POST" // CORS support
                routef "/syncsources/%s"        syncSources
                routef "/bootstrap/%s"          fetchBootstrap
                routef "/providers/%s"          fetchProviders
                routef "/links/%s"              fetchLinks
                routef "/suggestedtopics/%s"    fetchSuggestedTopics
                routef "/recent/%s"             fetchRecent
                routef "/followers/%s"          fetchFollowers
                routef "/subscriptions/%s"      fetchSubscriptions
                routef "/sources/%s"            fetchSources
                routef "/thumbnail/%s/%s"       fetchThumbnail
                routef "/provider/%s"           fetchProvider
                routef "/removesource/%s"       removeSourceHandler
            ]
        POST >=> 
            choose [
                route "/register"        >=> registrationHandler
                route "/login"           >=> loginHandler
                route "/logout"          >=> signOff AuthScheme >=> text "logged out"
                route "/follow"          >=> followHandler
                route "/unsubscribe"     >=> unsubscribeHandler
                route "/featurelink"     >=> featureLinkHandler
                route "/updateprofile"   >=> updateProfileHandler
                route "/updateprovider"  >=> updateProviderHandler
                route "/addsource"       >=> addSourceHandler
                route "/addlink"         >=> addLinkHandler
                route "/removelink"      >=> removeLinkHandler
                route "/updatethumbnail" >=> updateThumbnailHandler
                route "/featuredtopics"  >=> featuredTopicsHandler
            ]
            
        setStatusCode 404 >=> text "Not Found" ]