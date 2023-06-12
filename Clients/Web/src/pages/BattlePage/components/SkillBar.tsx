import React, { useContext, useState } from "react";
import { BattleContext } from "../BattleContext";
import { ISkill } from "../../../interfaces";
import { IDefaultButtonProps, PrimaryButton } from "../../../components/Buttons";
import "./skillBar.css"

interface ISkillCardProps {
    data: ISkill,
    enabled: boolean,
    onClick: IDefaultButtonProps['onClick']
}

const SkillCard : React.FC<ISkillCardProps> = ({data: skill, enabled, onClick}) => {
    
    return(<PrimaryButton text={skill.name} enabled={enabled} onClick={onClick} />)
}

export interface ISkillBarProps {
    onSkillSelect: (skill: string) => any;
    selected?: string
}

const SkillBar: React.FC<ISkillBarProps> = ({
    selected, onSkillSelect
}) => {
    const { player } = useContext(BattleContext);

    return(
        <div className="skill-bar-container">
            {
                player.skills.map(s => 
                    (<SkillCard 
                        key={s.name} 
                        data={s} 
                        enabled={selected === s.name} 
                        onClick={(() => onSkillSelect(s.name))}
                    />)
                )
            }
        </div>);
}

export default SkillBar;
