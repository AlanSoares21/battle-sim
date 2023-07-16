import React, { useContext } from "react";
import { BattleContext } from "../BattleContext";
import { IDefaultButtonProps, PrimaryButton } from "../../../components/Buttons";
import "./skillBar.css"

interface ISkillCardProps {
    data: string,
    enabled: boolean,
    onClick: IDefaultButtonProps['onClick']
}

const SkillCard : React.FC<ISkillCardProps> = ({data: skill, enabled, onClick}) => {
    
    return(<PrimaryButton text={skill} enabled={enabled} onClick={onClick} />)
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
                        key={s} 
                        data={s} 
                        enabled={selected === s} 
                        onClick={(() => onSkillSelect(s))}
                    />)
                )
            }
        </div>);
}

export default SkillBar;
