import React, { PropsWithChildren, useCallback, useContext, useEffect, useState } from "react";
import { CommomDataContext, ICommomDataContext } from "./CommomDataContext";
import { AuthContext } from "./AuthContext";
import { IAssetFileItem, IAssetsData, IAssetsFile, IUserConnected } from "../interfaces";
import { useNavigate } from "react-router-dom";
import { IServerEvents } from "../server";
import configs from "../configs";

function getAssetBitMap(assetImage: HTMLImageElement, item: IAssetFileItem) {
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
        }, []);

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
        }, []);

    useEffect(() => {
        fetch(`${configs.assetsUrl}/assets.map.json`)
        .then(async r => JSON.parse(await r.text()) as IAssetsFile)
        .then(map => {
            const assetsImage = new Image();
            assetsImage.onload = async () => {
                const convertingAssetsImageToBitMap: Promise<ImageBitmap>[] = [];
                const keysOfMap: Array<keyof IAssetsFile> = [];
                for (const key in map) {
                    const asset = map[key as keyof IAssetsFile];
                    keysOfMap.push(key as keyof IAssetsFile);
                    convertingAssetsImageToBitMap.push(getAssetBitMap(assetsImage, asset));
                }
                const result = await Promise.all(convertingAssetsImageToBitMap);
                const assetsWithImage = keysOfMap.reduce<Partial<IAssetsData>>(
                    (data, key, i) => {
                        data[key] = { image: result[i], ...map[key] }
                        return data;
                    }, 
                    {}
                );
                setAssetsData(assetsWithImage as IAssetsData);
            }
            assetsImage.src = `${configs.assetsUrl}/assets.png`;
        })
        .catch(console.error);
    }, []);

    useEffect(() => {
        if (battle !== undefined)
            navigate('/battle');
    }, [battle, navigate]);

    useEffect(() => {
        if (authContext.data !== undefined 
            && battle === undefined
            && window.location.pathname.indexOf("entity") === -1)
            navigate('/home');
    }, [authContext.data, battle, navigate]);

    useEffect(
        () => {
            console.log("effect do context provider", authContext.data);
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
            }
        }, 
        [
            authContext.data, 
            onListConnectedUsers, 
            onUserConnect, 
            onUserDisconnect, 
            onNewBattleRequest, 
            onBattleRequestSent, 
            onNewBattle,
            onBattleRequestCancelled,
            onBattleCancelled
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