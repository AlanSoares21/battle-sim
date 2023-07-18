import React, { PropsWithChildren, useCallback, useContext, useEffect, useState } from "react";
import { CommomDataContext, ICommomDataContext } from "./CommomDataContext";
import { AuthContext } from "./AuthContext";
import { IAssetItem, IAssetsFile, IUserConnected } from "../interfaces";
import { useNavigate } from "react-router-dom";
import { IServerEvents } from "../server";

function getAssetBitMap(assetImage: HTMLImageElement, item: IAssetItem) {
    return createImageBitmap(
        assetImage, 
        item.start.x, item.start.y, 
        item.size.width, item.size.height
    );
}

export const CommomDataContextProvider: React.FC<PropsWithChildren> = ({ 
    children 
}) => {
    const [assetsData, setAssetsData] = useState<ICommomDataContext['assets']>();

    const authContext = useContext(AuthContext);
    const [usersConnected, setUsersConnected] = useState<ICommomDataContext['usersConnected']>([]);
    const [battleRequests, setBattleRequests] = useState<ICommomDataContext['battleRequests']>([]);
    const [battle, setBattle] = useState<ICommomDataContext['battle']>();

    const navigate = useNavigate();

    const onListConnectedUsers = useCallback<IServerEvents['ListConnectedUsers']>(
        async users => {
            console.log({users});
            setUsersConnected(users);
        }, [setUsersConnected]);

    const onUserConnect = useCallback<IServerEvents['UserConnect']>(
        async newUser => { 
            setUsersConnected(users => ([...users, newUser]));
        }, [setUsersConnected]);

    const onUserDisconnect = useCallback<IServerEvents['UserDisconnect']>(
        async (userDisconnected: IUserConnected) => {
            setBattleRequests(requests => (requests.filter(request => request.requester !== userDisconnected.name)));
            setUsersConnected(users => (users.filter(user => user.name !== userDisconnected.name)));
        }, [setUsersConnected, setBattleRequests]);

    const onNewBattleRequest = useCallback<IServerEvents['NewBattleRequest']>(
        async request => {
            console.log({'new_battle_request': request})
            setBattleRequests(requests => ([...requests, request]));
        }, [setBattleRequests]);

    const onBattleRequestSent = useCallback<IServerEvents['BattleRequestSent']>(
        async request => {
            console.log({'battle_request_sent': request})
            setUsersConnected(users => users.map(user => {
                if (user.name === request.target)
                    user.challengedByYou = true;
                return user;
            }));
        }, [setUsersConnected]);

    const onNewBattle = useCallback<IServerEvents['NewBattle']>(
        async (battle) => {
            console.log({'new_battle': battle });
            const entities = battle.board.entitiesPosition.map(e => e.entityIdentifier);
            setBattleRequests(requests => 
                    requests.filter(req => 
                        !entities.some(e => e === req.requester)));
            setUsersConnected(users => users.map(setChangeChallendByYouToFalse(entities)))
            setBattle(battle);
            navigate('/battle');
        }, [navigate]);

    const onBattleRequestCancelled = useCallback<IServerEvents['BattleRequestCancelled']>(
        async (cancelledBy, request) => {
            console.log({'request_cancelled': { cancelledBy, request }});
            setBattleRequests(requests => requests
                .filter(req => req.requestId !== request.requestId));
            setUsersConnected(users => 
                users.map(
                    setChangeChallendByYouToFalse([request.requester, request.target])));
        }, []);
        
    const onBattleCancelled = useCallback<IServerEvents['BattleCancelled']>(
        async (cancelledBy, battleId) => {
            console.log({'battle_cancelled': { cancelledBy, battleId }});
            setBattle(undefined);
            navigate('/home');
        }, [navigate]);

    const onAttack = useCallback<IServerEvents['Attack']>(
        async (source, target, currentHealth) => {
            console.log('new-attack', {
                source,
                target,
                currentHealth
            });
        }, []);

    const onSkill = useCallback<IServerEvents['Skill']>(
        async (skillName, source, target, currentHealth) => {
            console.log('skill', {
                skillName,
                source,
                target,
                currentHealth
        });
    }, []);

    const updateEntitiesPosition = useCallback<IServerEvents['EntityMove']>(
        (entity, x, y) => {
            setBattle(b => (b && {
                ...b, 
                board: { 
                    ...b.board, 
                    entitiesPosition: b.board.entitiesPosition.map(value => {
                        if (entity === value.entityIdentifier) {
                            value.y = y;
                            value.x = x;
                        }
                        return value;
                    })
                }
            }));
        }
    , []);

    useEffect(() => {
        fetch(`${process.env.PUBLIC_URL}/assets/assets.map.json`)
        .then(async r => JSON.parse(await r.text()) as IAssetsFile)
        .then(map => {
            const assetsImage = new Image();
            assetsImage.onload = async () => {
                const result = await Promise.all([
                    createImageBitmap(assetsImage),
                    getAssetBitMap(assetsImage, map['board-background']),
                    getAssetBitMap(assetsImage, map['enemy']),
                    getAssetBitMap(assetsImage, map['player']),
                    getAssetBitMap(assetsImage, map['unknowed-skill'])
                ]);
                const data: ICommomDataContext['assets'] = {
                    file: result[0],
                    map: {
                        'board-background': { image: result[1], ...map['board-background'] },
                        'enemy': { image: result[2], ...map['enemy'] },
                        'player': { image: result[3], ...map['player'] },
                        'unknowed-skill': { image: result[4], ...map['unknowed-skill'] }
                    }
                }
                setAssetsData(data);
            }
            assetsImage.src = `${process.env.PUBLIC_URL}/assets/assets.png`;
        });
        
    }, []);

    useEffect(
        () => {
            if (authContext.data) {
                authContext.data.server
                    .onListConnectedUsers(onListConnectedUsers)
                    .onUserConnect(onUserConnect)
                    .onUserDisconnect(onUserDisconnect)
                    .onNewBattleRequest(onNewBattleRequest)
                    .onBattleRequestSent(onBattleRequestSent)
                    .onNewBattle(onNewBattle)
                    .onBattleRequestCancelled(onBattleRequestCancelled)
                    .onBattleCancelled(onBattleCancelled)
                    .onAttack(onAttack)
                    .onEntityMove(updateEntitiesPosition)
                    .onSkill(onSkill);
            }
        }, 
        [
            authContext, 
            onListConnectedUsers, 
            onUserConnect, 
            onUserDisconnect, 
            onNewBattleRequest, 
            onBattleRequestSent, 
            onNewBattle,
            onBattleRequestCancelled,
            onBattleCancelled,
            onAttack,
            updateEntitiesPosition,
            onSkill
        ]
    )

    return <CommomDataContext.Provider value={{ 
        usersConnected, 
        battleRequests, 
        battle,
        assets: assetsData
    }}>
        {children}
    </CommomDataContext.Provider>
}

function setChangeChallendByYouToFalse(targetIds: string[]) {
    return (user: IUserConnected) => {
        if (targetIds.some(id => id === user.name))
            user.challengedByYou = false;
        return user;
    };
}