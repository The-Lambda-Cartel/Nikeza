module Nikeza.Server.WordPress

    open System
    open System.Collections.Generic
    open Newtonsoft.Json
    open Model
    open Http
    open Utils
    open StackOverflow.Suggestions
    open Nikeza.Common

    [<Literal>]
    let private APIBaseAddress =   "https://public-api.wordpress.com/"

    [<Literal>]
    let private ArticlesUrl =      "rest/v1/sites/{0}/posts?number=100&page={1}"

    [<Literal>]
    let private DefaultThumbnail = "rest/v1/sites/{0}/posts?number=1&page=1"

    type Tag = { ID: string; name: string }

    type Post = { 
        title: string 
        URL:   string 
        date:  DateTime
        Tags:  IDictionary<string, Tag>
    }

    type Response = { found: int; posts: Post list }

    let private toLink profileId (post:Post) =

        let stringToTopic t = { 
            Id=   -1
            Name= t.ToString()                   
                   .Remove(0,1)
                   .Split(",")
                   .[0]
            IsFeatured= false
        }

        let derivedTopics = post.title  
                            |> suggestionsFromText 
                            |> List.map (fun n -> {Id= -1; Name=n; IsFeatured=false})

        let topics = 
            post.Tags 
             |> List.ofSeq
             |> List.map stringToTopic 
             |> List.append derivedTopics 
             |> Set.ofList 
             |> Set.toList

        { Id= -1
          ProfileId= profileId
          Title= post.title |> replaceHtmlCodes
          Description= ""
          Url= post.URL
          Topics= topics
          ContentType="Articles"
          IsFeatured= false
          Timestamp= post.date
        }

    let getThumbnail accessId =
 
        let response = sendRequest APIBaseAddress DefaultThumbnail accessId <| string 1

        if response.IsSuccessStatusCode
           then let json = response.Content.ReadAsStringAsync() |> toResult
                let line = json.Split("\"avatar_URL\"")
                                .[1]
                                .Split(',')
                                .[0]
                                .Replace("\\", "")
                                 
                let startIndex = line.IndexOf("https:")
                let url = line.Substring(startIndex, line.Length - 3)
                url
           else ""

    let rec private getLinks (user:User) (pageNumber:int) url existingLinks =

        let response = sendRequest APIBaseAddress url user.AccessId <| string pageNumber

        if  response.IsSuccessStatusCode
            then let json =     response.Content.ReadAsStringAsync() |> toResult
                 let settings = JsonSerializerSettings()
                 settings.MissingMemberHandling <- MissingMemberHandling.Ignore
 
                 let result =      JsonConvert.DeserializeObject<Response>(json, settings)
                 let lastPage =    (result.found / 100) + 1
                 let canContinue = lastPage >= pageNumber
                 if canContinue
                    then result.posts  |> Seq.toList 
                                       |> List.map (fun post -> toLink user.ProfileId post)
                                       |> List.append <| existingLinks
                                       |> getLinks user (pageNumber + 1) url
                    else existingLinks
            else []

    let wordpressLinks (user:User) =
        [] |> getLinks user 1 ArticlesUrl

    let newWordpressLinks (lastSynched:DateTime) (user:User) : Link list =
        let convertDate (date:DateTime) =
            
            let EnsureTwoDigits (x:int) =
                x |> string
                  |> String.length 
                  |> function
                     | 1 -> sprintf "0%i" x
                     | 2 -> x |> string
                     | _ -> "00"

            let month = date.Month |> EnsureTwoDigits
            let day =   date.Day   |> EnsureTwoDigits

            sprintf "%i%s%s" date.Year month day

        let pageNumber = 1
        let fromDate = convertDate lastSynched
        let dateUrl =  sprintf "%s&after=%s" ArticlesUrl fromDate
        [] |> getLinks user 1 dateUrl