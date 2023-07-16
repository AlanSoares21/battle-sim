import { HubConnection } from "@microsoft/signalr";
import configs from "./configs";
import { IApiError, IBattleData, IBattleRequest, IEntity, IEquip, ILoginResponse, IUserConnected, TCoordinates } from "./interfaces";
import { isLoginResponse } from "./typeCheck";

export async function login(name: string): 
Promise<ILoginResponse | IApiError> {
    const response = await fetch(`${configs.serverApiUrl}/Auth/Login`, {
        method: 'POST',
        body: JSON.stringify({ name }),
        headers: [
            ["Content-Type", "application/json"]
        ]
    })
    .then(r => r.blob())
    .then(r => r.text());
    return JSON.parse(response) as ILoginResponse | IApiError;
}

const REFRESH_TOKEN_KEY = "refreshToken";
const ACCESS_TOKEN_KEY = "accessToken";
let ACCESS_TOKEN_HEADER = "Authorization";
let ACCESS_TOKEN = "";

export async function setTokens(accessToken: string, refreshToken: string) {
    localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
    localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
    ACCESS_TOKEN = `Bearer ${accessToken}`;
}

export function removeTokens() {
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(ACCESS_TOKEN_KEY);
}

async function refreshTokens() {
    const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
    if (!refreshToken)
        return { message: 'Refresh token on storage not found' } as IApiError;
    
    const accessToken = localStorage.getItem(ACCESS_TOKEN_KEY);
    if (!accessToken)
        return { message: 'Access token on storage not found' } as IApiError;
    
    removeTokens();

    const body: ILoginResponse = {
        accessToken,
        refreshToken,
    }

    const response = await fetch(`${configs.serverApiUrl}/Auth/Refresh`, {
        method: 'POST',
        body: JSON.stringify(body),
        headers: [
            ["Content-Type", "application/json"]
        ]
    })
    .then(r => r.text());
    return JSON.parse(response) as ILoginResponse | IApiError;
}

async function requestApi(route: string, method?: 'GET' | 'PUT', body?: any, dontHandleUnauthError?: boolean): Promise<{
    text: string;
    status: number
}> {
    if (method === undefined)
        method = 'GET';
    let bodyString = undefined;
    if (body)
        bodyString = JSON.stringify(body);

    const fetchResult = await fetch(`${configs.serverApiUrl}${route}`, {
        method,
        body: bodyString,
        headers: [
            ["Content-Type", "application/json"],
            [ACCESS_TOKEN_HEADER, ACCESS_TOKEN]
        ]
    })
    .catch(e => console.log("request server error", e));
    
    if (!dontHandleUnauthError && (!fetchResult || fetchResult.status === 401)) {
        const newTokens = await refreshTokens();
        if (isLoginResponse(newTokens)) {
            setTokens(newTokens.accessToken, newTokens.refreshToken)
            return requestApi(route, method, body, true);
        }
    }

    if (!fetchResult)
        return { status: 400, text: JSON.stringify({ message: 'Error on trying to request ' + route } as IApiError) };
    
    return {
        text: await fetchResult.text(),
        status: fetchResult.status
    }
}

export async function getEntity(): Promise<IEntity | IApiError> {
    const response = await requestApi(`/Entity`);
    return JSON.parse(response.text) as IApiError | IEntity;
}

export async function getEquips(): Promise<IEquip[] | IApiError> {
    const response = await requestApi(`/Equip`);
    return JSON.parse(response.text) as IApiError | IEquip[];
}

export async function updateEntity(data: IEntity): Promise<IEntity | IApiError> {
    const response = await requestApi(`/Entity`, 'PUT', data);
    return JSON.parse(response.text) as IApiError | IEntity;
}

export interface IHubServer {
    ListUsers(): void;
    SendBattleRequest(targetUser: string): void;
    AcceptBattle(requestId: string): void;
    Move(x: number, y: number): void;
    CancelBattleRequest(requesterId: string): void;
    CancelBattle(battleId: string): void;
    Attack(targetId: string): void;
    Skill(skillName: string, targetId: string): void;
}

// events sent by the server
export interface IServerEvents {
    ListConnectedUsers(users: IUserConnected[]): void;
    UserConnect(user: IUserConnected): void;
    UserDisconnect(user: IUserConnected): void;
    NewBattleRequest(request: IBattleRequest): void;
    BattleRequestSent(request: IBattleRequest): void;
    NewBattle(battleData: IBattleData): void;
    EntityMove(entity: string, x: number, y: number): void;
    BattleRequestCancelled(cancellerId: string, request: IBattleRequest): void;
    BattleCancelled(cancellerId: string, battleId: string): void;
    Attack(source: string, target: string, currentHealth: TCoordinates): void;
    Skill(
        skillName: string, 
        source: string,
        target: string, 
        currentHealth: TCoordinates
    ): void;
}

export class ServerConnection implements IHubServer
{
    conn: HubConnection;
    constructor(conn: HubConnection) {
        this.conn = conn;
    }

    onListConnectedUsers(listener: IServerEvents['ListConnectedUsers']) {
        this.conn.on('ListConnectedUsers', listener);
        return this;
    }
    onUserConnect(listener: IServerEvents['UserConnect']) {
        this.conn.on('UserConnect', listener);
        return this;
    }
    onUserDisconnect(listener: IServerEvents['UserDisconnect']) {
        this.conn.on('UserDisconnect', listener);
        return this;
    }
    onNewBattleRequest(listener: IServerEvents['NewBattleRequest']) {
        this.conn.on('NewBattleRequest', listener);
        return this;
    }
    onBattleRequestSent(listener: IServerEvents['BattleRequestSent']) {
        this.conn.on('BattleRequestSent', listener);
        return this;
    }
    onNewBattle(listener: IServerEvents['NewBattle']) {
        this.conn.on('NewBattle', listener);
        return this;
    }
    onEntityMove(listener: IServerEvents['EntityMove']) {
        this.conn.on('EntityMove', listener);
        return this;
    }
    onBattleRequestCancelled(listener: IServerEvents['BattleRequestCancelled']) {
        this.conn.on('BattleRequestCancelled', listener);
        return this;
    }
    onBattleCancelled(listener: IServerEvents['BattleCancelled']) {
        this.conn.on('BattleCancelled', listener);
        return this;
    }
    onAttack(listener: IServerEvents['Attack']) {
        this.conn.on('Attack', listener);
        return this;
    }
    onSkill(listener: IServerEvents['Skill']) {
        this.conn.on('Skill', listener);
        return this;
    }
    
    ListUsers(): void {
        this.conn.send('ListUsers');
    }
    SendBattleRequest(targetUser: string): void {
        this.conn.send('SendBattleRequest', targetUser);
    }
    AcceptBattle(requestId: string): void {
        this.conn.send('AcceptBattle', requestId);
    }
    Move(x: number, y: number) {
        this.conn.send('Move', x, y);
    };
    CancelBattleRequest(requesterId: string): void {
        this.conn.send('CancelBattleRequest', requesterId);
    }
    CancelBattle(battleId: string) {
        this.conn.send('CancelBattle', battleId);
    }
    Attack(targetId: string) {
        this.conn.send('Attack', targetId);
    }
    Skill(skillName: string, targetId: string): void {
        console.log("usando skill", skillName)
        this.conn.send('Skill', skillName, targetId);
    }
}