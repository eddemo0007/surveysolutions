﻿using System.Threading.Tasks;
using Refit;

namespace WB.Core.BoundedContexts.Headquarters.DataExport
{
    public interface IOAuth2Api
    {
        [Post("/token")]
        Task<ExternalStorageTokenResponse> GetTokensByAuthorizationCodeAsync([Body(BodySerializationMethod.UrlEncoded)] ExternalStorageAccessTokenRequest request);
        [Post("/token")]
        Task<ExternalStorageTokenResponse> GetAccessTokenByRefreshTokenAsync([Body(BodySerializationMethod.UrlEncoded)] ExternalStorageRefreshTokenRequest request);
    }
    
    public class ExternalStorageAccessTokenRequest
    {
        [AliasAs("code")]
        public string Code { get; set; }
        [AliasAs("client_id")]
        public string ClientId { get; set; }
        [AliasAs("client_secret")]
        public string ClientSecret { get; set; }
        [AliasAs("redirect_uri")]
        public string RedirectUri { get; set; }
        [AliasAs("grant_type")]
        public string GrantType { get; set; }
    }
    
    public class ExternalStorageRefreshTokenRequest
    {
        [AliasAs("refresh_token")]
        public string RefreshToken { get; set; }
        [AliasAs("client_id")]
        public string ClientId { get; set; }
        [AliasAs("client_secret")]
        public string ClientSecret { get; set; }
        [AliasAs("grant_type")]
        public string GrantType { get; set; }
    }

    public class ExternalStorageTokenResponse
    {
        [AliasAs("access_token")]
        public string AccessToken { get; set; }
        [AliasAs("expires_in")]
        public int ExpiresIn { get; set; }
        [AliasAs("token_type")]
        public string TokenType { get; set; }
        [AliasAs("scope")]
        public string Scope { get; set; }
        [AliasAs("refresh_token")]
        public string RefreshToken { get; set; }
    }
}
