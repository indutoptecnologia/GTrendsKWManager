<%@ Page Title="" Language="C#" MasterPageFile="~/PP.Master" AutoEventWireup="true" CodeBehind="Index.aspx.cs" Inherits="GTProyect.Pages.Index" %>
<asp:Content ID="Content1" ContentPlaceHolderID="tittle" runat="server">
    Inicio
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="Style.css" />
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="body" runat="server" class="text-center">
    <form runat="server" class="d-block mx-auto" style="max-width: 800px; width: 100%;">
        <br />
        <div class="mx-auto text-center">
            <h2>Palabras claves</h2>
        </div>
        <br />
        <div class="container">
            <div class="row">
                <div class="col-4">
                    <asp:Label runat="server" ID="LblFiltroPais" CssClass="form-label" Text="Filtrar por País:" />
                    <asp:DropDownList ID="ddlFiltroPaises" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlFiltroPaises_SelectedIndexChanged">
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
                        <asp:TemplateField HeaderText="Actions">
                            <ItemTemplate>
                                <!-- Cambiado de Button a HyperLink para "Modify" -->
                                <asp:Button runat="server" Text="Modify" CssClass="btn btn-info btn-sm" ID="BtnModify" OnClick="BtnModify_Click" />
                                <asp:Button runat="server" Text="Delete" CssClass="btn btn-info btn-sm" ID="BtnDelete" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </form>
</asp:Content>




