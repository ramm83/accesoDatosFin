using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace AccesoDatos.Validator
{
    /// <summary>
    /// VAlidador de Token JWT
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="actionContext"></param>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.BaseAddress = new Uri("http://10.23.14.95:8995/Servicios/Autenticacion_JWT/api/authentication/authenticated");
                    var token = context.HttpContext.Request.Headers["Authorization"].ToString();
                    var tokenOrg = token.Substring("Bearer ".Length).Trim();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenOrg);
                    HttpResponseMessage result = client.GetAsync("").Result;
                    if (!result.IsSuccessStatusCode)
                    {
                        context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
                    }
                }
            }
            catch (Exception)
            {
                context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
            }
        }
    }
}