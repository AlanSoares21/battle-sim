import React, { useContext, useEffect, useState } from "react";
import { getEntity, getEquips } from "../../server";
import { isApiError } from "../../typeCheck";
import { IEntity, IEquip } from "../../interfaces";
import { TextBox } from "../../components/TextBox";
import MultilineTextBox from "../../components/MultilineTextBox/MultilineTextBox";

const EditEntityPage: React.FC = () => {
    
    const [entityData, setEntityData] = useState<IEntity>();
    const [equips, setEquips] = useState<IEquip[]>([]);
    
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
                <MultilineTextBox defaultValue={JSON.stringify(entityData, undefined, '\n\t')} />
            </div>
            :
            <div>Entity data undefined</div>
        }
        {
            equips.map(e => (<div key={e.id}>{e.id} - {e.shape} - {e.effect}</div>))
        }
    </div>
};

export default EditEntityPage;