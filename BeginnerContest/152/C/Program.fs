﻿module AtCoder

open Microsoft.FSharp.Collections
open System
open System.Collections
open System.Collections.Generic

#nowarn "0025" // パターンマッチが不完全である警告の無効

module Seq =
    let interval startInclusive endExclusive = seq { startInclusive .. (endExclusive - 1) }

    let inline mapAdjacent (op: 'a -> 'a -> 'b) (source: seq<'a>): seq<'b> =
        match source with
        | source when Seq.isEmpty source -> Seq.empty
        | _ -> Seq.map2 op source (Seq.tail source)

module Array2D =
    let inline transpose (array: 'a [,]): 'a [,] =
        let transposed = Array2D.zeroCreate (Array2D.length2 array) (Array2D.length1 array)
        transposed |> Array2D.mapi (fun i k _ -> array.[k, i])

module InputOutputs =
    let read(): string = Console.ReadLine()
    let reads(): string [] = read().Split()

    let readMatrix (row: int32): string [,] =
        let mutable lines = Array.zeroCreate row
        for i in Seq.interval 0 row do
            lines.[i] <- reads()
        lines |> array2D
    
    let readMatrixNoSpace (row: int32): string [,] =
        let mutable lines = Array.zeroCreate row
        for i in Seq.interval 0 row do
            lines.[i] <- read () |> Seq.map  string
        lines |> array2D

    let readInt32(): int32 = read() |> int32
    let readInt64(): int64 = read() |> int64
    let readInt32s(): int32 [] = reads() |> Array.map int32
    let readInt64s(): int64 [] = reads() |> Array.map int64

    let readMatrixInt32 (rowNum: int32): int32 [,] = readMatrix rowNum |> Array2D.map int32

    let readMatrixInt64 (rowNum: int32): int64 [,] = readMatrix rowNum |> Array2D.map int64

    let inline print (item: 'a): unit = printfn "%s" (string item)

    let inline printRow (line: seq<'a>): unit =
        let strs = line |> Seq.map string
        if not (Seq.isEmpty strs) then
            printf "%s" (Seq.head strs)
            for s in Seq.skip 1 strs do
                printf " %s" s
        printf "\n"

    let inline printColumn (line: seq<'a>): unit =
        for item in line do
            print item

    let inline printMatrix (matrix: 'a [,]): unit =
        if matrix.Length = 0 then 
            print ""
        else
            let height = matrix.[*, 0].Length
            for i in (Seq.interval 0 height) do
                matrix.[i, *]
                |> Seq.map string
                |> String.concat " "
                |> print
    
    let inline printMatrixNoSpace (matrix: 'a [,]): unit =
        if matrix.Length = 0 then 
            print ""
        else
            let height = matrix.[*, 0].Length
            for i in (Seq.interval 0 height) do
                matrix.[i, *]
                |> Seq.map string
                |> String.concat ""
                |> print

module NumericFunctions =
    type Mods =
        { divisor: int32 }

        member this.Mod(a: int64) =
            let b = a % int64 this.divisor |> int32
            if b < 0 then b + this.divisor else b

        member this.Mod(a: int32) = this.Mod(int64 a)

        member this.Add (a: int32) (b: int32): int32 = (this.Mod a + this.Mod b) % this.divisor

        member this.Sub (a: int32) (b: int32): int32 =
            let sub = (this.Mod a - this.Mod b) % this.divisor
            if sub < 0 then sub + this.divisor else sub

        member this.Mul (a: int32) (b: int32): int32 =
            (int64 (this.Mod a) * int64 (this.Mod b)) % int64 this.divisor |> int32

        member this.Div (a: int32) (b: int32): int32 = this.Mul a (this.Inv b)

        /// 二分累積 O(Log N)
        member this.Pow (b: int32) (n: int32): int32 =
            let digit = int32 (Math.Log(float n, 2.0))

            let seqs =
                seq { 0 .. digit }
                |> Seq.scan (fun acm _ -> this.Mul acm acm) b
                |> Seq.toArray
            seq { 0 .. digit }
            |> Seq.fold (fun acm i ->
                if ((n >>> i) &&& 1) = 1 then this.Mul acm seqs.[i] else acm) 1

        /// フェルマーの小定理より
        member this.Inv(a: int32): int32 = this.Pow a (this.divisor - 2)

        member this.Perm (n: int32) (k: int32): int32 =
            match (n, k) with
            | (n, _) when n < 0 -> invalidArg "n" "n >= 0"
            | (_, k) when k < 0 -> invalidArg "k" "k >= 0"
            | (n, k) when k > n -> 0
            | _ -> seq { n - k + 1 .. n } |> Seq.fold this.Mul 1

        member this.FactTable(nMax: int32): int32 [] =
            seq { 1 .. nMax }
            |> Seq.scan this.Mul 1
            |> Seq.toArray

        /// パスカルの三角形 O(N^2)
        member this.CombTable(nMax: int32): int32 [,] =
            let table = Array2D.zeroCreate (nMax + 1) (nMax + 1)
            for n in 0 .. nMax do
                for k in 0 .. nMax do
                    match (n, k) with
                    | (n, k) when n < k -> table.[n, k] <- 0
                    | (_, k) when k = 0 -> table.[n, k] <- 1
                    | _ ->
                        table.[n, k] <- int64 table.[n - 1, k - 1] + int64 table.[n - 1, k] % int64 this.divisor
                                        |> int32
            table

    let isEven (a: int64): bool = a % 2L = 0L
    let isOdd (a: int64): bool = not (isEven a)

    /// ユークリッドの互除法 O(Log N)
    let rec gcd (m: int64) (n: int64): int64 =
        match (m, n) with
        | (m, _) when m <= 0L -> invalidArg "m" "m <= 0"
        | (_, n) when n <= 0L -> invalidArg "n" "n <= 0"
        | (m, n) when m < n -> gcd n m
        | (m, n) when m % n = 0L -> n
        | _ -> gcd n (m % n)

    /// gcdを使っているため O(Log N)
    let lcm (m: int64) (n: int64): int64 = (bigint m) * (bigint n) / bigint (gcd m n) |> Checked.int64

    /// O(√N)
    let divisors (m: int64): seq<int64> =
        match m with
        | m when m <= 0L -> invalidArg "m" "m <= 0"
        | _ ->
            let sqrtM = int (sqrt (float m))

            let overRootM =
                Seq.interval 1 (sqrtM + 1)
                |> Seq.map int64
                |> Seq.filter (fun d -> m % d = 0L)
                |> Seq.rev
            overRootM
            |> if int64 sqrtM * int64 sqrtM = m then Seq.tail else id
            |> Seq.map (fun x -> m / x)
            |> Seq.append overRootM

    /// O(√N)
    let rec commonDivisor (m: int64) (n: int64): seq<int64> =
        match (m, n) with
        | (_, n) when n <= 0L -> invalidArg "n" "n <= 0"
        | (m, n) when m < n -> commonDivisor n m
        | _ -> divisors n |> Seq.filter (fun nd -> m % nd = 0L)

    /// エラトステネスの篩 O(N loglog N)
    let primes (n: int32): seq<int32> =
        match n with
        | n when n <= 1 -> invalidArg "n" "n <= 1"
        | _ ->
            let sqrtN = int32 (sqrt (float n))
            let mutable sieve = Seq.interval 2 (n + 1) |> Seq.toList
            let mutable ps = []
            while not (List.isEmpty sieve) && List.head sieve <= sqrtN do
                let m = List.head sieve
                ps <- m :: ps
                sieve <- List.filter (fun p -> p % m <> 0) sieve
            List.append ps sieve |> Seq.ofList

    /// 試し割り法 O(√N)
    let primeFactrization (n: int64): seq<int64 * int64> =
        match n with
        | n when n <= 1L -> invalidArg "n" "n <= 1"
        | _ ->
            let mutable i = 2L
            let mutable m = n
            let mutable ps = []
            while i * i <= n do
                if m % i = 0L then
                    let mutable count = 0L
                    while m % i = 0L do
                        count <- count + 1L
                        m <- m / i
                    ps <- (int64 i, count) :: ps
                i <- i + 1L
            if m <> 1L then ps <- (m, 1L) :: ps
            List.toSeq ps

module Algorithm =
    let rec binarySearch (predicate: int64 -> bool) (exclusiveNg: int64) (exclusiveOk: int64): int64 =
        match (exclusiveOk, exclusiveNg) with
        | (ok, ng) when abs (ok - ng) = 1L -> ok
        | _ ->
            let mid = (exclusiveOk + exclusiveNg) / 2L
            if predicate mid
            then binarySearch predicate exclusiveNg mid
            else binarySearch predicate mid exclusiveOk

    let runLengthEncoding (source: string): seq<string * int32> =
        match source.Length with
        | n when n = 0 -> Seq.empty
        | n ->
            let cutIxs = Seq.interval 1 n |> Seq.filter (fun i -> source.[i] <> source.[i - 1])
            Seq.append (Seq.append (seq { yield 0 }) cutIxs) (seq { yield n })
            |> Seq.mapAdjacent (fun i0 i1 -> (string source.[i0], i1 - i0))

    let rec ternarySearchDownward (left: float) (right: float) (convexFunction: float -> float) (allowableError: float) =
        match left, right, convexFunction, allowableError with
        | l, r, f, e when r - l < e -> l
        | l, r, f, e ->
            let ml = l + (r - l) / 3.0
            let mr = l + (r - l) / 3.0 * 2.0
            if f ml < f mr then ternarySearchDownward l mr f e
            else if f ml > f mr then ternarySearchDownward ml r f e
            else ternarySearchDownward ml mr f e

    let rec ternarySearchUpward (left: float) (right: float) (convexFunction: float -> float) (allowableError: float) =
        match left, right, convexFunction, allowableError with
        | l, r, f, e when r - l < e -> l
        | l, r, f, e ->
            let ml = l + (r - l) / 3.0
            let mr = l + (r - l) / 3.0 * 2.0
            if f ml < f mr then ternarySearchUpward ml r f e
            else if f ml > f mr then ternarySearchUpward l mr f e
            else ternarySearchUpward ml mr f e

    let checkFlag (flag: int64) (flagNumber: int): bool =
        if (flag < 0L) then invalidArg "flag" "flag < 0"
        if (flagNumber < 0) then invalidArg "flagNumber" "flagNumber < 0"
        flag >>> flagNumber &&& 1L = 1L

    let rec permutaions (xs: list<'a>): list<list<'a>> =
        match xs with
        | [] -> [ [] ]
        | [ x ] -> [ [ x ] ]
        | _ ->
            xs
            |> List.mapi (fun i x -> x, (List.append (List.take i xs) (List.skip (i + 1) xs)))
            |> List.collect (fun (y, other) -> List.map (fun ys -> y :: ys) (permutaions other))

    let rec fastPermutations (xs: list<'a>) =
        let rec insertions (x: 'a) (ys: list<'a>): list<list<'a>> =
            match ys with
            | [] -> [ [ x ] ]
            | (z :: zs) as l -> (x :: l) :: (List.map (fun w -> z :: w) (insertions x zs))
        match xs with
        | [] -> [ [] ]
        | y :: ys -> List.collect (insertions y) (fastPermutations ys)

module DataStructure =

    type Id = Id of Int32

    type private Parent(n: int32) =
        let mutable parent = Array.init n Id

        member this.Item
            with get (Id u) = parent.[u]
            and set (Id u) (Id v) = parent.[u] <- Id v

    type private Size(n: int32) =
        let mutable size = Array.create n 1

        member this.Item
            with get (Id u) = size.[u]
            and set (Id u) next = size.[u] <- next

    type UnionFind(n: int32) =
        let parent = Parent n

        let rec root u =
            if parent.[u] = u then
                u
            else
                let rootParent = root parent.[u]
                parent.[u] <- rootParent
                rootParent

        let mutable size = Size n

        member this.Unite (u: Id) (v: Id): unit =
            if root u = root v then
                ()
            else
                parent.[root u] <- root v
                size.[root v] <- size.[root u] + size.[root v]

        member this.Size(u: Id): int32 = size.[root u]

        member this.Find (u: Id) (v: Id): bool = root u = root v

        member this.Id(u: int32): Id = root (Id u)

    let reverseCompare (x: 'a) (y: 'a): int32 = compare x y * -1


    type PriorityQueue<'a when 'a: comparison and 'a: equality>(values: seq<'a>, comparison: 'a -> 'a -> int32) =

        let dict =
            let sorted = SortedDictionary<'a, Queue<'a>>(ComparisonIdentity.FromFunction comparison)
            Seq.groupBy id values |> Seq.iter (fun (key, value) -> sorted.Add(key, (new Queue<'a>(value))))
            sorted

        let mutable size = dict.Count
        new(values: seq<'a>) = PriorityQueue(values, compare)

        member this.Peek =
            if Seq.isEmpty dict then invalidOp "queue is Empty"

            (Seq.head dict).Value |> Seq.head

        member this.Size = size

        member this.Enqueue(item: 'a): unit =
            if dict.ContainsKey item then
                dict.[item].Enqueue(item)
            else
                let added = new Queue<'a>()
                added.Enqueue(item)
                dict.Add(item, added)
            size <- size + 1

        member this.Dequeue(): 'a =
            if Seq.isEmpty dict then invalidOp "queue is Empty"

            let first = Seq.head dict
            if first.Value.Count <= 1 then dict.Remove(first.Key) |> ignore

            size <- size - 1
            first.Value.Dequeue()

        interface IEnumerable<IEnumerable<'a>> with
            member this.GetEnumerator() =
                (seq {
                    for kv in dict -> seq kv.Value
                 }).GetEnumerator()

        interface IEnumerable with
            member this.GetEnumerator() = (this :> IEnumerable<IEnumerable<'a>>).GetEnumerator() :> IEnumerator

module Template =
    open InputOutputs
    /// 木構造の BFS ( DFS は、 order の Queue の部分を Stack にして、 Dequeue を Pop 、 Enqueue を Push にすれば良いだけ)
    let TreeBFS() =
        // 与えられた頂点数と辺
        let N = readInt32()
        let ab = readMatrixInt32 (N - 1)
        let a = ab.[*, 0]
        let b = ab.[*, 1]

        // 木構造の初期化
        let tree = Array.create N []

        for i in Seq.interval 0 (N - 1) do
            tree.[a.[i] - 1] <- (b.[i] - 1) :: tree.[a.[i] - 1]
            tree.[b.[i] - 1] <- (a.[i] - 1) :: tree.[b.[i] - 1]

        let order = new Queue<int32>()
        let reached = Array.create N false

        // 最初の頂点を追加
        order.Enqueue(0)
        reached.[0] <- true

        // 求めたい値
        ()

        // bfs
        while not (Seq.isEmpty order) do
            let node = order.Dequeue() // 現在見ている頂点
            let pruningCondition child = false // 枝刈り条件
            let nexts = tree.[node] |> List.filter (fun child -> not reached.[child] && not (pruningCondition child))

            // ************ 処理 ********************

            // 現在の頂点による処理
            ()

            // 現在の頂点と、次の頂点による処理
            for next in nexts do
                ()

            // **************************************

            for next in nexts do
                order.Enqueue(next)
                reached.[next] <- true

    // ヒント : 最短経路問題では、どの2頂点間の経路についても、通る頂点の数を最小にするように選べば良いので BFS が使える
    let GridGraphBFS() =
        // 与えられた高さと幅のグリッドグラフ
        let [| H; W |] = readInt32s()

        let S =
            Seq.interval 0 H
            |> Seq.map (fun _ -> read() |> Seq.map string)
            |> array2D

        let order = new Queue<int32 * int32>()
        let reached = Array2D.create H W false

        // 最初の頂点を追加
        order.Enqueue(0, 0)
        reached.[0, 0] <- true

        // 求めたい値
        ()

        // bfs
        while not (Seq.isEmpty order) do
            let (y0, x0) = order.Dequeue() // 現在見ている頂点

            let allSides =
                [ yield (y0, x0 - 1) // left
                  yield (y0 - 1, x0) // up
                  yield (y0, x0 + 1) // right
                  yield (y0 + 1, x0) ] // down
                |> List.filter (fun (y, x) -> 0 <= y && y < H && 0 <= x && x < W)

            let pruningCondition y x = false // 枝刈り条件

            let nexts = allSides |> List.filter (fun (y, x) -> not reached.[y, x] && not (pruningCondition y x))

            // ************ 処理 ********************

            // 現在の頂点による処理
            ()

            // 現在の頂点と、次の頂点による処理
            for (y, x) in nexts do
                ()

            // **************************************

            for (y, x) in nexts do
                order.Enqueue(y, x)
                reached.[y, x] <- true

    let WarshallFloyd () =
        // グラフの入力
        let [| N; M |] = readInt32s()
        let abc = readMatrixNoSpace M
        let a = abc.[*, 0] |> Array.map int32
        let b = abc.[*, 1] |> Array.map int32
        let c = abc.[*, 2] |> Array.map int64

        // 隣接行列の初期化
        let inf = pown 2L 60
        let G = Array2D.create N N inf

        for y in Seq.interval 0 N do
            for x in Seq.interval 0 N do
                if y = x then
                    G.[y,x] <- 0L
        
        for i in Seq.interval 0 M do
            G.[a.[i], b.[i]] <- c.[i]
            G.[b.[i], a.[i]] <- c.[i]

        for k in Seq.interval 0 N do
            for i in Seq.interval 0 N do
                for j in Seq.interval 0 N do
                    G.[i,j] <- min G.[i,j] (G.[i,k] + G.[k,j])
        
        


open Algorithm
open DataStructure
open InputOutputs
open NumericFunctions

[<EntryPoint>]
let main _ =
    let N = readInt32 ()
    let P = readInt32s ()

    let mutable ans = 0
    let mutable count = P.[0]
    for Pi in P do
        if (Pi <= count) then
            count <- Pi
            ans <- ans + 1
    
    print ans
    0 // return an integer exit code
