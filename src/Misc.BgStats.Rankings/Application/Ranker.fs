namespace Misc.BgStats.Rankings.Application

open Misc.BgStats.Rankings.Domain.Models

module Ranker =
    let Top25AllTime (evaluations : Evaluation list) =
        evaluations
        |> List.sortByDescending (fun e -> e.Scores |> List.sumBy (fun s -> s.Value))
        |> List.mapi (fun i e -> {
            Position = i + 1;
            BoardGame = e.BoardGame;
            Evaluation = e
        })

    let GamesToPlay (evaluations : Evaluation list) =
        evaluations
        |> List.filter (fun e -> e.BoardGame.PlayCount = 0 && Option.isNone e.BoardGame.MyRating)
        |> List.sortByDescending (fun e -> e.BoardGame.GeekRating)
        |> List.mapi (fun i e -> {
            Position = i + 1;
            BoardGame = e.BoardGame;
            Evaluation = e
        })

    let GamesToSellOrTrade (evaluations : Evaluation list) =
        evaluations
        |> List.filter (fun e -> e.BoardGame.Type <> "boardgameexpansion")
        |> List.sortBy (fun e -> e.Scores |> List.sumBy (fun s -> s.Value))
        |> List.mapi (fun i e -> {
            Position = i + 1;
            BoardGame = e.BoardGame;
            Evaluation = e;
        })

    let BggRanking (collection: Collection) =
        collection.BoardGames
        |> List.filter (fun b -> b.OverallRank > 0)
        |> List.sortBy (fun b -> b.OverallRank)