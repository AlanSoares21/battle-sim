import React, { useContext, useEffect, useState } from "react";
import { AuthContext } from "../../contexts/AuthContext";
import { CommomDataContext } from "../../contexts/CommomDataContext";
import './index.css'
import { CancelButton } from "../../components/Buttons";
import { useNavigate } from "react-router-dom";
import { BattleContext } from "./BattleContext";
import { IEntity } from "../../interfaces";
import { BattleCanvas } from "./components/BattleCanvas";

export const BattlePage: React.FC = () => {
    const navigate = useNavigate();

    const authContext = useContext(AuthContext);
    const commomData = useContext(CommomDataContext);

    const [player, setPlayer] = useState<IEntity>();

    useEffect(() => {
        if (player !== undefined)
            return;
        if (commomData.battle === undefined)
            return;
        if (authContext.data === undefined)
            return;
        const username = authContext.data.username;
        setPlayer(commomData.battle.entities.find(p => p.id === username));
    }, [commomData, authContext, player]);

    return(<>
        {
            player && commomData.battle && authContext.data && commomData.assets &&
            <BattleContext.Provider value={{
                battle: commomData.battle,
                server: authContext.data.server,
                player,
                assets: commomData.assets
            }}>
                <BattleCanvas />
            </BattleContext.Provider>
        }
        <CancelButton 
            text="Cancel Battle" 
            onClick={() => {
                if (authContext.data === undefined) {
                    alert('Auth data is undefined, you can not cancel the battle. redirecting to login');
                    navigate('/');
                    return;
                }
                if (commomData.battle === undefined) {
                    alert('Battle is undefined, you can not cancel the battle. redirecting to home');
                    navigate('/home');
                    return;
                }
                authContext.data.server.CancelBattle(commomData.battle?.id);
            }} />
    </>);
}
