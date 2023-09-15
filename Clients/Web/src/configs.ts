const configs = {
    serverApiUrl: process.env.REACT_APP_ServerApiUrl || 'http://localhost:5136',
    serverWsUrl: process.env.REACT_APP_ServerWsUrl || 'ws://localhost:5136/hubs/game',
    env: process.env.NODE_ENV,
    assetsUrl: process.env.REACT_APP_AssetsUrl || 'http://localhost/assets'
}

export default configs;