import React, { useCallback, useContext, useEffect, useRef, useState } from "react";
import BoardCanvas from "../../BoardCanvas";
import BoardRenderController from "../../BoardRenderController";
import CanvasWrapper from "../../CanvasWrapper";
import { AuthContext } from "../../contexts/AuthContext";
import { CommomDataContext } from "../../contexts/CommomDataContext";
import { TBoard, TCanvasCoordinates } from "../../interfaces";
import { convertCanvasToBoardCoordinates } from "../../utils";
import './index.css'
import { CancelButton } from "../../components/Buttons";
import { useNavigate } from "react-router-dom";
import LifeBar from "./components/LifeBar";

const cellSize = 100;

export const BattlePage: React.FC = () => {
    const navigate = useNavigate();

    const authContext = useContext(AuthContext);
    const commomData = useContext(CommomDataContext);
    const canvasRef = useRef<HTMLCanvasElement>(null);

    const [renderController, setRenderController] = useState<BoardRenderController>();

    const onEntityMove = useCallback((entityIdentifier: string, x: number, y: number) => {
        if (renderController === undefined)
            return console.log("Render controller is undefined on entity move");
        renderController.placePlayer({x, y}, { name: entityIdentifier });
        renderController.render();
    }, [renderController]);
    
    const handleCanvasClick = useCallback<React.MouseEventHandler<HTMLCanvasElement>>(ev => {
        const canvasCoordinates: TCanvasCoordinates = { x: ev.clientX, y: ev.clientY };
        const boardCoordinates = convertCanvasToBoardCoordinates(canvasCoordinates, cellSize);
        if (!renderController)
            return console.error(`board canvas não foi preenchido`);
        renderController.placePointerAndRender(boardCoordinates);
        console.log({boardCoordinates});
        if (authContext.data !== undefined)
            authContext.data.server.Move(boardCoordinates.x, boardCoordinates.y);
    }, [renderController, authContext.data]);

    useEffect(() => {
        if (commomData.battle === undefined) 
            return;
        const canvas = canvasRef.current;
        if (!canvas) {
            return console.log("canvas ta nulo");
        }
        const context = canvas.getContext('2d');
        if (!context)
            return console.log("contexto ta nulo");
        const canvasWrapper = new CanvasWrapper(context)
        const board: TBoard = {
            height: commomData.battle.board.height,
            width: commomData.battle.board.width
        };
        const boardCanvas = new BoardCanvas(board, canvasWrapper, cellSize);
        const render = new BoardRenderController(board, boardCanvas);
        for (const player of commomData.battle.board.entitiesPosition) {
            render.placePlayer({ x: player.x, y: player.y }, { name: player.entityIdentifier });
        }
        render.render();
        setRenderController(render);
    }, [commomData.battle]);

    useEffect(() => {
        if (authContext.data) {
            authContext.data.server.onEntityMove(onEntityMove);
        }
    }, [authContext.data, onEntityMove])

    return(<>
            {
                commomData.battle !== undefined &&
                (<LifeBar entities={commomData.battle.entities} />)
            }
        {
            commomData.battle !== undefined &&
            (
                <canvas 
                    ref={canvasRef} 
                    width={commomData.battle.board.width * cellSize} 
                    height={commomData.battle.board.height * cellSize} 
                    style={{border: "1px solid #f1f1f1"}}
                    onClick={handleCanvasClick}
                    >
                </canvas>
            )
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
