@ModelType LoginViewModel
@Code
    ViewBag.Title = "Se connecter"
End Code
<%@ Register TagPrefix="WQ" Namespace="WebQuiz" Assembly="WebQuiz" %>

<h2>@ViewBag.Title.</h2>
<div class="row">
    <div class="col-md-8">
        <WQ:Questionnaire id="test" runat="server">
        </WQ:Questionnaire>
        <section id="loginForm">
            @Using Html.BeginForm("Login", "Account", New With {.ReturnUrl = ViewBag.ReturnUrl}, FormMethod.Post, New With {.class = "form-horizontal", .role = "form"})
                @Html.AntiForgeryToken()
                @<text>
                    <h4>Utilisez un compte local pour vous connecter.</h4>
                    <hr />
                    @Html.ValidationSummary(True, "", New With {.class = "text-danger"})
                    <div class="form-group">
                        @Html.LabelFor(Function(m) m.Email, New With {.class = "col-md-2 control-label"})
                        <div class="col-md-10">
                            @Html.TextBoxFor(Function(m) m.Email, New With {.class = "form-control"})
                            @Html.ValidationMessageFor(Function(m) m.Email, "", New With {.class = "text-danger"})
                        </div>
                    </div>
                    <div class="form-group">
                        @Html.LabelFor(Function(m) m.Password, New With {.class = "col-md-2 control-label"})
                        <div class="col-md-10">
                            @Html.PasswordFor(Function(m) m.Password, New With {.class = "form-control"})
                            @Html.ValidationMessageFor(Function(m) m.Password, "", New With {.class = "text-danger"})
                        </div>
                    </div>
                    <div class="form-group">
                        <div class="col-md-offset-2 col-md-10">
                            <div class="checkbox">
                                @Html.CheckBoxFor(Function(m) m.RememberMe)
                                @Html.LabelFor(Function(m) m.RememberMe)
                            </div>
                        </div>
                    </div>
                    <div class="form-group">
                        <div class="col-md-offset-2 col-md-10">
                            <input type="submit" value="Se connecter" class="btn btn-default" />
                        </div>
                    </div>
                    <p>
                        @Html.ActionLink("Inscrire comme nouvel utilisateur", "Register")
                    </p>
                    @* Activez ceci une fois que vous avez activé la confirmation du compte pour la fonctionnalité de réinitialisation du mot de passe
                    <p>
                        @Html.ActionLink("Vous avez oublié votre mot de passe ?", "ForgotPassword")
                    </p>*@
                </text>
            End Using
        </section>
    </div>
    <div class="col-md-4">
        <section id="socialLoginForm">
            @Html.Partial("_ExternalLoginsListPartial", New ExternalLoginListViewModel With {.ReturnUrl = ViewBag.ReturnUrl})
        </section>
    </div>
</div>
@Section Scripts
    @Scripts.Render("~/bundles/jqueryval")
End Section
