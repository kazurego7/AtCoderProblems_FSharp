open System

[<EntryPoint>]
let main argv =
    let [ _; s; _ ] = Console.ReadLine().Split(' ') |> Array.toList
    let ans = "A" + (string s.[0]) + "C"
    printfn "%s" ans |> ignore
    0 // return an integer exit code
