import { createContext } from "react";
import { IAssetsFile, IBattleData, IBattleRequest, IUserConnected } from "../interfaces";

export interface ICommomDataContext {
    usersConnected: IUserConnected[];
    battleRequests: IBattleRequest[];
    battle?: IBattleData;
    assets?: {
        map: IAssetsFile,
        file: CanvasImageSource
    }
}

export const CommomDataContext = createContext<ICommomDataContext>({
    usersConnected: [],
    battleRequests: []
});