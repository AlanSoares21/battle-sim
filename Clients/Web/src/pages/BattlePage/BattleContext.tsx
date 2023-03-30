import { createContext } from "react";
import { IBattleData } from "../../interfaces";

export interface IBattleContext {
    battle: IBattleData;
}

export const BattleContext = createContext<IBattleContext>({
    battle: {
        board: {
            height: 4,
            width: 6,
            entitiesPosition: []
        },
        entities: [],
        id: ''
    }
});