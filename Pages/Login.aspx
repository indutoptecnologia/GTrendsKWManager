<%@ Page Title="" Language="C#" MasterPageFile="~/PP.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="GTProyect.Pages.Login" %>

<asp:Content ID="Content1" ContentPlaceHolderID="tittle" runat="server">
    GT Keyword Manager
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="Style.css" />
    <script>
        function onEnter(event) {
            if (event.keyCode === 13) {
                document.getElementById('<%= BtnLogin.ClientID %>').click();
                return false;
            }
            return true;
        }
    </script>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="body" runat="server" ClientIDMode="Static">
    <div class="container">
        <div class="row justify-content-center mt-5">
            <div class="col-md-6">
                <div class="card">
                    <div class="card-body">
                        <!-- Formulario de inicio de sesion -->
                        <h3 class="card-title text-center mb-4">Inicio de Sesión</h3>

                        <div class="mb-3">
                            <label for="TxtUsername" class="form-label">Usuario:</label>
                            <asp:TextBox runat="server" ID="TxtUsername" CssClass="form-control" onkeypress="return onEnter(event)" />
                        </div>

                        <div class="mb-3">
                            <label for="TxtPassword" class="form-label">Contraseña:</label>
                            <asp:TextBox runat="server" ID="TxtPassword" TextMode="Password" CssClass="form-control" onkeypress="return onEnter(event)" />
                        </div>

                        <asp:Button runat="server" ID="BtnLogin" Text="Iniciar Sesión" OnClick="BtnLogin_Click" CssClass="btn btn-info btn-block" />

                        <asp:Label runat="server" ID="lblErrorMessage" CssClass="text-danger mt-3" Visible="false"></asp:Label>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>

