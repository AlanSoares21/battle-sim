import React from "react";
import './index.css'

export interface INumericInputProps {
    defaultValue?: number;
    value?: number;
    onChange:(value?: number) => void;
}

export const NumericInput: React.FC<INumericInputProps> = ({ value, defaultValue, onChange }) => {

    return (<input 
        className="numeric-input" 
        type="number" 
        value={value}
        defaultValue={defaultValue}
        onChange={ev => {
            onChange(parseInt(ev.target.value));
        }}
    ></input>);
};
