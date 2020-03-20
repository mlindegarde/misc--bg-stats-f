﻿namespace Misc.BgStats

open System
open System.Net
open System.Net.Http
open Microsoft.Extensions.Configuration

open Serilog

open FSharp.Control.Tasks.V2

open Misc.BgStats.Domain.Models
open Misc.BgStats.Application

module Program =
    let config =
        ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.local.json", true)
            .Build()
            .Get<Settings>();

    let logger =
        LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.Console()
            .CreateLogger()

    let initClient () =
        let cookieJar = CookieContainer()
        let handler = new HttpClientHandler()

        handler.CookieContainer <- cookieJar

        let client = new HttpClient(handler)

        client.BaseAddress <- config.BoardGameGeek.Url
        client

    let calcAverageScore (plays : Domain.Models.Play list) =
        0

    let runAsync = 
        task {
            use client = initClient()

            // 286096 = Tapestry
            // 199792 = Everdell
            // 266192 = Wingspan
            let! result = client |> BoardGameGeekClient.getAverageScoreForItemAsync 266192 logger
            Renderer.displayAverageScore result

            (*
            let! collection = client |> BoardGameGeekClient.getCollectionAsync config.BoardGameGeek logger
            let evaluations = collection |> Evaluator.evaluate

            evaluations |> Ranker.Top25AllTime |> Renderer.displayTop25Games
            evaluations |> Ranker.GamesToPlay |> Renderer.display15GamesToPlay
            evaluations |> Ranker.GamesToSellOrTrade |> Renderer.display15GamesToSellOrTrade
            *)

            printf "%sPress ENTER to exit: " Environment.NewLine
            Console.ReadLine() |> ignore
        }

    [<EntryPoint>]
    let main argv =
        runAsync 
        |> Async.AwaitTask 
        |> Async.RunSynchronously
        0