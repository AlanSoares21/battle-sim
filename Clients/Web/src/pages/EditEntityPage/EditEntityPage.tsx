import React, { useCallback, useEffect, useState } from "react";
import { getEntity, getEquips, updateEntity } from "../../server";
import { isApiError } from "../../typeCheck";
import { IEntity, IEquip } from "../../interfaces";
import MultilineTextBox from "../../components/MultilineTextBox/MultilineTextBox";
import { DefaultButton, PrimaryButton } from "../../components/Buttons";
import { useNavigate } from "react-router-dom";

const EditEntityPage: React.FC = () => {
    const navigate = useNavigate();
    const [entityData, setEntityData] = useState<IEntity>();
    const [equips, setEquips] = useState<IEquip[]>([]);

    const handleBtnUpdateEntityClick = useCallback(async () => {
        if (entityData === undefined)
            return alert('entity data is undefined');
        const response = await updateEntity(entityData);
        if (isApiError(response))
            return alert(`error on updating entity. Message: ${response.message}`);
        setEntityData(entityData);
        alert('Entity updated');
    }, [entityData]);

    const handleTextBoxChange = useCallback((text: string) => {
        try {
            const data = JSON.parse(text);
            setEntityData(data);
        }
        catch {
            alert('Your entity data is invalid');
        }
    }, []);
    
    useEffect(() => {
        getEntity().then(response => {
            if (isApiError(response)) {
                setEntityData(undefined);
                return alert(response.message);
            }
            setEntityData(response);
        })
        .finally(() => 
            getEquips().then(response => {
                if (isApiError(response)) {
                    setEquips([]);
                    return alert(response.message);
                }
                setEquips(response);
            })
        );
    }, []);

    return <div style={{paddingLeft: '1vw'}}>
        {
            entityData !== undefined ? 
            <div>
                <p>Name: {entityData.id}</p>
                <MultilineTextBox 
                    defaultValue={JSON.stringify(entityData, undefined, '\n\t')} 
                    onChange={handleTextBoxChange}
                />
                <PrimaryButton text="updateEntity" onClick={handleBtnUpdateEntityClick} />
            </div>
            :
            <div>Entity data undefined</div>
        }
        {
            equips.map(e => (<div key={e.id}>{e.id} - {e.shape} - {e.effect}</div>))
        }
        <DefaultButton text="<- back to home" onClick={() => navigate('/home')} />
    </div>
};

export default EditEntityPage;