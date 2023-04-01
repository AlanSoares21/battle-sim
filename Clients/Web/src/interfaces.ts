export type TCoordinates = { x: number, y: number };

export type TBoardCoordinates = TCoordinates;

export type TCanvasCoordinates = TCoordinates;

export type TBoard = { width: number, height: number };

export interface IBoardColors {
    background: string;
    grid: string;
    pointer: string;
    player: {
        circle: string;
        name: string;
    }
}

export type TRenderElementFunction = (element: TRenderElement) => any;

export type RenderElementsHandlers = {
    'pointer': TRenderElementFunction,
    'player': TRenderElementFunction
}

export type TRenderElement = { cell: TBoardCoordinates, type: keyof RenderElementsHandlers, data?: any };

export interface IPlayerRenderData {
    name: string;
}

export interface IApiError {
    message: string;
}

export interface ICheckNameResponse {
    token: string;
}

export interface IUserConnected {
    name: string;
    isOnBattle: boolean;
    challengedByYou: boolean;
}

export interface IBattleRequest {
    requester: string;
    requestId: string;
    target: string;
}

export interface IEntityPosition
{
    x: number;
    y: number;
    entityIdentifier: string;
}

export interface IBoardData {
    size: {
        width: number;
        height: number;
    }
    entitiesPosition: IEntityPosition[];
}

export enum DamageDirection {
    Neutral = 0,
    Positive = 1,
    Negative = 2
}

export interface IWeapon {
    name: string;
    damageOnX: DamageDirection;
    damageOnY: DamageDirection;
}

// TODO:: this will be implemented in the future
export interface IEntity {
    id: string;
    skills: any[];
    weapon: IWeapon;
    state: {
        currentHealth: TCoordinates;
        healthRadius: number;
    };
    offensiveStats: {
        damage: number;
    };
    defensiveStats: {
        defenseAbsorption: number;
    }
}

export interface IBattleData {
    id: string;
    board: IBoardData;
    entities: IEntity[];
}
