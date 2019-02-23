using System.Web;
using System.Web.Mvc;
using System.Web.Http;

namespace Alpha
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }

    // TODO règle d'authentification / Authorization pour les WebAPI
    public class APIAhthorizeAttribute : System.Web.Mvc.AuthorizeAttribute
    {        
        
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            
            throw new System.NotImplementedException();
        }
    }
}
