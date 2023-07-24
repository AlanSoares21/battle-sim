import React, { useCallback, useContext, useState } from "react";
import { PrimaryButton } from "../../components/Buttons";
import { TextBox } from "../../components/TextBox";
import { AuthContext } from "../../contexts/AuthContext";
import { login } from "../../server";
import { isLoginResponse } from "../../typeCheck";

export const LoginPage: React.FC = () => {
    const authContext = useContext(AuthContext);
    const [isCheckingName, setIsCheckingName] = useState(false);
    const [username, setUsername] = useState<string>();

    const handleLogin = useCallback(async () => {
        if (username === undefined || username.length === 0)
            return alert(`Por favor informe um username.`);
        setIsCheckingName(true);
        const response = await login(username);
        setIsCheckingName(false);
        if (!isLoginResponse(response)) 
            return alert(`Erro ao validar nome. Erro: ${response.message}`);
        authContext.setToken(response.accessToken, response.refreshToken, username);
    }, [username, authContext]);

    return (
        <form onSubmit={(ev) => { ev.preventDefault(); handleLogin(); }}>
            {isCheckingName && <label>Checking the username...</label>}
            <fieldset disabled={isCheckingName}>
                <TextBox placeholder="Username..." onChange={setUsername} />
                <PrimaryButton text="Logar" onClick={handleLogin} />
            </fieldset>
        </form>
    );
}