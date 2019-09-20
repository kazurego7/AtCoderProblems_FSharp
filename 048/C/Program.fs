open System

let input() =
    Console.ReadLine().Split()
    |> Array.toList
    |> List.map int32

let interval a b = seq { a..(b - 1) }

[<EntryPoint>]
let main _ =
    let [ N; x ] = input()
    let mutable a = input() |> List.toArray
    let mutable ans = 0L
    if a.[0] > x then
        ans <- int64 (a.[0] - x)
        a.[0] <- x
    for i in interval 0 (N - 1) do
        if a.[i] + a.[i + 1] > x then
            ans <- ans + int64 (a.[i + 1] + a.[i] - x)
            a.[i + 1] <- x - a.[i]
    printfn "%d" ans
    0 // return an integer exit code
