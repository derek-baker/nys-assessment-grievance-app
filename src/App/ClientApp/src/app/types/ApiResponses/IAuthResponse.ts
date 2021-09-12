export interface IAuthResponse
{
    userName: string;
    sessionHash: string;
    authResult: {
        isAuthenticated: boolean
        authorization: {
            userName: string;
            userType: number;
        }
    };
}
