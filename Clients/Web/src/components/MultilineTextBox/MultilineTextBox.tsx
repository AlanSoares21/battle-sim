import React from "react";
import './index.css';

export interface IMultilineTextBoxProps {
    defaultValue?: string,
    onChange?: (text: string) => any;
}

const MultilineTextBox: React.FC<IMultilineTextBoxProps> = ({
    defaultValue,
    onChange
}) => {

    return <textarea 
        className="multiline-text-box" 
        defaultValue={defaultValue} 
        onChange={ev => {
            if (onChange)
                onChange(ev.target.value);
        }} 
    >

    </textarea>
}

export default MultilineTextBox;