namespace Nikeza.Server

module StackOverflow =
    let getThumbnail accessId apiKey = ""

    open System
    open System.IO
    open Newtonsoft.Json
    open Nikeza.Server.Model
    open Nikeza.Server.Http
    open Nikeza.Server.Literals

    [<Literal>]
    let TagsUrl =    "2.2/tags?page={0}&order=desc&sort=popular&site=stackoverflow&filter=!-.G.68grSaJm"

    [<Literal>]
    let AnswersUrl = "2.2/users/{0}/answers?order=desc&sort=activity&site=stackoverflow&filter=!Fcazzsr2b3M)LbUjGAu-Fs0Wf8&key={1}"

    [<Literal>]
    let APIBaseAddress = "https://api.stackexchange.com/"

    [<Literal>]
    let SiteBaseAddress = "https://stackoverflow.com/"

    type Answer = {
        link:          string
        title:         string
        name:          string
        tags:          string list
        isAccepted:    bool
        creation_date: int
        answer_id:     int
        question_id:   int
    }

    type AnswersResponse = { 
        items:           Answer list
        has_more:        bool
        quota_max:       int
        quota_remaining: int
    }

    let toLink profileId item =
        { Id= -1
          ProfileId= profileId
          Title= item.title
          Description= ""
          Url= item.link
          Topics= item.tags |> List.map (fun t -> { Id= -1; Name= t })
          ContentType="Answers"
          IsFeatured= false
        }

    let stackoverflowLinks (platformUser:PlatformUser) =

        use client =   httpClient APIBaseAddress
        let user =     platformUser.User
        let url =      String.Format(AnswersUrl, user.AccessId, platformUser.APIKey)
        let response = client.GetAsync(url) |> Async.AwaitTask 
                                            |> Async.RunSynchronously
        if response.IsSuccessStatusCode
        then let json =    response.Content.ReadAsStringAsync() |> Async.AwaitTask |> Async.RunSynchronously
             let result =  JsonConvert.DeserializeObject<AnswersResponse>(json)
             let links =   result.items |> Seq.map (fun item -> toLink user.ProfileId item)
             links
        else seq []

    type Tag =          { name : string }
    type TagsResponse = { items: Tag list }

    let getTags (pageNumber:int) : string list =
        
        let client = httpClient APIBaseAddress

        try let url =        String.Format(TagsUrl, pageNumber |> string)
            let urlWithKey = sprintf "%s&key=%s" url (File.ReadAllText(KeyFile_StackOverflow))
            let response =   client.GetAsync(urlWithKey) |> Async.AwaitTask 
                                                         |> Async.RunSynchronously
            if response.IsSuccessStatusCode
            then let json =   response.Content.ReadAsStringAsync() |> Async.AwaitTask |> Async.RunSynchronously
                 let result = JsonConvert.DeserializeObject<TagsResponse>(json)
                 let tags =   result.items |> List.ofSeq 
                                           |> List.map (fun i -> i.name)
                 tags
            else []

        finally  client.Dispose()


    module CachedTags =
        
        let private x = [1..25] |> List.collect (fun page -> getTags(page))
        let Instance() = x


    module Suggestions =

        let getRelatedTags (tag:string) =

            if tag <> ""
            then let parseTag (text:string) =
                     let index = text.IndexOf("|")
                     if  index > 0
                         then Some <| text.Substring(0,index)
                         else None
                 
                 let client = httpClient SiteBaseAddress
         
                 try let relatedTagsUrl = sprintf "filter/tags?q=%s" tag
                     let response =       client.GetAsync(relatedTagsUrl) |> Async.AwaitTask 
                                                                          |> Async.RunSynchronously
                     if response.IsSuccessStatusCode
                     then let result = response.Content.ReadAsStringAsync() |> Async.AwaitTask 
                                                                            |> Async.RunSynchronously 
                          result.Split('\n') |> List.ofArray 
                                             |> List.filter (fun x -> x <> "")
                                             |> List.tryHead
                                             |> function 
                                                | None -> []
                                                | Some formatted ->
                                                    let tags = formatted.Split("\\n") 
                                                               |> List.ofArray
                                                               |> List.choose parseTag
                                                               |> List.map   (fun tag -> tag.Replace(@"""", ""))
                                                               |> List.filter(fun current -> current <> tag)
                                                    tag::tags
                     else []
     
                 finally client.Dispose()
            else []
                
        let getSuggestions (searchItem:string) =
            if searchItem <> ""
            then let tags =         CachedTags.Instance() |> List.map (fun t -> t.ToLower())
                 let filteredTags = tags |> List.filter(fun t -> t.Contains(searchItem.ToLower()))
                 let matchingTags = filteredTags |> List.filter (fun t -> t = searchItem)

                 if matchingTags |> List.isEmpty |> not
                     then getRelatedTags matchingTags.Head
                     else filteredTags
            else []