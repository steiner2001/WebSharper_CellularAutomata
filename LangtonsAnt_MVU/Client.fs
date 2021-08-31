namespace Samples

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Html
open WebSharper.UI.Client
open WebSharper.Mvu

[<JavaScript>]
module Counter =
    
    let height = 50
    let width = 50

    type Direction =
        | Left
        | Right
        | Up
        | Down

    type Model = { 
        Grid: ((int*int) * bool) list
        Ant: (int * int) * Direction
    }

    let mutable gridList = []

    for y in [0..height] do
       for x in [0..width] do
        gridList <- gridList@[((x,y),false)]

    let startAnt = ((int(width/2), int(height/2)), Up)

    let startModel : Model = {Grid = gridList; Ant = startAnt}

    let updateGrid (model:Model) (x, y) =
        let resultModel =
            if snd model.Grid.[x + y * (width+1)]
            then
                let newGrid = List.map(fun ((curx, cury), v) -> if curx = x && cury = y then ((curx, cury), false) else ((curx, cury), v)) model.Grid
                let newAnt = 
                    match snd model.Ant with
                    | Left -> ((x, y+1), Down)
                    | Right -> ((x, y-1), Up)
                    | Up -> ((x-1, y), Left)
                    | Down -> ((x+1, y), Right)
                {Grid = newGrid; Ant = newAnt}
            else
                let newGrid = List.map(fun ((curx, cury), v) -> if curx = x && cury = y then ((curx, cury), true) else ((curx, cury), v)) model.Grid
                let newAnt = 
                    match snd model.Ant with
                    | Right -> ((x, y+1), Down)
                    | Left -> ((x, y-1), Up)
                    | Down -> ((x-1, y), Left)
                    | Up -> ((x+1, y), Right)
                {Grid = newGrid; Ant = newAnt}
        resultModel
    
    type Message = Step of int * int

    let Update (msg: Message) (model: Model) =
        match msg with
        | Step (x, y)-> updateGrid model (x, y)


    let createRow (x, y) (model: View<Model>) (dispatch: Dispatch<Message>) =
        List.init x (fun i ->
            let tr = model.Map(fun model -> snd model.Grid.[i + y * (width+1)])
            div [
                on.viewUpdate model (fun _ model ->
                    let curx = fst (fst model.Ant)
                    let cury = snd (fst model.Ant)
                    if curx = i && cury = y then dispatch (Step (i, y))
                )
                attr.classDyn (tr.Map(fun t -> if t then "gol-box gol-triggered" else "gol-box"))
            ] [])
         
    let createRows (x, y) (model: View<Model>) (dispatch: Dispatch<Message>) =
        List.init y (fun i -> div [attr.``class`` "gol-row"] (createRow (x, i) model dispatch))


    let Render (dispatch: Dispatch<Message>) (model: View<Model>) =
        div [
            on.afterRender (fun _ -> dispatch (Step (fst startAnt)))
        ] [
            div [][
                yield! (createRows (width, height) model dispatch)
            ]
        ]

    [<SPAEntryPoint>]
    let Main =
        App.CreateSimple startModel Update Render
        |> App.Run
        |> Doc.RunById "main"
