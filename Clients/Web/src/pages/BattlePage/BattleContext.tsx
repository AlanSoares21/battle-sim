import { createContext } from "react";
import { DamageDirection, IAssetsFile, IBattleData, IEntity } from "../../interfaces";
import { ServerConnection } from "../../server";
import { HubConnectionBuilder } from '@microsoft/signalr';

export interface IBattleContext {
    battle: IBattleData;
    player: IEntity;
    server: ServerConnection;
    assets: {
        map: IAssetsFile,
        file?: ImageBitmap
    }
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
    assets: {
        map: {
            "board-background": {
                size: { width: 0, height: 0 },
                start: { x: 0, y: 0 }
            },
            "enemy": {
                size: { width: 0, height: 0 },
                start: { x: 0, y: 0 }
            },
            "player": {
                size: { width: 0, height: 0 },
                start: { x: 0, y: 0 }
            },
            "unknowed-skill": {
                size: { width: 0, height: 0 },
                start: { x: 0, y: 0 }
            }
        }
    }
});