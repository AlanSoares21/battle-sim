import React from "react";
import './index.css'

export interface INumericInputProps {
    max?: number;
    min?: number;
    defaultValue?: number;
    value?: number;
    onChange:(value?: number) => void;
}

export const NumericInput: React.FC<INumericInputProps> = ({ 
    value, defaultValue, onChange, max, min
}) => {

    return (<input 
        max={max}
        min={min}
        className="numeric-input" 
        type="number" 
        value={value}
        defaultValue={defaultValue}
        onChange={ev => {
            onChange(parseInt(ev.target.value));
        }}
    ></input>);
};
