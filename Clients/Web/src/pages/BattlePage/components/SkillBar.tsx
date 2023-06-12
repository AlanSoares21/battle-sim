import React, { useContext, useState } from "react";
import { BattleContext } from "../BattleContext";
import { ISkill } from "../../../interfaces";
import { DefaultButton } from "../../../components/Buttons";


interface ISkillCardProps {
    data: ISkill
}

const SkillCard : React.FC<ISkillCardProps> = ({data: skill}) => {
    
    return(<DefaultButton text={skill.name} className="primary" />)
}

export interface ISkillBarProps { }

const SkillBar: React.FC<ISkillBarProps> = () => {
    const { player } = useContext(BattleContext);

    return(<div>
        Name: {player.id}
        {player.skills.map(s => (<SkillCard data={s} />))}
    </div>);
}

export default SkillBar;
