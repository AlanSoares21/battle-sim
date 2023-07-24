import React, { useContext } from "react"
import { AuthContext } from "../../contexts/AuthContext";
import { BattleRequestPage } from "../BattleRequestsPage";
import { UsersListPage } from "../UsersListPage";
import { PrimaryButton } from "../../components/Buttons";
import { useNavigate } from "react-router-dom";

export const HomePage : React.FC = () => {
    const authContext = useContext(AuthContext);
    const navigate = useNavigate();

    return(
        <div>
            <PrimaryButton text="Edit Entity" onClick={() => {
                navigate('/entity');
            }} />
            <label>{authContext.data?.username}</label>
            <hr />
            <BattleRequestPage />
            <hr />
            <UsersListPage />
        </div>
    );
}