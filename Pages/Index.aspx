<%@ Page Title="" Language="C#" MasterPageFile="~/PP.Master" AutoEventWireup="true" CodeBehind="Index.aspx.cs" Inherits="GTProyect.Pages.Index" EnableEventValidation="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="tittle" runat="server">Inicio</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="Style.css" />
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="body" runat="server" class="text-center">
    <div class="d-block mx-auto" style="max-width: 800px; width: 100%;">
        <br />
        <div class="mx-auto text-center">
            <h2>Palabras claves</h2>
        </div>
        <br />
        <div class="container">
            <div class="row mb-3">
                <div class="col-4">
                    <asp:Label runat="server" ID="LblFiltroPais" CssClass="form-label" Text="País:" />
                    <asp:DropDownList ID="ddlFiltroPaises" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlFiltroPaises_SelectedIndexChanged" CssClass="form-control"></asp:DropDownList>
                </div>
                <div class="col-4">
                    <asp:Label runat="server" ID="LblFiltroStatus" CssClass="form-label" Text="Estado:" />
                    <asp:DropDownList ID="ddlFiltroStatus" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlFiltroStatus_SelectedIndexChanged" CssClass="form-control">
                        <asp:ListItem Value="Todos">Todos</asp:ListItem>
                        <asp:ListItem Value="Activo">Activo</asp:ListItem>
                        <asp:ListItem Value="NoActivo">No Activo</asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="col align-self-center text-center">
                    <asp:Button runat="server" ID="BtnAdd" CssClass="btn btn-info btn-sm" Text="Add" OnClick="BtnAdd_Click" />
                </div>
            </div>
        </div>
        <br />
        <div class="container row">
            <div class="table">
                <asp:GridView runat="server" ID="keywordslist" class="table table-borderless table-hover">
                    <Columns>
                        <asp:TemplateField HeaderText="Acciones">
                            <ItemTemplate>
                                <asp:Button runat="server" Text="Modify" CssClass="btn btn-info btn-sm" ID="BtnModify" OnClick="BtnModify_Click" />
                                <asp:Button runat="server" ID="BtnDelete" CssClass="btn btn-info btn-sm" Text="Delete" OnClick="BtnDelete_Click" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="Footer" runat="server">
    <footer class="mt-auto text-center">
        <div class="container">
            <div class="row">
                <div class="col text-center">
                    <img src="Images/Logos.png" alt="Logo" class="img-fluid" />
                </div>
            </div>
        </div>
    </footer>
</asp:Content>
