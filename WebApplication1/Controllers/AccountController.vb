﻿Imports System.Globalization
Imports System.Security.Claims
Imports System.Threading.Tasks
Imports Microsoft.AspNet.Identity
Imports Microsoft.AspNet.Identity.Owin
Imports Microsoft.Owin.Security
Imports Owin

<Authorize>
Public Class AccountController
    Inherits Controller
    Private _signInManager As ApplicationSignInManager
    Private _userManager As ApplicationUserManager

    Public Sub New()
    End Sub

    Public Sub New(appUserMan As ApplicationUserManager, signInMan As ApplicationSignInManager)
        UserManager = appUserMan
        SignInManager = signInMan
    End Sub

    Public Property SignInManager() As ApplicationSignInManager
        Get
            Return If(_signInManager, HttpContext.GetOwinContext().[Get](Of ApplicationSignInManager)())
        End Get
        Private Set
            _signInManager = value
        End Set
    End Property

    Public Property UserManager() As ApplicationUserManager
        Get
            Return If(_userManager, HttpContext.GetOwinContext().GetUserManager(Of ApplicationUserManager)())
        End Get
        Private Set
            _userManager = value
        End Set
    End Property

    ' L'action Authorize est le point de terminaison qui est appelé quand vous accédez à
    ' une Web API protégée. S'il n'est pas connecté, l'utilisateur est alors redirigé vers 
    ' la page de Login. Une fois la connexion établie, vous pouvez appeler une Web API.
    <HttpGet>
    Public Function Authorize() As ActionResult
        Dim claims = New ClaimsPrincipal(User).Claims.ToArray()
        Dim identity = New ClaimsIdentity(claims, "Bearer")
        AuthenticationManager.SignIn(identity)
        Return New EmptyResult()
    End Function

    '
    ' GET: /Account/Login
    <AllowAnonymous>
    Public Function Login(returnUrl As String) As ActionResult
        ViewBag.ReturnUrl = returnUrl
        Return View()
    End Function

    '
    ' POST: /Account/Login
    <HttpPost>
    <AllowAnonymous>
    <ValidateAntiForgeryToken>
    Public Async Function Login(model As LoginViewModel, returnUrl As String) As Task(Of ActionResult)
        If Not ModelState.IsValid Then
            Return View(model)
        End If

        ' Ceci ne comptabilise pas les échecs de connexion pour le verrouillage du compte
        ' Pour que les échecs de mot de passe déclenchent le verrouillage du compte, utilisez shouldLockout := True
        Dim result = Await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout:=False)
        Select Case result
            Case SignInStatus.Success
                Return RedirectToLocal(returnUrl)
            Case SignInStatus.LockedOut
                Return View("Lockout")
            Case SignInStatus.RequiresVerification
                Return RedirectToAction("SendCode", New With {
                    returnUrl,
                    model.RememberMe
                })
            Case Else
                ModelState.AddModelError("", "Tentative de connexion non valide.")
                Return View(model)
        End Select
    End Function

    '
    ' GET: /Account/VerifyCode
    <AllowAnonymous>
    Public Async Function VerifyCode(provider As String, returnUrl As String, rememberMe As Boolean) As Task(Of ActionResult)
        ' Nécessiter que l'utilisateur soit déjà connecté via un nom d'utilisateur/mot de passe ou une connexte externe
        If Not Await SignInManager.HasBeenVerifiedAsync() Then
            Return View("Error")
        End If
        Return View(New VerifyCodeViewModel() With {
            .Provider = provider,
            .ReturnUrl = returnUrl,
            .RememberMe = rememberMe
        })
    End Function

    '
    ' POST: /Account/VerifyCode
    <HttpPost>
    <AllowAnonymous>
    <ValidateAntiForgeryToken>
    Public Async Function VerifyCode(model As VerifyCodeViewModel) As Task(Of ActionResult)
        If Not ModelState.IsValid Then
            Return View(model)
        End If

        ' Le code suivant protège des attaques par force brute contre les codes à 2 facteurs. 
        ' Si un utilisateur entre des codes incorrects pendant un certain intervalle, le compte de cet utilisateur 
        ' est alors verrouillé pendant une durée spécifiée. 
        ' Vous pouvez configurer les paramètres de verrouillage du compte dans IdentityConfig
        Dim result = Await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:=model.RememberMe, rememberBrowser:=model.RememberBrowser)
        Select Case result
            Case SignInStatus.Success
                Return RedirectToLocal(model.ReturnUrl)
            Case SignInStatus.LockedOut
                Return View("Lockout")
            Case Else
                ModelState.AddModelError("", "Code non valide.")
                Return View(model)
        End Select
    End Function

    '
    ' GET: /Account/Register
    <AllowAnonymous>
    Public Function Register() As ActionResult
        Return View()
    End Function

    '
    ' POST: /Account/Register
    <HttpPost>
    <AllowAnonymous>
    <ValidateAntiForgeryToken>
    Public Async Function Register(model As RegisterViewModel) As Task(Of ActionResult)
        If ModelState.IsValid Then
            Dim user = New ApplicationUser() With {
                .UserName = model.Email,
                .Email = model.Email,
                .Hometown = model.Hometown
            }
            Dim result = Await UserManager.CreateAsync(user, model.Password)
            If result.Succeeded Then
                Await SignInManager.SignInAsync(user, isPersistent:=False, rememberBrowser:=False)

                ' Pour plus d'informations sur l'activation de la confirmation de compte et de la réinitialisation de mot de passe, visitez https://go.microsoft.com/fwlink/?LinkID=320771
                ' Envoyer un e-mail avec ce lien
                ' Dim code = Await UserManager.GenerateEmailConfirmationTokenAsync(user.Id)
                ' Dim callbackUrl = Url.Action("ConfirmEmail", "Account", New With { .userId = user.Id, code }, protocol := Request.Url.Scheme)
                ' Await UserManager.SendEmailAsync(user.Id, "Confirmer votre compte", "Confirmez votre compte en cliquant <a href=""" & callbackUrl & """>ici</a>")

                Return RedirectToAction("Index", "Home")
            End If
            AddErrors(result)
        End If

        ' Si nous sommes arrivés là, quelque chose a échoué, réafficher le formulaire
        Return View(model)
    End Function

    '
    ' GET: /Account/ConfirmEmail
    <AllowAnonymous>
    Public Async Function ConfirmEmail(userId As String, code As String) As Task(Of ActionResult)
        If userId Is Nothing OrElse code Is Nothing Then
            Return View("Error")
        End If
        Dim result = Await UserManager.ConfirmEmailAsync(userId, code)
        Return View(If(result.Succeeded, "ConfirmEmail", "Error"))
    End Function

    '
    ' GET: /Account/ForgotPassword
    <AllowAnonymous>
    Public Function ForgotPassword() As ActionResult
        Return View()
    End Function

    '
    ' POST: /Account/ForgotPassword
    <HttpPost>
    <AllowAnonymous>
    <ValidateAntiForgeryToken>
    Public Async Function ForgotPassword(model As ForgotPasswordViewModel) As Task(Of ActionResult)
        If ModelState.IsValid Then
            Dim user = Await UserManager.FindByNameAsync(model.Email)
            If user Is Nothing OrElse Not (Await UserManager.IsEmailConfirmedAsync(user.Id)) Then
                ' Ne révélez pas que l'utilisateur n'existe pas ou qu'il n'est pas confirmé
                Return View("ForgotPasswordConfirmation")
            End If
            ' Pour plus d'informations sur l'activation de la confirmation de compte et de la réinitialisation de mot de passe, visitez https://go.microsoft.com/fwlink/?LinkID=320771
            ' Envoyer un e-mail avec ce lien
            ' Dim code = Await UserManager.GeneratePasswordResetTokenAsync(user.Id)
            ' Dim callbackUrl = Url.Action("ResetPassword", "Account", New With { .userId = user.Id, code }, protocol := Request.Url.Scheme)
            ' Await UserManager.SendEmailAsync(user.Id, "Réinitialiser le mot de passe", "Veuillez réinitialiser votre mot de passe en cliquant <a href=""" & callbackUrl & """>ici</a>")
            ' Return RedirectToAction("ForgotPasswordConfirmation", "Account")
        End If

        ' Si nous sommes arrivés là, quelque chose a échoué, réafficher le formulaire
        Return View(model)
    End Function

    '
    ' GET: /Account/ForgotPasswordConfirmation
    <AllowAnonymous>
    Public Function ForgotPasswordConfirmation() As ActionResult
        Return View()
    End Function

    '
    ' GET: /Account/ResetPassword
    <AllowAnonymous>
    Public Function ResetPassword(code As String) As ActionResult
        Return If(code Is Nothing, View("Error"), View())
    End Function

    '
    ' POST: /Account/ResetPassword
    <HttpPost>
    <AllowAnonymous>
    <ValidateAntiForgeryToken>
    Public Async Function ResetPassword(model As ResetPasswordViewModel) As Task(Of ActionResult)
        If Not ModelState.IsValid Then
            Return View(model)
        End If
        Dim user = Await UserManager.FindByNameAsync(model.Email)
        If user Is Nothing Then
            ' Ne révélez pas que l'utilisateur n'existe pas
            Return RedirectToAction("ResetPasswordConfirmation", "Account")
        End If
        Dim result = Await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password)
        If result.Succeeded Then
            Return RedirectToAction("ResetPasswordConfirmation", "Account")
        End If
        AddErrors(result)
        Return View()
    End Function

    '
    ' GET: /Account/ResetPasswordConfirmation
    <AllowAnonymous>
    Public Function ResetPasswordConfirmation() As ActionResult
        Return View()
    End Function

    '
    ' POST: /Account/ExternalLogin
    <HttpPost>
    <AllowAnonymous>
    <ValidateAntiForgeryToken>
    Public Function ExternalLogin(provider As String, returnUrl As String) As ActionResult
        ' Demander une redirection vers le fournisseur de connexion externe
        Return New ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", New With {
            returnUrl
        }))
    End Function

    '
    ' GET: /Account/SendCode
    <AllowAnonymous>
    Public Async Function SendCode(returnUrl As String, rememberMe As Boolean) As Task(Of ActionResult)
        Dim userId = Await SignInManager.GetVerifiedUserIdAsync()
        If userId Is Nothing Then
            Return View("Error")
        End If
        Dim userFactors = Await UserManager.GetValidTwoFactorProvidersAsync(userId)
        Dim factorOptions = userFactors.[Select](Function(purpose) New SelectListItem() With {
            .Text = purpose,
            .Value = purpose
        }).ToList()
        Return View(New SendCodeViewModel() With {
            .Providers = factorOptions,
            .ReturnUrl = returnUrl,
            .RememberMe = rememberMe
        })
    End Function

    '
    ' POST: /Account/SendCode
    <HttpPost>
    <AllowAnonymous>
    <ValidateAntiForgeryToken>
    Public Async Function SendCode(model As SendCodeViewModel) As Task(Of ActionResult)
        If Not ModelState.IsValid Then
            Return View()
        End If

        ' Générer le jeton et l'envoyer
        If Not Await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider) Then
            Return View("Error")
        End If
        Return RedirectToAction("VerifyCode", New With { _
            .Provider = model.SelectedProvider,
            model.ReturnUrl,
            model.RememberMe
        })
    End Function

    '
    ' GET: /Account/ExternalLoginCallback
    <AllowAnonymous>
    Public Async Function ExternalLoginCallback(returnUrl As String) As Task(Of ActionResult)
        Dim loginInfo = Await AuthenticationManager.GetExternalLoginInfoAsync()
        If loginInfo Is Nothing Then
            Return RedirectToAction("Login")
        End If

        ' Connecter cet utilisateur à ce fournisseur de connexion externe si l'utilisateur possède déjà une connexion
        Dim result = Await SignInManager.ExternalSignInAsync(loginInfo, isPersistent:=False)
        Select Case result
            Case SignInStatus.Success
                Return RedirectToLocal(returnUrl)
            Case SignInStatus.LockedOut
                Return View("Lockout")
            Case SignInStatus.RequiresVerification
                Return RedirectToAction("SendCode", New With {
                    returnUrl,
                    .RememberMe = False
                })
            Case Else
                ' Si l'utilisateur n'a pas de compte, invitez alors celui-ci à créer un compte
                ViewBag.ReturnUrl = returnUrl
                ViewBag.LoginProvider = loginInfo.Login.LoginProvider
                Return View("ExternalLoginConfirmation", New ExternalLoginConfirmationViewModel() With {
                    .Email = loginInfo.Email
                })
        End Select
    End Function

    '
    ' POST: /Account/ExternalLoginConfirmation
    <HttpPost>
    <AllowAnonymous>
    <ValidateAntiForgeryToken>
    Public Async Function ExternalLoginConfirmation(model As ExternalLoginConfirmationViewModel, returnUrl As String) As Task(Of ActionResult)
        If User.Identity.IsAuthenticated Then
            Return RedirectToAction("Index", "Manage")
        End If

        If ModelState.IsValid Then
            ' Obtenir des informations sur l'utilisateur à partir du fournisseur de connexion externe
            Dim info = Await AuthenticationManager.GetExternalLoginInfoAsync()
            If info Is Nothing Then
                Return View("ExternalLoginFailure")
            End If
            Dim userInfo = New ApplicationUser() With {
                .UserName = model.Email,
                .Email = model.Email,
                .Hometown = model.Hometown
            }
            Dim result = Await UserManager.CreateAsync(userInfo)
            If result.Succeeded Then
                result = Await UserManager.AddLoginAsync(userInfo.Id, info.Login)
                If result.Succeeded Then
                    Await SignInManager.SignInAsync(userInfo, isPersistent:=False, rememberBrowser:=False)
                    Return RedirectToLocal(returnUrl)
                End If
            End If
            AddErrors(result)
        End If

        ViewBag.ReturnUrl = returnUrl
        Return View(model)
    End Function

    '
    ' POST: /Account/LogOff
    <HttpPost>
    <ValidateAntiForgeryToken>
    Public Function LogOff() As ActionResult
        AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie)
        Return RedirectToAction("Index", "Home")
    End Function

    '
    ' GET: /Account/ExternalLoginFailure
    <AllowAnonymous>
    Public Function ExternalLoginFailure() As ActionResult
        Return View()
    End Function

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            If _userManager IsNot Nothing Then
                _userManager.Dispose()
                _userManager = Nothing
            End If
            If _signInManager IsNot Nothing Then
                _signInManager.Dispose()
                _signInManager = Nothing
            End If
        End If

        MyBase.Dispose(disposing)
    End Sub

#Region "Applications auxiliaires"
    ' Utilisé(e) pour la protection XSRF lors de l'ajout de connexions externes
    Private Const XsrfKey As String = "XsrfId"

    Private ReadOnly Property AuthenticationManager() As IAuthenticationManager
        Get
            Return HttpContext.GetOwinContext().Authentication
        End Get
    End Property

    Private Sub AddErrors(result As IdentityResult)
        For Each [error] In result.Errors
            ModelState.AddModelError("", [error])
        Next
    End Sub

    Private Function RedirectToLocal(returnUrl As String) As ActionResult
        If Url.IsLocalUrl(returnUrl) Then
            Return Redirect(returnUrl)
        End If
        Return RedirectToAction("Index", "Home")
    End Function

    Friend Class ChallengeResult
        Inherits HttpUnauthorizedResult
        Public Sub New(provider As String, redirectUri As String)
            Me.New(provider, redirectUri, Nothing)
        End Sub

        Public Sub New(provider As String, redirect As String, user As String)
            LoginProvider = provider
            RedirectUri = redirect
            UserId = user
        End Sub

        Public Property LoginProvider As String
        Public Property RedirectUri As String
        Public Property UserId As String

        Public Overrides Sub ExecuteResult(context As ControllerContext)
            Dim properties = New AuthenticationProperties() With {
                .RedirectUri = RedirectUri
            }
            If UserId IsNot Nothing Then
                properties.Dictionary(XsrfKey) = UserId
            End If
            context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider)
        End Sub
    End Class
#End Region
End Class
