import { IApiError, ILoginResponse } from "./interfaces";

export function isLoginResponse(value: any): value is ILoginResponse {
    return typeof value["accessToken"] === "string" && typeof value["refreshToken"] === "string";
}

export function isApiError(value: any): value is IApiError {
    return typeof value["message"] === "string" && Object.keys(value).length === 1;
}