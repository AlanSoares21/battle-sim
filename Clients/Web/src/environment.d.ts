declare global {
    namespace NodeJS {
        interface ProcessEnv {
            REACT_APP_ServerApiUrl?: string;
            REACT_APP_ServerWsUrl?: string;
        }
    }
}

export {};