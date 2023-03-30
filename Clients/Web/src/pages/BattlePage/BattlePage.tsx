import React, { useCallback, useContext, useEffect, useMemo, useRef } from "react";
import BoardCanvas from "../../BoardCanvas";
import BoardRenderController from "../../BoardRenderController";
import CanvasWrapper from "../../CanvasWrapper";
import { AuthContext } from "../../contexts/AuthContext";
import { CommomDataContext } from "../../contexts/CommomDataContext";
import { TBoard, TCanvasCoordinates } from "../../interfaces";
import './index.css'
import { CancelButton } from "../../components/Buttons";
import { useNavigate } from "react-router-dom";
import LifeBar from "./components/LifeBar";
import { BattleContext } from "./BattleContext";
import Board from "./components/Board";

const cellSize = 100;

export const BattlePage: React.FC = () => {
    const navigate = useNavigate();

    const authContext = useContext(AuthContext);
    const commomData = useContext(CommomDataContext);
    const canvasRef = useRef<HTMLCanvasElement>(null);

    const renderController= useMemo<BoardRenderController | undefined>(() => {
        const canvas = canvasRef.current;
        if (!canvas) {
            console.log("canvas ta nulo");
            return;
        }
        const context = canvas.getContext('2d');
        if (!context) {
            console.log("contexto ta nulo")
            return;
        }
        const canvasWrapper = new CanvasWrapper(context)
        const board: TBoard = {
            height: 6,
            width: 6
        };
        const boardCanvas = new BoardCanvas(board, canvasWrapper, cellSize);
        const value = new BoardRenderController(board, boardCanvas)
        value.render();
        return value;
    }, [ canvasRef ]);

    const onEntityMove = useCallback((entityIdentifier: string, x: number, y: number) => {
        if (renderController === undefined)
            return console.log("Render controller is undefined on entity move");
        renderController.placePlayer({x, y}, { name: entityIdentifier });
        renderController.render();
    }, [renderController]);


    useEffect(() => {
        if (authContext.data) {
            authContext.data.server.onEntityMove(onEntityMove);
        }
    }, [authContext.data, onEntityMove])

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
