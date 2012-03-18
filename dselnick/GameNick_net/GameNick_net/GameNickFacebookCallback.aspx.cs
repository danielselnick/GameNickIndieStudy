using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Facebook;

namespace GameNickService
{
    public partial class GameNickFacebookCallback : System.Web.UI.Page
    {
        private const string AppId = "212683568783078";
        private const string AppSecret = "5282002c07d5e6557ac5c816317e19ab";
        private const string _silverlightFacebookCallback = "http://gamenick.net/GameNickFacebookCallback.aspx";

        protected string AccessToken { get; private set; }
        protected string ErrorDescription { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            var url = HttpContext.Current.Request.Url;
            FacebookOAuthResult oauthResult;

            var oauthClient = new FacebookOAuthClient { AppId = AppId, AppSecret = AppSecret, RedirectUri = new Uri(_silverlightFacebookCallback) };

            if (oauthClient.TryParseResult(url, out oauthResult))
            {
                if (oauthResult.IsSuccess)
                {
                    var result = (IDictionary<string, object>)oauthClient.ExchangeCodeForAccessToken(oauthResult.Code);
                    AccessToken = result["access_token"].ToString();
                    Label1.Text = AccessToken;
                }
                else
                {
                    ErrorDescription = oauthResult.ErrorDescription;
                }

                Response.Cache.SetCacheability(HttpCacheability.NoCache);
            }
            else
            {
                Response.Redirect("~/");
            }
        }
    }
}