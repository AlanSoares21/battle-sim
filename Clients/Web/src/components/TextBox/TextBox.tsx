
import React from "react";

export interface ITextBoxProps {
    placeholder: string;
    onChange?: (text: string) => any;
}

export const TextBox: React.FC<ITextBoxProps> = ({
    placeholder,
    onChange
}) => {
    return (<input
        type='text'
        placeholder={placeholder}
        onChange={ev => {
            if (onChange)
                onChange(ev.target.value);
        }}
    ></input>);
}