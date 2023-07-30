import React, { useCallback, useEffect, useState } from "react";
import { getEntity, updateEntity } from "../../server";
import { isApiError } from "../../typeCheck";
import { EquipEffect, EquipShape, IEntity, IEquip } from "../../interfaces";
import { CancelButton, DefaultButton, PrimaryButton } from "../../components/Buttons";
import { useNavigate } from "react-router-dom";
import Dropdown, { IDropdownOption } from "../../components/Dropdown/Dropdown";
import { NumericInput } from "../../components/NumericInput";
import "./index.css"

interface IEditEquipProps {
    data: IEquip;
    index: number;
    onUpdate: (equip: IEquip, index: number) => void;
    onRemove: (index: number) => void;
}

const shapeOptions: IDropdownOption[] = [
    { text: "Rectangle", value: `${EquipShape.Rectangle}` } 
];

const effectOptions: IDropdownOption[] = [
    { text: "Barrier", value: `${EquipEffect.Barrier}` }
];

const EditEquip: React.FC<IEditEquipProps> = ({
    data, index, onRemove, onUpdate
}) => {

    const handleAddCoordinate = useCallback(() => {
        onUpdate({ ...data, coordinates: [ ...data.coordinates, { x: 0, y: 0 } ] }, index)
    }, [data, index, onUpdate]);

    const handleRemoveCoordinate = useCallback((coordIndex: number) => {
        const coordinates = data.coordinates.filter((c,i) => i !== coordIndex); 
        onUpdate({...data, coordinates }, index);
    }, [data, index, onUpdate]);
    
    return(<div className="edit-equip-container">
        <CancelButton text="-" onClick={() => onRemove(index)} />
        <label>Equip {index} </label>
        <div className="equip-properties-container">
            <div className="equip-coordinates-container" >
                <table>
                    <thead>
                        <tr>
                            <td>
                                Coordinates
                            </td>
                        </tr>
                    </thead>
                    <thead>
                        <tr>
                            <th>X</th>
                            <th>Y</th>
                            <th></th>
                        </tr>
                    </thead>
                    <tbody>
                        {data.coordinates.map((c, i) => (<tr key={`coord-${i}`}>
                            <td><NumericInput 
                                value={c.x} 
                                onChange={value => {
                                    const coordinates = [...data.coordinates];
                                    coordinates[i].x = value ? value : 0;
                                    onUpdate({...data, coordinates }, index);
                                }} 
                            /></td>
                            <td><NumericInput 
                                value={c.y} 
                                onChange={value => {
                                    const coordinates = [...data.coordinates];
                                    coordinates[i].y = value ? value : 0;
                                    onUpdate({...data, coordinates }, index);
                                }} 
                            /></td>
                            <td><CancelButton 
                                text="-" 
                                onClick={() => handleRemoveCoordinate(i)} 
                            /></td>
                        </tr>))}
                    </tbody>
                </table>
                <PrimaryButton text="+" onClick={handleAddCoordinate} />
            </div>
            <div>
                <Dropdown label="Effect" optionSelected={`${data.effect}`} options={effectOptions} onSelect={console.log} />
                <Dropdown label="Shape" optionSelected={`${data.shape}`} options={shapeOptions} onSelect={console.log} />
            </div>
        </div>
    </div>);
}

const EditEntityPage: React.FC = () => {
    const navigate = useNavigate();
    const [entityData, setEntityData] = useState<IEntity>();
    const [equipList, setEquipList] = useState<IEquip[]>([]);

    const handleBtnUpdateEntityClick = useCallback(async () => {
        if (entityData === undefined)
            return alert('entity data is undefined');
        const data  = {...entityData};
        data.equips = equipList;
        const response = await updateEntity(data);
        if (isApiError(response))
            return alert(`error on updating entity. Message: ${response.message}`);
        setEntityData(response);
        setEquipList(response.equips);
        alert('Entity updated');
    }, [entityData, equipList]);

    const handleBtnNewEquip = useCallback(() => {
        setEquipList(e => ([
            ...e, 
            { coordinates: [
                { x: 0, y: 0},
                { x: 0, y: 0},
                { x: 0, y: 0}
            ], effect: EquipEffect.Barrier, shape: EquipShape.Rectangle }
        ]));
    }, [setEquipList]);

    const handleEquipUpdate = useCallback<IEditEquipProps['onUpdate']>((equip, index) => {
        setEquipList(list => list.map((e, i) => {
            if (i === index)
                return equip;
            return e;
        }))
    }, [setEquipList]);

    const handleRemoveEquip = useCallback<IEditEquipProps['onRemove']>(index => {
        setEquipList(list => list.filter((e, i) => i !== index))
    }, [setEquipList]);
    
    useEffect(() => {
        getEntity().then(response => {
            if (isApiError(response)) {
                setEntityData(undefined);
                return alert(response.message);
            }
            setEntityData(response);
            setEquipList(response.equips);
        });
    }, []);

    return <div style={{paddingLeft: '1vw'}}>
        {
            entityData !== undefined ? 
            <form onSubmit={ev => { ev.preventDefault(); handleBtnUpdateEntityClick(); }}>
                <fieldset>
                    <div className="equips-list">
                        {
                            equipList.map((e, i) => (<EditEquip 
                                index={i} 
                                onRemove={handleRemoveEquip}
                                onUpdate={handleEquipUpdate} 
                                key={`equip-${i}`} 
                                data={e} 
                            />))
                        }
                    </div>
                    <div style={{ display: 'flex' }}>
                        <PrimaryButton text="Add Equip" onClick={handleBtnNewEquip} />
                        <PrimaryButton text="Update Entity" onClick={handleBtnUpdateEntityClick} />
                    </div>
                </fieldset>
            </form>
            :
            <div>Error on getting entity data</div>
        }
        <DefaultButton text="<- back to home" onClick={() => navigate('/home')} />
    </div>
};

export default EditEntityPage;