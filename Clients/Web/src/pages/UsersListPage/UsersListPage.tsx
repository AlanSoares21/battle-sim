import React, { useContext, useEffect } from "react";
import { useCallback } from "react";
import { useNavigate } from "react-router-dom";
import { DefaultButton, IDefaultButtonProps } from "../../components/Buttons";
import { AuthContext } from "../../contexts/AuthContext";
import { CommomDataContext } from "../../contexts/CommomDataContext";
import { IUserConnected } from "../../interfaces";
import './index.css'

interface IUserCardProps {
    user: IUserConnected;
    onClick: IDefaultButtonProps['onClick']
}

function selectUserCardButtonColor(user: IUserConnected): string {
    return user.challengedByYou ? "#747a1f" : "#236e1d";
}

const UserCard: React.FC<IUserCardProps> = ({
    user, onClick
}) => {

    return (<div className="user-card">
        <DefaultButton 
            text={user.name} 
            textColor="aliceblue"
            color={selectUserCardButtonColor(user)}
            onClick={onClick}
        />
    </div>);
}

export const UsersListPage: React.FC = () => {
    const navigate = useNavigate();
    const authContext = useContext(AuthContext);
    const commomData = useContext(CommomDataContext);

    const handleChallenge = useCallback((user: IUserConnected) => {
        if (authContext.data === undefined) 
            return alert('É necessário estabeler uma conexão para desafiar um jogador');
        authContext.data.server.SendBattleRequest(user.name);
        console.log({"battle_requested": { target: user }});
    }, [authContext.data]);
    
    useEffect(() => {
        if (authContext.data && commomData.usersConnected.length === 0) {
            authContext.data.server.ListUsers();
        }
    }, [authContext.data, commomData.usersConnected, navigate]);

    return(<>
        <div>{commomData.usersConnected.length} online users</div>
        <div className="user-card-container">
            {
                commomData.usersConnected.map((user, i) => (
                    <UserCard key={`user-${i}`} user={user} onClick={() => handleChallenge(user)} />
                ))
            }
        </div>
    </>
    );
}
