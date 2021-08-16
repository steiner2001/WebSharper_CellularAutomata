namespace Samples

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Html
open WebSharper.UI.Client
open WebSharper.Mvu

[<JavaScript>]
module Counter =
    
    let height = 20
    let width = 30

    type Model = { 
        Grid: ((int*int) * bool) list
        Trigger: bool
    }

    let mutable gridList = []

    for y in [0..height] do
       for x in [0..width] do
        gridList <- gridList@[((x,y),false)]

    let updateGrid (model:Model) (x, y) =
        if model.Trigger then
            let mutable neighbours = 0
            if y > 0 && snd (model.Grid.[x + (y-1) * width]) then neighbours <- neighbours + 1
            if y < height-1 && snd (model.Grid.[x + (y+1) * width]) then neighbours <- neighbours + 1
            if x > 0 && snd (model.Grid.[(x-1) + y * width]) then neighbours <- neighbours + 1
            if x < width-1 && snd (model.Grid.[(x+1) + y * width]) then neighbours <- neighbours + 1
            if x > 0 && y > 0 && snd (model.Grid.[(x-1) + (y-1) * width]) then neighbours <- neighbours + 1
            if x < width-1 && y > 0 && snd (model.Grid.[(x+1) + (y-1) * width]) then neighbours <- neighbours + 1
            if y < height-1 && x > 0 && snd (model.Grid.[(x-1) + (y+1) * width]) then neighbours <- neighbours + 1
            if x < width-1  && y < height && snd (model.Grid.[(x+1) + y * width]) then neighbours <- neighbours + 1
            Console.Log(neighbours)
            if neighbours < 2 || neighbours > 3 then
                {model with Grid = model.Grid |> List.map(fun ((curx, cury), value) -> if x = curx && y = cury then ((x, y), false) else (curx, cury), value)}
            else if neighbours = 3 then
                {model with Grid = model.Grid |> List.map(fun ((curx, cury), value) -> if x = curx && y = cury then ((x, y), true) else (curx, cury), value)}
            else model
        else model
    
    type Message = Step of int * int | SetTrigger | Check of int * int

    let Update (msg: Message) (model: Model) =
        match msg with
        | SetTrigger -> Console.Log("Trigger set");{ model with Trigger = not model.Trigger }
        | Step (x, y) -> updateGrid model (x, y)
        | Check (x, y) ->
            {
                model with
                    Grid =
                        model.Grid
                        |> List.map(fun ((curx, cury), value) ->
                            if x = curx && y = cury then
                                ((x, y), not value)
                            else
                                ((curx, cury), value))}

    let createRow (x, y) (model: View<Model>) (dispatch: Dispatch<Message>) =
        List.init x (fun i ->
            let tr = model.Map(fun model -> snd model.Grid.[i + y * (width+1)])
            div [
                on.click (fun _ _ -> dispatch (Check (i, y)))
                on.viewUpdate model (fun _ _ -> dispatch (Step (i, y)))
                attr.classDyn (tr.Map(fun t -> if t then "gol-box gol-triggered" else "gol-box"))
            ] [])
         
    let createRows (x, y) (model: View<Model>) (dispatch: Dispatch<Message>)=
        List.init y (fun i -> div [attr.``class`` "gol-row"] (createRow (x, i) model dispatch))


    let Render (dispatch: Dispatch<Message>) (model: View<Model>) =
        div [] [
            div [][
                yield! (createRows (width, height) model dispatch)
            ]
            button [on.click (fun _ _ -> dispatch SetTrigger)] [text "Start/Stop"]
        ]

    [<SPAEntryPoint>]
    let Main =
        App.CreateSimple { Grid = gridList; Trigger = false } Update Render
        |> App.Run
        |> Doc.RunById "main"
