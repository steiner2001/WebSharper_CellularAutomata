import { Union, Record } from "./.fable/fable-library.3.2.9/Types.js";
import { union_type, record_type, list_type, bool_type, tuple_type, int32_type } from "./.fable/fable-library.3.2.9/Reflection.js";
import { getEnumerator, createAtom } from "./.fable/fable-library.3.2.9/Util.js";
import { initialize, map, item, singleton, append, empty } from "./.fable/fable-library.3.2.9/List.js";
import { singleton as singleton_2, delay, toList } from "./.fable/fable-library.3.2.9/Seq.js";
import { rangeDouble } from "./.fable/fable-library.3.2.9/Range.js";
import * as react from "react";
import { sleep, start } from "./.fable/fable-library.3.2.9/Async.js";
import { singleton as singleton_1 } from "./.fable/fable-library.3.2.9/AsyncBuilder.js";
import { ProgramModule_mkSimple, ProgramModule_run } from "./.fable/Fable.Elmish.4.0.0-alpha-1/program.fs.js";
import { Program_withReactHydrate } from "./.fable/Fable.Elmish.React.4.0.0-alpha-1/react.fs.js";

export const width = 60;

export const height = 40;

export class Model extends Record {
    constructor(Grid, Trigger) {
        super();
        this.Grid = Grid;
        this.Trigger = Trigger;
    }
}

export function Model$reflection() {
    return record_type("App.Model", [], Model, () => [["Grid", list_type(tuple_type(tuple_type(int32_type, int32_type), bool_type))], ["Trigger", bool_type]]);
}

export class Message extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Step", "Check", "SetTrigger"];
    }
}

export function Message$reflection() {
    return union_type("App.Message", [], Message, () => [[], [["Item1", int32_type], ["Item2", int32_type]], []]);
}

export let grid = createAtom(empty());

(function () {
    const enumerator = getEnumerator(toList(rangeDouble(0, 1, height - 1)));
    try {
        while (enumerator["System.Collections.IEnumerator.MoveNext"]()) {
            const y = enumerator["System.Collections.Generic.IEnumerator`1.get_Current"]() | 0;
            const enumerator_1 = getEnumerator(toList(rangeDouble(0, 1, width - 1)));
            try {
                while (enumerator_1["System.Collections.IEnumerator.MoveNext"]()) {
                    const x = enumerator_1["System.Collections.Generic.IEnumerator`1.get_Current"]() | 0;
                    let value = false;
                    let cell = [[x, y], value];
                    grid(append(grid(), singleton(cell)), true);
                }
            }
            finally {
                enumerator_1.Dispose();
            }
        }
    }
    finally {
        enumerator.Dispose();
    }
})();

export function startModel() {
    return new Model(grid(), false);
}

export function updateGrid(grid_1, trigger) {
    if (trigger) {
        let helperList = empty();
        const enumerator = getEnumerator(toList(rangeDouble(0, 1, height - 1)));
        try {
            while (enumerator["System.Collections.IEnumerator.MoveNext"]()) {
                const y = enumerator["System.Collections.Generic.IEnumerator`1.get_Current"]() | 0;
                const enumerator_1 = getEnumerator(toList(rangeDouble(0, 1, width - 1)));
                try {
                    while (enumerator_1["System.Collections.IEnumerator.MoveNext"]()) {
                        const x = enumerator_1["System.Collections.Generic.IEnumerator`1.get_Current"]() | 0;
                        let neighbours = 0;
                        if ((y > 0) ? item(x + ((y - 1) * width), grid_1)[1] : false) {
                            neighbours = ((neighbours + 1) | 0);
                        }
                        if ((y < (height - 1)) ? item(x + ((y + 1) * width), grid_1)[1] : false) {
                            neighbours = ((neighbours + 1) | 0);
                        }
                        if ((x > 0) ? item((x - 1) + (y * width), grid_1)[1] : false) {
                            neighbours = ((neighbours + 1) | 0);
                        }
                        if ((x < (width - 1)) ? item((x + 1) + (y * width), grid_1)[1] : false) {
                            neighbours = ((neighbours + 1) | 0);
                        }
                        if (((x > 0) ? (y > 0) : false) ? item((x - 1) + ((y - 1) * width), grid_1)[1] : false) {
                            neighbours = ((neighbours + 1) | 0);
                        }
                        if (((x < (width - 1)) ? (y > 0) : false) ? item((x + 1) + ((y - 1) * width), grid_1)[1] : false) {
                            neighbours = ((neighbours + 1) | 0);
                        }
                        if (((y < (height - 1)) ? (x > 0) : false) ? item((x - 1) + ((y + 1) * width), grid_1)[1] : false) {
                            neighbours = ((neighbours + 1) | 0);
                        }
                        if (((x < (width - 1)) ? (y < (height - 1)) : false) ? item((x + 1) + ((y + 1) * width), grid_1)[1] : false) {
                            neighbours = ((neighbours + 1) | 0);
                        }
                        if ((neighbours < 2) ? true : (neighbours > 3)) {
                            let value = false;
                            let cell = [[x, y], value];
                            helperList = append(helperList, singleton(cell));
                        }
                        else if (neighbours === 3) {
                            let value_1 = true;
                            let cell_1 = [[x, y], value_1];
                            helperList = append(helperList, singleton(cell_1));
                        }
                        else {
                            helperList = append(helperList, singleton(item(x + (y * width), grid_1)));
                        }
                    }
                }
                finally {
                    enumerator_1.Dispose();
                }
            }
        }
        finally {
            enumerator.Dispose();
        }
        return helperList;
    }
    else {
        return grid_1;
    }
}

export function update(msg, model) {
    switch (msg.tag) {
        case 1: {
            const y = msg.fields[1] | 0;
            const x = msg.fields[0] | 0;
            const newGrid_1 = map((tupledArg) => {
                const _arg1 = tupledArg[0];
                const value = tupledArg[1];
                const cury = _arg1[1] | 0;
                const curx = _arg1[0] | 0;
                return ((curx === x) ? (cury === y) : false) ? [[curx, cury], !value] : [[curx, cury], value];
            }, model.Grid);
            return new Model(newGrid_1, model.Trigger);
        }
        case 2: {
            return new Model(model.Grid, !model.Trigger);
        }
        default: {
            const newGrid = updateGrid(model.Grid, model.Trigger);
            return new Model(newGrid, model.Trigger);
        }
    }
}

export function createRow(i, model, dispatch) {
    return initialize(width, (j) => {
        const tr = (list) => map((model_1) => item(j + (i * (width + 1)), model_1.Grid)[1], list);
        return react.createElement("div", {
            onClick: (_arg1) => {
                dispatch(new Message(1, j, i));
            },
            className: item(j + (i * width), model.Grid)[1] ? "cell active" : "cell",
        });
    });
}

export function createRows(model, dispatch) {
    return initialize(height, (i) => react.createElement("div", {
        className: "row",
    }, ...createRow(i, model, dispatch)));
}

export function view(model, dispatch) {
    start(singleton_1.Delay(() => singleton_1.While(() => true, singleton_1.Delay(() => singleton_1.Bind(sleep(1000), () => {
        dispatch(new Message(0));
        return singleton_1.Zero();
    })))));
    return react.createElement("div", {}, react.createElement("div", {}, ...toList(delay(() => createRows(model, dispatch)))), react.createElement("button", {
        onClick: (_arg1_1) => {
            dispatch(new Message(2));
        },
    }, ...toList(delay(() => (model.Trigger ? singleton_2("Stop") : singleton_2("Start"))))));
}

ProgramModule_run(Program_withReactHydrate("cellular", ProgramModule_mkSimple(startModel, (msg, model) => update(msg, model), (model_1, dispatch) => view(model_1, dispatch))));

