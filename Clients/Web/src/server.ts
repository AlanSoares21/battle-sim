import { HubConnection } from "@microsoft/signalr";
import configs from "./configs";
import { IApiError, IBattleData, IBattleRequest, ICheckNameResponse, IUserConnected, TCoordinates } from "./interfaces";

export async function login(name: string): 
    Promise<ICheckNameResponse | IApiError> {
    const response = await fetch(`${configs.serverApiUrl}/Login`, {
        method: 'POST',
        body: JSON.stringify({ name }),
        headers: [
            ["Content-Type", "application/json"]
        ]
    })
    .then(r => r.blob())
    .then(r => r.text());

    return JSON.parse(response) as ICheckNameResponse | IApiError;
} 

export interface IHubServer {
    ListUsers(): void;
    SendBattleRequest(targetUser: string): void;
    AcceptBattle(requestId: string): void;
    Move(x: number, y: number): void;
    CancelBattleRequest(requesterId: string): void;
    CancelBattle(battleId: string): void;
    Attack(targetId: string): void;
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
}