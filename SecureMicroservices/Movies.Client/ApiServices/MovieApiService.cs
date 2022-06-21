using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Movies.Client.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Movies.Client.ApiServices
{
    public class MovieApiService : IMovieApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MovieApiService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public Task<Movie> CreateMovie(Movie movie)
        {
            throw new System.NotImplementedException();
        }

        public Task DeleteMovie(string id)
        {
            throw new System.NotImplementedException();
        }

        public Task<Movie> GetMovie(int id)
        {
            throw new System.NotImplementedException();
        }

        public async Task<IEnumerable<Movie>> GetMovies()
        {
            //1-Get Token from Identity Server
            //2-Send Rquest to Protected API
            //3-Deserialize Object to MovieList

            ////WAY 1:
            var httpClient = _httpClientFactory.CreateClient("MovieAPIClient");
            var request = new HttpRequestMessage(HttpMethod.Get, "/movies");

            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<Movie>>(content);


            ////WAY 2:
            //1. "retrieve" our api credentials. This must be registered on Identity Server!
            //var apiClientCredentials = new ClientCredentialsTokenRequest
            //{
            //    Address = "https://localhost:5005/connect/token",
            //    ClientId = "movieClient",
            //    ClientSecret = "secret",
            //    //This is the scope out Protected API requires
            //    Scope = "movieAPI"
            //};

            ////creates a new HttpClient to talk to out IdentityServer (localhost:5005)
            //var client = new HttpClient();

            ////just checks if we can reach the Discovery document. Not 100% needed but...
            //var disco = await client.GetDiscoveryDocumentAsync("https://localhost:5005");
            //if (disco.IsError)
            //{
            //    return null; // throw 500 error
            //}

            ////2. Authenticates and get an access token from Identity Server
            //var tokenResponse = await client.RequestClientCredentialsTokenAsync(apiClientCredentials);
            //if (tokenResponse.IsError)
            //{
            //    return null;
            //}

            ////Another HttpClient for talking now with out Protected API
            //var apiClient = new HttpClient();

            ////3. Set the access_token in the request Authorization: Bearer <token>
            //apiClient.SetBearerToken(tokenResponse.AccessToken);


            ////4. Send a request to out Protected API
            //var response = await apiClient.GetAsync("https://localhost:5001/api/movies");
            //response.EnsureSuccessStatusCode();

            //var content = await response.Content.ReadAsStringAsync();

            ////Deserialize Object
            //return JsonConvert.DeserializeObject<List<Movie>>(content);
        }

        public async Task<UserInfoViewModel> GetUserInfo()
        {
            var idpClient = _httpClientFactory.CreateClient("IDPClient");

            var metaDataResponse = await idpClient.GetDiscoveryDocumentAsync();

            if (metaDataResponse.IsError)
            {
                throw new HttpRequestException("Something went wrong while requesting the access token");
            }

            var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);

            var userInfoResponse = await idpClient.GetUserInfoAsync(
                new UserInfoRequest
                {
                    Address = metaDataResponse.UserInfoEndpoint,
                    Token = accessToken
                });

            if (userInfoResponse.IsError)
            {
                throw new HttpRequestException("Something went wrong while getting user info");
            }

            var userInfoDictionary = new Dictionary<string, string>();

            foreach(var claim in userInfoResponse.Claims)
            {
                userInfoDictionary.Add(claim.Type, claim.Value);
            }

            return new UserInfoViewModel(userInfoDictionary);
        }

        public Task<Movie> UpdateMovie(Movie movie)
        {
            throw new System.NotImplementedException();
        }
    }
}
