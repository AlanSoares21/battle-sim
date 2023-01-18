import React, { useContext } from "react"
import { AuthContext } from "../../contexts/AuthContext";
import { BattleRequestPage } from "../BattleRequestsPage";
import { UsersListPage } from "../UsersListPage";

export const HomePage : React.FC = () => {
    const authContext = useContext(AuthContext);

    return(
        <div>
            <div>Your name: {authContext.data?.username}</div>
            <hr />
            <BattleRequestPage />
            <hr />
            <UsersListPage />
        </div>
    );
}