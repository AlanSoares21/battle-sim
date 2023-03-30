import React, { useContext, useEffect, useMemo, useState } from "react";
import CanvasWrapper from "../../../CanvasWrapper";
import { BattleContext } from "../BattleContext";
import LifeBarRender from "./LifeBarRender";

export interface ILifeBarProps { }

const LifeBar: React.FC<ILifeBarProps> = () => {
    const { battle: { entities }} = useContext(BattleContext);
    
    const [canvasRef, setCanvasRef] = useState<HTMLCanvasElement | null>(null);
    
    const canvasWrapper = useMemo<CanvasWrapper | undefined>(() => {
        if (!canvasRef) {
            console.error("canvas ref ta nulo");
            return;
        }
        const context = canvasRef.getContext('2d');
        if (!context) {
            console.error("contexto ta nulo");
            return;
        }
        return new CanvasWrapper(context)
    }, [ canvasRef ]);

    useEffect(() => {
        if (canvasWrapper !== undefined) {
            const render = new LifeBarRender(canvasWrapper);
            for (const entity of entities)
                render.setEntity(entity);
            render.render();
        }
    }, [entities, canvasWrapper]);

    return (<div>
        <canvas 
            style={{border: "1px solid #f1f1f1"}}
            width={500}
            height={125}
            ref={setCanvasRef}>
        </canvas>
        {
            entities.reduce((v, e) => 
                `${v}${e.id}: ${e.state.healthRadius} - (${e.state.currentHealth.x},${e.state.currentHealth.y}) | `
                , '| ')
        }
    </div>);
}

export default LifeBar;