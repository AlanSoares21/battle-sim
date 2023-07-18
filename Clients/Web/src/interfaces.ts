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

export interface IWeapon {
    name: string;
    damageOnX: DamageDirection;
    damageOnY: DamageDirection;
}

export interface IEntityEquip {
    entityId: string;
    equipId: string;
    coordinates: TCoordinates[];
}

export interface IEntity {
    id: string;
    skills: string[];
    weapon: IWeapon;
    healthRadius: number;
    damage: number;
    defenseAbsorption: number;
    equips: IEntityEquip[];
}

export interface IBattleData {
    id: string;
    board: IBoardData;
    entities: IEntity[];
}

export enum EquipEffect {
    Barrier
}

export enum EquipShape {
    Rectangle
}

export interface IEquip {
    id: string;
    effect: EquipEffect;
    shape: EquipShape
}

export interface IAssetItem {
    start: TCoordinates;
    size: TSize;
}

export interface IAssetItemWithImage extends IAssetItem {
    image: ImageBitmap;
}

export interface IAssetsFile {
    "board-background": IAssetItem;
    "enemy": IAssetItem;
    "player": IAssetItem;
    "unknowed-skill": IAssetItem;
}

export interface IAssetsData {
    "board-background": IAssetItemWithImage;
    "enemy": IAssetItemWithImage;
    "player": IAssetItemWithImage;
    "unknowed-skill": IAssetItemWithImage;
}

export interface IAsset {
    image: ImageBitmap;
    size: TSize;
    start: TCoordinates;
}