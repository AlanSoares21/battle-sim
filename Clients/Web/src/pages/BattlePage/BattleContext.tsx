import { createContext } from "react";
import { DamageDirection, IBattleData, IEntity } from "../../interfaces";
import { ServerConnection } from "../../server";
import { HubConnectionBuilder } from '@microsoft/signalr';

export interface IBattleContext {
    battle: IBattleData;
    player: IEntity;
    server: ServerConnection;
}

export const BattleContext = createContext<IBattleContext>({
    battle: {
        board: {
            size: {
                height: 4,
                width: 6
            },
            entitiesPosition: []
        },
        entities: [],
        id: ''
    },
    player: {
        id: '',
        defensiveStats: {
            defenseAbsorption: 0
        },
        offensiveStats: {
            damage: 0
        },
        skills: [],
        state: {
            currentHealth: { x: 0, y: 0 },
            healthRadius: 0
        },
        weapon: {
            damageOnX: DamageDirection.Neutral,
            damageOnY: DamageDirection.Neutral,
            name: ''
        }
    },
    server: new ServerConnection(
        new HubConnectionBuilder().withUrl('http://localhost').build()
    )
});