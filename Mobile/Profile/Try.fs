﻿module Nikeza.Mobile.Profile.Try

open Nikeza.DataTransfer
open Registration

type SubmitFn =      ValidatedForm         -> Result<Profile, ValidatedForm>
type LoginFn =       Credentials           -> Result<Provider, Credentials>
type LogoutFn =      Provider              -> Result<Provider, Provider>
type SaveProfileFn = ValidatedProfile      -> Result<Profile, ValidatedProfile>
type SaveSourcesFn = DataSourceSubmit list -> Result<Profile, DataSourceSubmit list>

let submit : SubmitFn = 
    fun registration -> Error registration

let internal logout : LogoutFn = 
    fun p -> Error p

let internal login : LoginFn = 
    fun credentials -> Error credentials

let internal saveProfile : SaveProfileFn = 
    fun profile -> Error profile

let internal saveSources : SaveSourcesFn = 
    fun sources -> Error sources