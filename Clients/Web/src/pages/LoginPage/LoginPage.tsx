import React, { useCallback, useContext, useState } from "react";
import { PrimaryButton } from "../../components/Buttons";
import { TextBox } from "../../components/TextBox";
import { AuthContext } from "../../contexts/AuthContext";
import { login } from "../../server";
import { isLoginResponse } from "../../typeCheck";
import './index.css'

export const LoginPage: React.FC = () => {
    const authContext = useContext(AuthContext);
    const [isCheckingName, setIsCheckingName] = useState(false);
    const [username, setUsername] = useState<string>();

    const handleBtnLoginClick = useCallback(async () => {
        if (username === undefined || username.length === 0)
            return alert(`Por favor informe um username.`);
        setIsCheckingName(true);
        const response = await login(username);
        setIsCheckingName(false);
        if (!isLoginResponse(response)) 
            return alert(`Erro ao validar nome. Erro: ${response.message}`);
        authContext.setToken(response.accessToken, response.refreshToken, username);
    }, [username, authContext]);

    return (<div className="padding-around stack-vertical child-margin10">
        {
            isCheckingName ?
            <div>Checking the name {username}</div>
            :
            <>
                <TextBox placeholder="Username..." onChange={setUsername} />
                <PrimaryButton text="Logar" onClick={handleBtnLoginClick} />
            </>
        }
    </div>);
}