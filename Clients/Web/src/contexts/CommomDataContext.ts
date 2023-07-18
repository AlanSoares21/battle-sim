import { createContext } from "react";
import { IAssetsData, IBattleData, IBattleRequest, IUserConnected } from "../interfaces";

export interface ICommomDataContext {
    usersConnected: IUserConnected[];
    battleRequests: IBattleRequest[];
    battle?: IBattleData;
    assets?: {
        map: IAssetsData,
        file: ImageBitmap
    }
}

export const CommomDataContext = createContext<ICommomDataContext>({
    usersConnected: [],
    battleRequests: []
});