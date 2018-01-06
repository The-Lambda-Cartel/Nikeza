module Nikeza.Mobile.Portfolio.Events

open Nikeza.Common
open Nikeza.DataTransfer

type LinksEvent =
    | LinkFeatured         of LinkId
    | LinkFeaturedFailed   of LinkId

    | LinkUnfeatured       of LinkId
    | LinkUnfeaturedFailed of LinkId

type TopicsEvent =
    | TopicsFeatured       of TopicId list
    | TopicsFeaturedFailed of TopicId list

type QueryEvent =
    | GetPortfolioFailed    of ProviderId
    | GetPortfolioSucceeded of Provider