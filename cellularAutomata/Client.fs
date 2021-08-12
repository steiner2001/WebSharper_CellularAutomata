namespace CellularAutomata

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Html
open WebSharper.UI.Client
open WebSharper.UI.Notation

[<JavaScript>]
module Client =
    [<SPAEntryPoint>]
    let Main =
        let (!!) (b: bool) = if b then 1 else 0
        let trigger = Var.Create false
        let lm : ListModel<(int * int), (int * int) * View<bool>> = ListModel.Create fst []
        let dimensions = (80,80)
        let createTriggered (x: int, y: int) (maxX, maxY) =
            let tr = Var.Create false
            lm.Add((x, y), tr.View)
            lm.View
            |> View.Bind (fun lms ->
                let north =
                    if x = 0 then View.Const false else lms |> Seq.find (fun (k, _) -> k = (x-1, y)) |> snd
                let south =
                    if x = maxX-1 then View.Const  false else lms |> Seq.find (fun (k, _) -> k = (x+1, y)) |> snd
                let west =
                    if y = 0 then View.Const false else lms |> Seq.find (fun (k, _) -> k = (x, y-1)) |> snd
                let east =
                    if y = maxY-1 then View.Const false else lms |> Seq.find (fun (k, _) -> k = (x, y+1)) |> snd
                let northwest = 
                    if x = 0 || y = 0 then View.Const false else lms |> Seq.find (fun (k, _) -> k = (x-1, y-1)) |> snd
                let northeast = 
                    if x = 0 || y = maxY-1 then View.Const false else lms |> Seq.find (fun (k, _) -> k = (x-1, y+1)) |> snd
                let southwest = 
                    if x = maxX-1 || y = 0 then View.Const false else lms |> Seq.find (fun (k, _) -> k = (x+1, y-1)) |> snd
                let southeast = 
                    if x = maxX-1 || y = maxY-1 then View.Const false else lms |> Seq.find (fun (k, _) -> k = (x+1, y+1)) |> snd
                View.Map2 (+)
                    (View.Map2 (+)
                        (View.Map2 (fun x y -> !!x + !!y) north south)
                        (View.Map2 (fun x y -> !!x + !!y) east west))
                    (View.Map2 (+)
                        (View.Map2 (fun x y -> !!x + !!y) northwest southeast)
                        (View.Map2 (fun x y -> !!x + !!y) northeast southwest))
            )
            |> View.Map2 (fun x y -> x, y) trigger.View
            |> View.Sink (fun (trigger, counter) ->
                if trigger then
                    (* async {
                        do! Async.Sleep 100
                        if counter < 2 || counter > 3 then
                            tr.UpdateMaybe (fun x -> if not x then None else Some false)
                        else if counter = 3 then
                            tr.UpdateMaybe (fun _ -> Some true)
                        //else tr.UpdateMaybe (fun x -> if x then None else Some true)

                    }
                    |> Async.Start *)
                    if counter < 2 || counter > 3 then
                            tr.UpdateMaybe (fun x -> if not x then None else Some false)
                        else if counter = 3 then
                            tr.UpdateMaybe (fun _ -> Some true)
            )
            div [on.click (fun _ _ -> tr.Update not); attr.classDyn (tr.View.Map(fun x -> if x then "gol-box gol-triggered" else "gol-box"))] [] //[if (x <> (fst dimensions)/2 || y <> (snd dimensions)/2) then yield attr.disabled "disabled"] tr
            
        let createRow (x, y) (mX, mY) =
            List.init y (fun i -> createTriggered (x, i) (mX, mY))
             
        let createRows (x, y)=
            List.init y (fun i -> div [attr.``class`` "gol-row"] (createRow (i, y) (x, y)))

        div [] [
            div [] [
                yield! createRows dimensions
            ]
            div [] [
                Doc.Button "Start" [] (fun _ ->
                    trigger.Update not
                )
            ]
        ]
        |> Doc.RunById "main"