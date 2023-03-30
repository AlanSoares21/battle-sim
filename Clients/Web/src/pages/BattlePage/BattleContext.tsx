import { createContext } from "react";
import { IBattleData } from "../../interfaces";
import { ServerConnection } from "../../server";
import { HubConnectionBuilder } from '@microsoft/signalr';

export interface IBattleContext {
    battle: IBattleData;
    server: ServerConnection;
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
    },
    server: new ServerConnection(
        new HubConnectionBuilder().withUrl('http://localhost').build()
    )
});