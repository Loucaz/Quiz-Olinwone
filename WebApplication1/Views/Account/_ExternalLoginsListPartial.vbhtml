@ModelType ExternalLoginListViewModel
@Imports Microsoft.Owin.Security
@Code
    Dim loginProviders = Context.GetOwinContext().Authentication.GetExternalAuthenticationTypes()
End Code
<h4>Connectez-vous à l'aide d'un autre service.</h4>
<hr />
@If loginProviders.Count() = 0 Then
    @<div>
          <p>
              Aucun service d'authentification externe n'est configuré. Consultez <a href="https://go.microsoft.com/fwlink/?LinkId=403804">cet article</a>
              pour plus d'informations sur la configuration de cette application ASP.NET et la prise en charge des connexions via des services externes.
          </p>
    </div>
Else
    @Using Html.BeginForm("ExternalLogin", "Account", New With {.ReturnUrl = Model.ReturnUrl}, FormMethod.Post, New With {.class = "form-horizontal", .role = "form"})
        @Html.AntiForgeryToken()
        @<div id="socialLoginList">
           <p>
               @For Each p As AuthenticationDescription In loginProviders
                   @<button type="submit" class="btn btn-default" id="@p.AuthenticationType" name="provider" value="@p.AuthenticationType" title="Connexion avec votre compte @p.Caption">@p.AuthenticationType</button>
               Next
           </p>
        </div>
    End Using
End If
