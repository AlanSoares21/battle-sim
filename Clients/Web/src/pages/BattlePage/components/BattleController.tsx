import React, { useCallback, useContext, useState } from "react";
import LifeBar from "./LifeBar";
import { BattleContext } from "../BattleContext";
import Board, { IBoardProps } from "./Board";
import SkillBar from "./SkillBar";

export const BattleController: React.FC = () => {
    const { battle, server} = useContext(BattleContext);

    const [skillSelected, setSkillSelected] = useState<string>();

    const handleBoardClick = useCallback<IBoardProps['onBoardClick']>(
        (cell) => {
            const index = battle.board.entitiesPosition.findIndex(p =>
                p.x === cell.x && p.y === cell.y);
            if (index === -1) {
                server.Move(cell.x, cell.y)
                return;
            }
            const target = battle.board.entitiesPosition[index].entityIdentifier;
            if (skillSelected !== undefined) {
                server.Skill(skillSelected, target);    
                setSkillSelected(s => s === skillSelected ? undefined : s);
            }
            else
                server.Attack(target);
        }, 
    [battle, server, skillSelected]);

    return(<>
        <LifeBar />
        <Board onBoardClick={handleBoardClick} cellSize={25} />
        <SkillBar selected={skillSelected} onSkillSelect={setSkillSelected} />
    </>);
}
