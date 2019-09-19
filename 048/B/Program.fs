// Learn more about F# at http://fsharp.org
open System

[<EntryPoint>]
let main argv =
    let [ a; b; x ] =
        Console.ReadLine().Split(' ')
        |> List.ofArray
        |> List.map int64

    let ans =
        if a = 0L then b / x + 1L
        else b / x - (a - 1L) / x

    printfn "%d" ans
    0 // return an integer exit code
