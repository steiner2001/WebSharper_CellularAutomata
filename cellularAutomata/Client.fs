namespace CellularAutomata

open WebSharper
open WebSharper.UI
open WebSharper.UI.Html
open WebSharper.UI.Client

[<JavaScript>]
module Client =
    (* type Node () =
        let mutable isActive = Var.Create false
        let mutable siblings = View.Const 0
        member this.IsActive = isActive
        member this.Siblings = siblings *)
 
    [<SPAEntryPoint>]
    let Main =
        let (!!) (b: bool) = if b then 1 else 0
        let trigger = Var.Create false
        let lm : ListModel<(int * int), (int * int) * View<bool>> = ListModel.Create fst []
        let dimensions = (40,60)
        let createTriggered (x: int, y: int) =
            let tr = Var.Create false
            lm.Add((x, y), tr.View)
            let baseView =
                lm.View
                |> View.Map2 (fun x y -> x, y) trigger.View
                |> View.Bind (fun (trigger, _) ->
                    if trigger then
                        //lm.Wrap
                        let findDir k =
                            lm.TryFindByKey k
                            |> Option.map (snd)
                            |> Option.defaultValue (View.Const false)
                        let north =
                            findDir (x-1, y)
                        let south =
                            findDir (x+1, y)
                        let west =
                            findDir (x, y-1)
                        let east =
                            findDir (x, y+1)
                        let northwest = 
                            findDir (x-1, y-1)
                        let northeast = 
                            findDir (x-1, y+1)
                        let southwest = 
                            findDir (x+1, y-1)
                        let southeast = 
                            findDir (x+1, y+1)
                        let current =
                            findDir (x, y)
                        View.Map2 (fun c counter -> if counter = 0 && not c then None else Some counter)
                            current
                            (View.Map2 (+)
                                (View.Map2 (+)
                                    (View.Map2 (fun x y -> !!x + !!y) north south)
                                    (View.Map2 (fun x y -> !!x + !!y) east west))
                                (View.Map2 (+)
                                    (View.Map2 (fun x y -> !!x + !!y) northwest southeast)
                                    (View.Map2 (fun x y -> !!x + !!y) northeast southwest)))
                    else
                        View.Const None
                )
            baseView
            |> View.Sink (function
                | Some counter ->
                    async {
                        do! Async.Sleep 100
                        if counter < 2 || counter > 3 then
                            tr.UpdateMaybe (fun x ->
                                if not x then None else Some false)
                        else if counter = 3 then
                            tr.UpdateMaybe (fun x ->
                                if x then None else Some true)

                    }
                    |> Async.Start
                    (* if counter < 2 || counter > 3 then
                            tr.UpdateMaybe (fun x -> if not x then None else Some false)
                        else if counter = 3 then
                            tr.UpdateMaybe (fun _ -> Some true) *)
                | _ -> ()
            )
            div [on.click (fun _ _ -> tr.Update not); attr.classDyn (tr.View.Map(fun x -> if x then "gol-box gol-triggered" else "gol-box"))] [] //[if (x <> (fst dimensions)/2 || y <> (snd dimensions)/2) then yield attr.disabled "disabled"] tr
            
        let createRow (x, y)=
            List.init y (fun i -> createTriggered (x, i))
             
        let createRows (x, y)=
            List.init x (fun i -> div [attr.``class`` "gol-row"] (createRow (i, y)))

        div [
            //on.viewUpdate
        ] [
            div [] [
                yield! createRows dimensions
            ]
            button [
                on.click (fun _ _ ->
                    trigger.Update not)
            ] [
                textView (trigger.View.Map(fun x -> if x then "Stop" else "Start"))
            ]
        ]
        |> Doc.RunById "main"