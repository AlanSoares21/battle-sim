import { createContext } from "react";
import { DamageDirection, IAssetsData, IBattleData, IEntity } from "../../interfaces";
import { ServerConnection } from "../../server";
import { HubConnectionBuilder } from '@microsoft/signalr';

export interface IBattleContext {
    battle: IBattleData;
    player: IEntity;
    server: ServerConnection;
    assets: IAssetsData
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
        },
        "barrier-equip-pattern": {
            size: { width: 0, height: 0 },
            start: { x: 0, y: 0 }
        },
        "base-damage-x-negative": {
            size: { width: 0, height: 0 },
            start: { x: 0, y: 0 }
        },
        "base-damage-x-positive": {
            size: { width: 0, height: 0 },
            start: { x: 0, y: 0 }
        },
        "life-sphere": {
            size: { width: 0, height: 0 },
            start: { x: 0, y: 0 }
        },
        "life-pointer": {
            size: { width: 0, height: 0 },
            start: { x: 0, y: 0 }
        },
        "base-damage-y-negative": {
            size: { width: 0, height: 0 },
            start: { x: 0, y: 0 }
        },
        "base-damage-y-positive": {
            size: { width: 0, height: 0 },
            start: { x: 0, y: 0 }
        }
    }
});