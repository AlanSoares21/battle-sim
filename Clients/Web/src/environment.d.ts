declare global {
    namespace NodeJS {
        interface ProcessEnv {
            REACT_APP_ServerApiUrl?: string;
            REACT_APP_ServerWsUrl?: string;
            PUBLIC_URL: string;
            REACT_APP_AssetsUrl: string;
        }
    }
}

export {};