import React, { useCallback, useContext, useEffect, useMemo, useState } from "react";
import CanvasWrapper from "../../../CanvasWrapper";
import { BattleContext } from "../BattleContext";
import LifeBarRender from "./LifeBarRender";
import { IServerEvents } from "../../../server";
import { TCoordinates } from "../../../interfaces";

export interface ILifeBarProps { }

const onAttack = (render: LifeBarRender): IServerEvents['Attack'] => 
(
    (_, target, currentHealth) => {
        render.setEntityCurrentHealth(target, currentHealth);
        render.render();
    }
)

const onSkill = (render: LifeBarRender): IServerEvents['Skill'] => 
(
    (_, source, target, currentHealth) => {
        render.setEntityCurrentHealth(target, currentHealth);
        render.render();
    }
)

const LifeBar: React.FC<ILifeBarProps> = () => {
    const { battle: { entities }, server } = useContext(BattleContext);
    
    const [ playersLife, setPlayerLife ] = useState<{[key: string]: TCoordinates}>({});

    const UpdatePlayersLifeOnAttack = useCallback<IServerEvents['Attack']>((_, target, currentHealth) => {
        setPlayerLife(p => {
            p[target] = currentHealth;
            return p;
        });
    }, []);

    const UpdatePlayersLifeOnSkill = useCallback<IServerEvents['Skill']>((_, source, target, currentHealth) => {
        setPlayerLife(p => {
            p[target] = currentHealth;
            return p;
        });
    }, []);

    const [canvasRef, setCanvasRef] = useState<HTMLCanvasElement | null>(null);
    
    const render = useMemo<LifeBarRender | undefined>(() => {
        if (!canvasRef) {
            console.error("canvas ref ta nulo");
            return;
        }
        const context = canvasRef.getContext('2d');
        if (!context) {
            console.error("contexto ta nulo");
            return;
        }
        return new LifeBarRender(new CanvasWrapper(context));
    }, [ canvasRef ]);

    useEffect(() => {
        if (render !== undefined) {
            for (const entity of entities)
                render.setEntity({...entity});
            render.render();
        }
    }, [entities, render]);

    useEffect(() => {
        if (render !== undefined)
            server
                .onAttack(UpdatePlayersLifeOnAttack) 
                .onAttack(onAttack(render))
                .onSkill(UpdatePlayersLifeOnSkill)
                .onSkill(onSkill(render));
    }, [ server, render ]);

    return (<div>
        <canvas 
            style={{border: "1px solid #f1f1f1"}}
            width={500}
            height={125}
            ref={setCanvasRef}>
        </canvas>
        {
            Object.keys(playersLife)
                .map((k, i) => (<b key={i}>`${k}: (${playersLife[k].x}, ${playersLife[k].y})`</b>))
        }
    </div>);
}

export default LifeBar;