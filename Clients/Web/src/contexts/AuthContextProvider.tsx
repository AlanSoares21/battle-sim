import React, { PropsWithChildren, useCallback, useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import * as signalR from '@microsoft/signalr';
import { AuthContext, IAuthContext } from "./AuthContext";
import configs from "../configs";
import { ServerConnection, setTokens } from "../server";

export const AuthContextProvider: React.FC<PropsWithChildren> = ({ 
    children 
}) => {
    const navigate = useNavigate();
    
    const [isValidatingUser, setIsValidatingUser] = useState(false);
    const [data, setData] = useState<IAuthContext['data']>(undefined);

    const onSetToken = useCallback<IAuthContext['setToken']>(async (token, refreshToken, userId) => {
        setTokens(token, refreshToken);
        setIsValidatingUser(true);
        const conn = new signalR.HubConnectionBuilder()
            .withUrl(configs.serverWsUrl, { 
                skipNegotiation: true, 
                transport: signalR.HttpTransportType.WebSockets, 
                accessTokenFactory: () => token})
            .build();
        await conn.start()
            .then(() => {
                conn.onclose(() => {
                    setData(undefined);
                    alert('Connection closed with server');
                    navigate('/');
                })
                setData({
                    token,
                    username: userId,
                    server: new ServerConnection(conn)
                });
                navigate('/home');
            })
            .catch(() => alert('Error in establishing connection with server.'))
            .finally(() => setIsValidatingUser(false))
    }, [setIsValidatingUser, setData, navigate]);
    
    useEffect(() => {
        if (window.location.pathname !== '/' && data === undefined)
            window.location.href = window.location.origin;
    }, [data]);

    return <AuthContext.Provider value={{ setToken: onSetToken, data }}>
        {
            isValidatingUser && <div>Validating token...</div>
        }
        <div style={{display: undefined}}>
            {children}
        </div>
    </AuthContext.Provider>
}