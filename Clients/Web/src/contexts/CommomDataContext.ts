import { createContext } from "react";
import { IBattleData, IBattleRequest, IUserConnected } from "../interfaces";

export interface ICommomDataContext {
    usersConnected: IUserConnected[];
    battleRequests: IBattleRequest[];
    battle?: IBattleData;
}

export const CommomDataContext = createContext<ICommomDataContext>({
    usersConnected: [],
    battleRequests: []
});