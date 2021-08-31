module App

open Fable.React
open Fable.React.Props
open Elmish
open Elmish.React

let width = 60
let height = 40

type Model =
    {
        Grid : ((int * int) * bool) list
        Trigger : bool
    }

type Message =
    | Step
    | Check of int * int
    | SetTrigger

let mutable grid = []
for y in [0..height-1] do
    for x in [0..width-1] do
        let mutable value = false
        let mutable cell = ((x, y), value)
        grid <- grid@[cell]

let startModel () =
    {
        Grid = grid
        Trigger = false
    }

let updateGrid (grid : ((int * int) * bool) list) (trigger : bool)=
        if trigger then
            let mutable helperList = []
            for y in [0..height-1] do
                for x in [0..width-1] do
                    let mutable neighbours = 0
                    if y > 0 && snd (grid.[x + (y-1) * (width)]) then neighbours <- neighbours + 1
                    if y < height-1 && snd (grid.[x + (y+1) * (width)]) then neighbours <- neighbours + 1
                    if x > 0 && snd (grid.[(x-1) + y * (width)]) then neighbours <- neighbours + 1
                    if x < width-1 && snd (grid.[(x+1) + y * (width)]) then neighbours <- neighbours + 1
                    if x > 0 && y > 0 && snd (grid.[(x-1) + (y-1) * (width)]) then neighbours <- neighbours + 1
                    if x < width-1 && y > 0 && snd (grid.[(x+1) + (y-1) * (width)]) then neighbours <- neighbours + 1
                    if y < height-1 && x > 0 && snd (grid.[(x-1) + (y+1) * (width)]) then neighbours <- neighbours + 1
                    if x < width-1  && y < height-1 && snd (grid.[(x+1) + (y+1) * (width)]) then neighbours <- neighbours + 1
                    if neighbours < 2 || neighbours > 3 then
                        let mutable value = false
                        let mutable cell = ((x, y), value)
                        helperList <- helperList@[cell]
                    else if neighbours = 3 then
                        let mutable value = true
                        let mutable cell = ((x, y), value)
                        helperList <- helperList@[cell]
                    else helperList <- helperList@[grid.[x + y * (width)]]
            helperList
        else grid


let update (msg : Message) (model : Model) =
    match msg with
    | Step ->
        let newGrid = updateGrid model.Grid model.Trigger
        {Grid = newGrid;Trigger = model.Trigger}
    | (Check (x, y)) ->
        let newGrid = List.map(fun ((curx, cury), value) ->
                        if curx = x && cury = y
                        then ((curx, cury), not value)
                        else ((curx, cury), value)) model.Grid
        {Grid = newGrid;Trigger = model.Trigger}
    | SetTrigger -> {Grid = model.Grid;Trigger = not model.Trigger}
    

let createRow (i : int) (model : Model) (dispatch : Dispatch<Message>) =
    List.init width (fun j ->
            let tr = List.map (fun model -> snd model.Grid.[j + i * (width+1)])
            div [
                OnClick (fun _ -> dispatch (Check (j, i)))
                Class (if snd model.Grid.[j + i * width] then "cell active" else "cell")
            ] [])

let createRows (model : Model) (dispatch : Dispatch<Message>) =
    List.init height (fun i -> div [Class "row"] (createRow i model dispatch))

let view (model : Model) (dispatch : Dispatch<Message>) =
    async {
        while true do
            do! Async.Sleep 1000
            dispatch Step
    }
    |> Async.Start
    (* dispatch Step *)
    div [][
        div[][yield! createRows model dispatch]
        button [OnClick (fun _ ->
            dispatch SetTrigger
        )][if model.Trigger then (str "Stop") else str "Start"]
    ]

Program.mkSimple startModel update view
|> Program.withReactHydrate "cellular"
|> Program.run