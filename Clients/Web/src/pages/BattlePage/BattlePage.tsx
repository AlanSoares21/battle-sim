import React, { useContext } from "react";
import { AuthContext } from "../../contexts/AuthContext";
import { CommomDataContext } from "../../contexts/CommomDataContext";
import './index.css'
import { CancelButton } from "../../components/Buttons";
import { useNavigate } from "react-router-dom";
import LifeBar from "./components/LifeBar";
import { BattleContext } from "./BattleContext";
import Board from "./components/Board";

export const BattlePage: React.FC = () => {
    const navigate = useNavigate();

    const authContext = useContext(AuthContext);
    const commomData = useContext(CommomDataContext);

    return(<>
        {
            commomData.battle && authContext.data &&
            <BattleContext.Provider value={{
                battle: commomData.battle,
                server: authContext.data.server
            }}>
                <LifeBar />
                <Board cellSize={25} />
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
