import { createContext } from "react";
import { ServerConnection } from "../server";

export interface IAuthContext {
    setToken: (accessToken: string, refreshToken: string, userId: string) => any;
    data?: {
        username: string;
        token: string;
        server: ServerConnection; 
    }
}

export const AuthContext = createContext<IAuthContext>({
    setToken: () => {}
});