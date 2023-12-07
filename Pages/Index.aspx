<%@ Page Title="" Language="C#" MasterPageFile="~/PP.Master" AutoEventWireup="true" CodeBehind="Index.aspx.cs" Inherits="GTProyect.Pages.Index" EnableEventValidation="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="tittle" runat="server">
    GT Keyword Manager
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="Style.css" />
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="body" runat="server" class="text-center">
    <div class="d-block mx-auto" style="max-width: 800px; width: 100%;">
        <br />
        <div class="mx-auto text-center text-dark">
            <h2 class="text-dark">PALABRAS CLAVES</h2>
        </div>
        <br />
        <div class="container" id="index-container" >
            <div class="row mb-3 align-items-center" >
                <div class="col-4" >
                    <asp:Label runat="server" ID="LblFiltroPais" CssClass="form-label text-dark" Text="País:" />
                    <asp:DropDownList ID="ddlFiltroPaises" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlFiltroPaises_SelectedIndexChanged" CssClass="form-select bg-dark text-light rounded h-100"></asp:DropDownList>
                </div>
                <div class="col-4">
                    <asp:Label runat="server" ID="LblFiltroStatus" CssClass="form-label text-dark" Text="Estado:" />
                    <asp:DropDownList ID="ddlFiltroStatus" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlFiltroStatus_SelectedIndexChanged" CssClass="form-select bg-dark text-light rounded h-100">
                        <asp:ListItem Value="Todos">Todos</asp:ListItem>
                        <asp:ListItem Value="Activo">Activo</asp:ListItem>
                        <asp:ListItem Value="NoActivo">No Activo</asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="col-4 text-center">
                    <asp:Button runat="server" ID="BtnAdd" CssClass="btn btn-info btn-rounded" Text="Agregar" OnClick="BtnAdd_Click" style="margin-top: 5px;" />
                </div>
            </div>
        </div>
        <br />
        <div class="container row bg-dark p-3 rounded">
            <div class="table-responsive text-center" " style="max-height: 700px; overflow-y: auto;" >
                <asp:GridView runat="server" ID="keywordslist" CssClass="table table-borderless table-hover text-light rounded" AutoGenerateColumns="false" PageSize="10">
                    <Columns>
                        
                        <asp:BoundField DataField="Palabra Clave" HeaderText="PALABRA CLAVE" SortExpression="kw" />
                        <asp:TemplateField HeaderText="ESTADO" SortExpression="activo">
                        <ItemTemplate>
                            <asp:CheckBox ID="CheckBoxStatus" runat="server" Enabled="false" Checked='<%# Convert.ToBoolean(Eval("Status")) %>' />
                        </ItemTemplate>
                    </asp:TemplateField>
                        <asp:BoundField DataField="Pais" HeaderText="PAIS" SortExpression="nombre" />
                        <asp:TemplateField HeaderText="ACCIONES">
    <ItemTemplate>
        <asp:Button runat="server" Text="Editar" CssClass="btn btn-info btn-sm rounded" ID="BtnModify" OnClick="BtnModify_Click" />
        <asp:Button runat="server" ID="BtnDelete" CssClass="btn btn-info btn-sm rounded" Text="Estado" OnClick="BtnDelete_Click" />
    </ItemTemplate>
</asp:TemplateField>
                    </Columns>
                </asp:GridView>
                <asp:Label runat="server" ID="Label1" CssClass="text-danger" Visible="false"></asp:Label>
            </div>
        </div>
    </div>
         <footer class="mt-5 text-center bg-dark text-light rounded" style="margin-top: 65px !important;">
    <div class="container">
        <div class="row">
            <div class="col text-center">
                <img src="../Images/Logos2024.png" />
            </div>
        </div>
    </div>
</footer>
</asp:Content>




