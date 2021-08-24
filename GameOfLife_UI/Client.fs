namespace CellularAutomata

open WebSharper
open WebSharper.UI
open WebSharper.UI.Html
open WebSharper.UI.Client

[<JavaScript>]
module Client =
    [<SPAEntryPoint>]
    let Main =
        let (!!) (b: bool) = if b then 1 else 0
        let trigger = Var.Create false

        let height = 20
        let width = 30

        let mutable gridList = []

        for y in [0..height-1] do
           for x in [0..width-1] do
            gridList <- gridList@[((x,y),false)]

        let lm : Var<((int*int) * bool) list> = Var.Create gridList

        let createTriggered (x: int, y: int) =
            let tr =
                lm.View.Map (fun l ->
                    l |> List.find (fun ((cx, cy), _) -> cx = x && cy = y) |> snd
                )
            let updateLM () =
                Var.Update lm (fun l -> l |> List.map (fun ((cx, cy), ov) -> if cx = x && cy = y then ((cx, cy), not ov) else ((cx, cy), ov)))
            div [on.click (fun _ _ -> updateLM ()); attr.classDyn (tr.Map(fun x -> if x then "gol-box gol-triggered" else "gol-box"))] [] //[if (x <> (fst dimensions)/2 || y <> (snd dimensions)/2) then yield attr.disabled "disabled"] tr
            
        let createRow (x, y)=
            List.init x (fun i -> createTriggered (i, y))
             
        let createRows (x, y)=
            List.init y (fun i -> div [attr.``class`` "gol-row"] (createRow (x, i)))

        let updateGrid trigger (lmv: ((int * int) * bool) list) =
            if trigger then
                let mutable helperList = []
                for y in [0..height-1] do
                    for x in [0..width-1] do
                        [
                            if y > 0 && snd (lmv.[x + (y-1) * (width)]) then 1 else 0
                            if y < height-1 && snd (lmv.[x + (y+1) * (width)]) then 1 else 0
                            if x > 0 && snd (lmv.[(x-1) + y * (width)]) then 1 else 0
                            if x < width-1 && snd (lmv.[(x+1) + y * (width)]) then 1 else 0
                            if x > 0 && y > 0 && snd (lmv.[(x-1) + (y-1) * (width)]) then 1 else 0
                            if x < width-1 && y > 0 && snd (lmv.[(x+1) + (y-1) * (width)]) then 1 else 0
                            if y < height-1 && x > 0 && snd (lmv.[(x-1) + (y+1) * (width)]) then 1 else 0
                            if x < width-1  && y < height-1 && snd (lmv.[(x+1) + (y+1) * (width)]) then 1 else 0
                        ]
                        |> List.sum
                        |> fun neighbours ->
                            if neighbours < 2 || neighbours > 3 then
                                helperList <- helperList@[((x, y), false)]
                            else if neighbours = 3 then
                                helperList <- helperList@[((x, y), true)]
                            else helperList <- helperList@[lmv.[x + y * (width)]]
                lm.Set helperList
            else
                ()

        div [
            on.viewUpdate (lm.View |> View.Map2 (fun x y -> x, y) trigger.View) (fun _ (trigger, lm) -> updateGrid trigger lm)
        ] [
            div [] [
                yield! createRows (width, height)
            ]
            button [
                on.click (fun _ _ ->
                    trigger.Update not)
            ] [
                textView (trigger.View.Map(fun x -> if x then "Stop" else "Start"))
            ]
        ]
        |> Doc.RunById "main"