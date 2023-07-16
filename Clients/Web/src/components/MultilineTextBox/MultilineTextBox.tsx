import React from "react";
import './index.css';

export interface IMultilineTextBoxProps {
    defaultValue?: string
}

const MultilineTextBox: React.FC<IMultilineTextBoxProps> = ({
    defaultValue
}) => {

    return <textarea className="multiline-text-box" defaultValue={defaultValue} >

    </textarea>
}

export default MultilineTextBox;