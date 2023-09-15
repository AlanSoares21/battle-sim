import { createContext } from "react";
import { DamageDirection, TGameAssets, IBattleData, IEntity } from "../../interfaces";
import { ServerConnection } from "../../server";
import { HubConnectionBuilder } from '@microsoft/signalr';

export interface IBattleContext {
    battle: IBattleData;
    player: IEntity;
    server: ServerConnection;
    assets: TGameAssets;
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
        defenseAbsorption: 0,
        damage: 0,
        skills: [],
        healthRadius: 0,
        maxMana: 0,
        weapon: {
            damageOnX: DamageDirection.Neutral,
            damageOnY: DamageDirection.Neutral,
            name: ''
        },
        equips: []
    },
    server: new ServerConnection(
        new HubConnectionBuilder().withUrl('http://localhost').build()
    ),
    assets: {}
});