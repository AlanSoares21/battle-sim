import React from "react";
import "./index.css"

export interface IDropdownOption {
    text: string;
    value: string;
}

export interface IDropdownProps {
    label?: string;
    options: IDropdownOption[];
    optionSelected?: IDropdownOption['value'];
    onSelect: (option: IDropdownOption) => void;
};


const Dropdown: React.FC<IDropdownProps> = ({
    options, onSelect, label, optionSelected
}) => {

    return (<label className="dropdown-label">
        {label}
        <select className="dropdown" onSelect={console.log} defaultValue={optionSelected}>
            {options.map((opt, i) => (
                <option key={`opt-${i}-${opt.value}`} value={opt.value}>
                    {opt.text}
                </option>
            ))}
        </select>
    </label>);
}

export default Dropdown;