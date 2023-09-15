export type TCoordinates = { x: number, y: number };

export type TBoardCoordinates = TCoordinates;

export type TCanvasCoordinates = TCoordinates;

export type TSize = { width: number, height: number };
export type TBoard = TSize;
export type TCanvasSize = TSize;

export interface IPlayerRenderData {
    name: string;
}

export interface IApiError {
    message: string;
}

export interface ILoginResponse {
    accessToken: string;
    refreshToken: string;
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

export interface ISkillData {
    cost: number;
    range: number;
}

export interface IWeapon {
    name: string;
    damageOnX: DamageDirection;
    damageOnY: DamageDirection;
}

export enum EquipEffect {
    Barrier
}

export enum EquipShape {
    Rectangle
}

export interface IEquip {
    coordinates: TCoordinates[];
    effect: EquipEffect;
    shape: EquipShape
}

export interface IEntity {
    id: string;
    skills: string[];
    weapon: IWeapon;
    healthRadius: number;
    damage: number;
    defenseAbsorption: number;
    equips: IEquip[];
    maxMana: number;
}

export interface IBattleData {
    id: string;
    board: IBoardData;
    entities: IEntity[];
}

export interface IAssetFileItem {
    start: TCoordinates;
    size: TSize;
}

export type TAssetsNames = 
    "board-background" 
    | "enemy" 
    | "player" 
    |"unknowed-skill"
    | "barrier-equip-pattern"
    | "base-damage-x-negative"
    | "base-damage-x-positive"
    | "life-sphere"
    | "life-pointer"
    | "base-damage-y-negative"
    | "base-damage-y-positive"
    | "mana-bar-border"
    | "mana-bar-background"
    | "mana-bar-fill";

export type TAssetsMapFile = {
    [assetName in TAssetsNames] : IAssetFileItem;
}

export interface IAsset {
    image: ImageBitmap;
    size: TSize;
    start: TCoordinates;
}

export type TGameAssets = {
    [assetName in TAssetsNames]?: IAsset;
}

