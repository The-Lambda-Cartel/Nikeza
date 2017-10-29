module Controls.NewLinks exposing (..)

import Html exposing (..)
import Html.Attributes exposing (..)
import Html.Events exposing (..)
import Json.Decode exposing (map)
import Http
import Settings exposing (..)
import Domain.Core exposing (..)
import Services.Adapter exposing (..)


type Msg
    = InputTitle String
    | InputUrl String
    | InputTopic String
    | RemoveTopic Topic
    | InputContentType String
    | AddLink NewLinks
    | AddTopic Topic
    | TopicSuggestionResponse (Result Http.Error (List Topic))
    | Response (Result Http.Error JsonLink)


update : Msg -> NewLinks -> ( NewLinks, Cmd Msg )
update msg model =
    let
        linkToCreate =
            model.current

        linkToCreateBase =
            model.current.base
    in
        case msg of
            InputTitle v ->
                ( { model | current = { linkToCreate | base = { linkToCreateBase | title = Title v } } }, Cmd.none )

            InputUrl v ->
                ( { model | current = { linkToCreate | base = { linkToCreateBase | url = Url v } } }, Cmd.none )

            InputTopic v ->
                ( { model | current = { linkToCreate | currentTopic = Topic v False } }, runtime.suggestedTopics v TopicSuggestionResponse )

            RemoveTopic v ->
                let
                    link =
                        { linkToCreateBase | topics = linkToCreateBase.topics |> List.filter (\t -> t /= v) }
                in
                    ( { model | current = { linkToCreate | base = link } }, Cmd.none )

            AddTopic v ->
                ( { model
                    | current =
                        { linkToCreate
                            | currentTopic = Topic "" False
                            , base =
                                { linkToCreateBase | topics = v :: linkToCreateBase.topics }
                        }
                  }
                , Cmd.none
                )

            InputContentType v ->
                ( { model | current = { linkToCreate | base = { linkToCreateBase | contentType = toContentType v } } }, Cmd.none )

            AddLink v ->
                let
                    ( link, current ) =
                        ( v.current.base, v.current )

                    updatedCurrent =
                        { current | base = { link | profileId = model.profileId } }

                    preparedLink =
                        updatedCurrent.base
                in
                    ( { model | current = updatedCurrent }, runtime.addLink preparedLink Response )

            TopicSuggestionResponse (Ok topics) ->
                ( model, Cmd.none )

            TopicSuggestionResponse (Err reason) ->
                Debug.crash (toString reason) ( model, Cmd.none )

            Response (Ok jsonLink) ->
                ( { model
                    | added = (jsonLink |> toLink) :: model.added
                    , current = initLinkToCreate
                  }
                , Cmd.none
                )

            Response (Err error) ->
                Debug.crash ("Error: " ++ toString error) ( model, Cmd.none )



-- VIEW


view : NewLinks -> Html Msg
view model =
    let
        toButton topic =
            div []
                [ button [ onClick <| AddTopic topic ] [ text <| topicText topic ]
                , br [] []
                ]

        -- topicsSelectionUI search =
        --     div []
        --         (search
        --             |> topicText
        --             |> runtime.suggestedTopics
        --             |> List.map toButton
        --         )
        selectedTopicsUI =
            current.base.topics
                |> List.map
                    (\t ->
                        div []
                            [ label [ class "topicAdded" ]
                                [ text <| topicText t
                                , button [ class "removeTopic", onClick <| RemoveTopic t ] [ text "Remove" ]
                                , br [] []
                                , br [] []
                                ]
                            ]
                    )

        ( current, base ) =
            ( model.current, model.current.base )

        listbox =
            select [ Html.Events.on "change" (Json.Decode.map InputContentType Html.Events.targetValue) ]
                [ option [ value "instructions" ] [ text "Content Type" ]
                , option [ value "Article" ] [ text "Article" ]
                , option [ value "Video" ] [ text "Video" ]
                , option [ value "Answer" ] [ text "Answer" ]
                , option [ value "Podcast" ] [ text "Podcast" ]
                ]
    in
        div [ class "mainContent" ]
            [ h3 [] [ text "Link" ]
            , table []
                [ tr []
                    [ td []
                        [ table []
                            [ tr []
                                [ td [] [ input [ class "addLinkText", type_ "text", placeholder "title", onInput InputTitle, value <| titleText base.title ] [] ]
                                ]
                            , tr [] [ td [] [ input [ class "addLinkText", type_ "text", placeholder "link", onInput InputUrl, value <| urlText base.url ] [] ] ]
                            , tr []
                                [ td []
                                    [ table []
                                        [ tr []
                                            [ td [] [ input [ class "addTopic", type_ "text", placeholder "topic", onInput InputTopic, value (topicText current.currentTopic) ] [] ]
                                            , td [] [ listbox ]
                                            ]
                                        ]
                                    ]
                                ]

                            -- , tr [] [ td [] [ topicsSelectionUI current.currentTopic ] ]
                            , tr [] [ td [] [ div [] selectedTopicsUI ] ]
                            ]
                        ]
                    , td [] [ button [ class "addLink", onClick <| AddLink model ] [ text "Add Link" ] ]
                    , td [] [ label [] [ text <| toString model.current.base ] ]
                    ]
                ]
            ]
