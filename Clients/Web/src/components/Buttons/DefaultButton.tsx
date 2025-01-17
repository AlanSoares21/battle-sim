import React, { CSSProperties } from "react";
import './index.css';

export interface IDefaultButtonProps {
    text: string;
    color?: CSSProperties['color'];
    enabled?: boolean;
    textColor?: CSSProperties['color'];
    className?: string;
    onClick?: () => any;
}

export const DefaultButton: React.FC<IDefaultButtonProps> = ({
    text, color, textColor, className, onClick, enabled
}) => {
    return (
        <button 
            className={className === undefined ? `btn-default` : `${className} btn-default`} 
            type='button' 
            style={{backgroundColor: color, color: textColor}}
            onClick={onClick}
            disabled={!!enabled}
        >
            {text}
        </button>
    );
}

export const PrimaryButton: React.FC<IDefaultButtonProps> = (props) => {
    return (<DefaultButton {...props} className='primary' />);
}

export const CancelButton: React.FC<IDefaultButtonProps> = (props) => {
    return (<DefaultButton {...props} className='cancel' />);
}