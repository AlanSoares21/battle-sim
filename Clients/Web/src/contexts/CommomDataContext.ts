import { createContext } from "react";
import { TGameAssets, IBattleData, IBattleRequest, IUserConnected } from "../interfaces";

export interface ICommomDataContext {
    usersConnected: IUserConnected[];
    battleRequests: IBattleRequest[];
    battle?: IBattleData;
    assets: TGameAssets
}

export const CommomDataContext = createContext<ICommomDataContext>({
    usersConnected: [],
    battleRequests: [],
    assets: {}
});