type TCoordinates = { x: number, y: number };

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
    width: number;
    height: number;
    entitiesPosition: IEntityPosition[]
}