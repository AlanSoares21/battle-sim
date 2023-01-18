import React, { useCallback, useContext } from 'react';
import { AuthContext } from '../../contexts/AuthContext';
import { CommomDataContext } from '../../contexts/CommomDataContext';
import { IBattleRequest } from '../../interfaces';
import { PrimaryButton, IDefaultButtonProps, CancelButton } from '../../components/Buttons';
import './index.css';

export interface IBattleRequestCardProps {
    request: IBattleRequest;
    onAccept: IDefaultButtonProps['onClick'];
    onReject: IDefaultButtonProps['onClick'];
}

export const BattleRequestCard : React.FC<IBattleRequestCardProps> = ({
    request, onAccept, onReject
}) => {

    return (<div className='battle-request-card'>
        <i>{request.requester}</i>
        <PrimaryButton text='Accpet' onClick={onAccept} />
        <CancelButton text='Reject' onClick={onReject} />
    </div>);
}

export const BattleRequestPage: React.FC = (
) => {
    const authContext = useContext(AuthContext);
    const { battleRequests } = useContext(CommomDataContext);

    const handleOnAccept = useCallback((request: IBattleRequest) => {
        if (authContext.data) {
            authContext.data.server.AcceptBattle(request.requestId);
        }
    }, [authContext.data]);

    const handleOnReject = useCallback((request: IBattleRequest) => {
        if (authContext.data) {
            authContext.data.server.CancelBattleRequest(request.requestId);
        }
    }, [authContext.data]);

    return(
        <div>
            Battle requests
            <div className='battle-request-list'>
                {
                    battleRequests.map(request => 
                        (<BattleRequestCard 
                            key={request.requestId} 
                            request={request} 
                            onAccept={() => handleOnAccept(request)}
                            onReject={() => handleOnReject(request)}
                        />)
                    )
                }
            </div>
        </div>
    );
};
