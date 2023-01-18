import { createContext } from "react";
import { IBattleRequest, IBoardData, IUserConnected } from "../interfaces";

export interface ICommomDataContext {
    usersConnected: IUserConnected[];
    battleRequests: IBattleRequest[];
    battle?: {
        id: string;
        boardData: IBoardData;
    }
}

export const CommomDataContext = createContext<ICommomDataContext>({
    usersConnected: [],
    battleRequests: []
});